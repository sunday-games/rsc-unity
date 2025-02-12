using System;
using System.Collections.Generic;
using System.Numerics;

namespace SG.BlockchainPlugin
{
    using DictSO = Dictionary<string, object>;
    using ListO = List<object>;
    using DictSF = Dictionary<string, SmartContract.Function>;
    using ListSS = List<(string type, string functionName)>;

    public class Transaction : IDictionarizable<Transaction>
    {
        public static Action<string, Transaction> OnReplaced;
        public static Action<Transaction> OnProgressUpdated;
        public static Action<Transaction> OnStatusUpdated;
        public static Action<Transaction> OnSyncRequired;

        public static Dictionary<Blockchain, DictSF> Functions = new Dictionary<Blockchain, DictSF>();
        public static void SetupTypes(SmartContract[] contractsFromABI, Dictionary<Blockchain, Dictionary<string, ListSS>> blockchainContractTypeFunctions)
        {
            if (contractsFromABI == null)
                return;

            var contractsByBlockchain = new Dictionary<Blockchain, Dictionary<string, SmartContract>>();
            foreach (var contract in contractsFromABI)
            {
                if (!contractsByBlockchain.ContainsKey(contract.Blockchain))
                    contractsByBlockchain.Add(contract.Blockchain, new Dictionary<string, SmartContract>());

                if (blockchainContractTypeFunctions.ContainsKey(contract.Blockchain) && blockchainContractTypeFunctions[contract.Blockchain].ContainsKey(contract.Name))
                    contractsByBlockchain[contract.Blockchain].Add(contract.Name, contract);
            }

            foreach (var blockchainContractTypeFunction in blockchainContractTypeFunctions)
            {
                var blockchain = blockchainContractTypeFunction.Key;

                if (!contractsByBlockchain.TryGetValue(blockchain, out var contracts))
                {
                    Log.Blockchain.Error("Transaction - SetupTypes - Cant find " + blockchain);
                    continue;
                }

                if (!Functions.ContainsKey(blockchain))
                    Functions.Add(blockchain, new DictSF());

                foreach (var contractTypeFunction in blockchainContractTypeFunction.Value)
                    foreach (var item in contractTypeFunction.Value)
                    {
                        if (!contracts.TryGetValue(contractTypeFunction.Key, out var contract))
                        {
                            Log.Blockchain.Warning($"Transaction - SetupTypes - Cant find {blockchain}.{contractTypeFunction.Key}");
                            continue;
                        }

                        //if ((blockchain == Blockchain.eosio || blockchain == Blockchain.wax) &&
                        //    item.functionName == blockchain.NativeToken.TransferFunction.Name)
                        //{
                        //    Functions[blockchain][item.type] = new SmartContract.Function(contract, blockchain.NativeToken.TransferFunction);
                        //    continue;
                        //}

                        if (!contract.Functions.TryGetValue(item.functionName, out var function))
                        {
                            Log.Blockchain.Warning($"Transaction - SetupTypes - Error mapping: {blockchain}.{contract.Name}.{item.functionName}");
                            continue;
                        }

                        Functions[blockchain][item.type] = function;
                    }
            }
        }

        public string Type;
        public Blockchain Blockchain;
        public string From;
        public string To;
        public decimal NativeTokenAmount; // "value"
        public DictSO Content; // Function's input parameters
        public DictSO Payload; // Developer's custom data

#if SG_BLOCKCHAIN
        public Nethereum.Contracts.TransactionHandlers.IFunction FunctionMessage;
#endif

        private SmartContract.Function _function;
        public SmartContract.Function Function
        {
            get
            {
                if (!_function && !Type.IsEmpty())
                {
                    if (Functions.ContainsKey(Blockchain) && Functions[Blockchain].ContainsKey(Type))
                        _function = Functions[Blockchain][Type];
                    else if (Type == TransferTransaction.TYPE && Payload.TryGetString("tokenName", out var tokenName) &&
                        tokenName == Blockchain.NativeToken.Name)
                        _function = Blockchain.NativeToken.TransferFunction[Blockchain];
                }

                return _function;
            }
            set
            {
                _function = value;

                if (_function != null)
                {
                    if (_function.Inputs != null && Content != null && _function.Inputs.Count != Content.Count)
                        Log.Blockchain.Warning($"Transaction - Wrong number of parameters. It's does not match what {_function.ToString()} function expects");

                    if ((!_function.Payable && NativeTokenAmount > 0) || (_function.Payable && NativeTokenAmount <= 0))
                        Log.Blockchain.Warning($"Transaction - Wrong money amount ({(_function.Payable ? "must be greater than zero" : "must be zero")})");
                }
            }
        }
        public bool FunctionIsNativeTokenTransfer => Function == Blockchain.NativeToken.TransferFunction[Blockchain];

        public DateTime Date;
        public string Signed;
        public string Hash;
        public ulong? BlockNumber;

        public ulong BlockConfirmations { get; protected set; }
        public void SetBlockConfirmations(ulong blockConfirmations)
        {
            if (blockConfirmations <= this.BlockConfirmations)
                return;

            this.BlockConfirmations = blockConfirmations;
            Log.Blockchain.Debug($"Transaction '{Hash}' - Progress: {(int) (progress * 100f)}%");
            OnProgressUpdated?.Invoke(this);
        }

        public ulong? nonce = null;
        public decimal feeMax;
        public decimal feeMaxPriority;
        public BigInteger gasLimit;
        public BigInteger gasEstimation;
        //public Transaction SetFee(ulong gasLimit)
        //{
        //    this.gasLimit = Function.FeeModifier != null ? Function.FeeModifier(gasLimit) : gasLimit;
        //    return this;
        //}

        public Status status;
        public enum Status
        {
            // UNKNOWN >> SERVER_NOTIFY_REQUIRED (NEO) >> REJECTED or PENDING >> TIMEOUT or FAIL or SUCCESS >> SUCCESS_SYNCED
            UNKNOWN,                // Just created
            REJECTED,               // Rejected by user
            PENDING,                // Pending in blockchain
            TIMEOUT,                // Still pending in blockchain, but timeout
            FAIL,                   // Fail in blockchain
            SUCCESS,                // Success in blockchain
            SUCCESS_SYNCED,         // Blockchain and server synced
        }
        public DateTime statusLastUpdated { get; private set; }
        public void SetStatus(Status status, bool forceEvent = false)
        {
            statusLastUpdated = DateTime.UtcNow;

            if (status == Status.UNKNOWN)
                return;

            if (this.status == status && !forceEvent)
                return;

            Log.Blockchain.Debug($"Transaction '{Hash}' - Status: {this.status} >> {status}");

            this.status = status;

            OnStatusUpdated?.Invoke(this);
        }

        public string error;
        public string errorLocalized;
        public DictSO errorData;

        public DictSO executionFormat => Blockchain.GetTxDefaultExecutionFormat(this);

        public Transaction(string type, DictSO payload = null)
        {
            Type = type;
            Payload = payload ?? new DictSO();
            Date = DateTime.UtcNow;
            status = Status.UNKNOWN;
        }
        public void SetFunction(SmartContract.Function function, DictSO content = null, string from = null)
        {
            Blockchain = function.Contract.Blockchain;
            Function = function;

            From = from ?? (Blockchain.loginedAccount ? Blockchain.loginedAccount.Address : null);
            To = function.Contract.Address;
            Content = content ?? new DictSO();
        }

        public Transaction(SmartContract.Function function, string type = null,
            string from = null, string to = null, decimal money = 0, DictSO content = null,
            DictSO payload = null, DateTime date = default, Status status = Status.UNKNOWN, string hash = null)
        {
            Blockchain = function.Contract.Blockchain;
            Function = function;

            Type = type ?? $"{function.Contract.Name}.{function.Name}";
            From = from ?? (Blockchain.loginedAccount ? Blockchain.loginedAccount.Address : null);
            To = to ?? function.Contract.Address;
            NativeTokenAmount = money;
            Content = content ?? new DictSO();
            Payload = payload ?? new DictSO();
            Date = date != default ? date : DateTime.UtcNow;
            this.status = status;
            Hash = hash;
        }

        public Transaction(string type, Blockchain blockchain, DictSO content = null) :
            this(Functions[blockchain][type], type, content: content)
        { }

        protected Transaction() { }
        public Transaction FromDictionary(DictSO data)
        {
            if (data.TryGetInt("bcId", out int bcId))
                Blockchain = Blockchain.Deserialize(bcId);
            else if (data.TryGetString("blockchain", out string blockchainName))
                Blockchain = Blockchain.Deserialize(blockchainName);
            else
                Blockchain = Blockchain.current;
            if (!Blockchain)
                return null;

            Type = data.GetString("type");
            From = data.GetString("from");
            To = data.GetString("to");
            NativeTokenAmount = data.GetDecimal("money");
            Content = data.IsValue("content") ? Json.Deserialize(data["content"].ToString()) as DictSO : new DictSO();
            Payload = data.IsValue("payload") ? Json.Deserialize(data["payload"].ToString()) as DictSO : new DictSO();
            Date = data.GetDateTime("date");
            status = data.GetEnum<Status>("status");
            Hash = data.GetString("hash");
            BlockNumber = data.GetULongNullable("blockNumber");
            BlockConfirmations = data.GetULong("blockConfirmations");
            nonce = data.GetULongNullable("nonce");
            if (nonce == ulong.MaxValue)
                nonce = null; // For compatibility with the old version
            feeMax = data.ContainsKey("feePrice") ? data.GetDecimal("feePrice") : data.GetDecimal("feeMax");
            feeMaxPriority = data.GetDecimal("feeMaxPriority");
            gasLimit = data.GetBigInteger("fee");
            gasEstimation = data.GetBigInteger("feeEstimation");

            if (data.TryGetString("functionName", out var functionName))
            {
                if (Uniswap.SwapTransaction.FunctionNames.Contains(functionName))
                    Function = Uniswap.SwapTransaction.GetFunction(Blockchain, Payload.GetString("tokenName"));
#if SG_BLOCKCHAIN
                else if (functionName == Seaport.FulfillBasicOrderTransaction.FUNCTION_NAME)
                    Function = Seaport.FulfillBasicOrderTransaction.GetFunction(Blockchain);
#endif
                else if (Payload.TryGetString("tokenName", out var tokenName) &&
                    BlockchainManager.GetToken(tokenName).Contracts.TryGetValue(Blockchain, out var contract) &&
                    contract.Functions.ContainsKey(functionName))
                    Function = contract.Functions[functionName];
            }

            return this;
        }
        public DictSO ToDictionary()
        {
            return new DictSO
            {
                ["bcId"] = Blockchain.id,
                ["type"] = Type,
                ["from"] = From,
                ["to"] = To,
                ["money"] = NativeTokenAmount,
                ["content"] = Json.Serialize(Content),
                ["payload"] = Json.Serialize(Payload),
                ["date"] = Date.ToTimestamp(),
                ["status"] = (int) status,
                ["hash"] = Hash,
                ["blockNumber"] = BlockNumber,
                ["blockConfirmations"] = BlockConfirmations,
                ["nonce"] = nonce,
                ["feeMax"] = feeMax,
                ["feeMaxPriority"] = feeMaxPriority,
                ["fee"] = gasLimit,
                ["feeEstimation"] = gasEstimation,
                ["functionName"] = Function != null ? Function.Name : null,
            };
        }

        public string Url => Blockchain.network.explorer.GetTxURL(Hash);
        public void OpenUrl() => UI.Helpers.OpenLink(Url);

        public void Replace(string oldHash)
        {
            OnReplaced?.Invoke(oldHash, this);
        }

        static TimeSpan timeout = TimeSpan.FromHours(1);
        public TimeSpan age => DateTime.UtcNow - Date;
        public bool isTimeout => age > timeout;
        public bool isPending => status == Status.PENDING;
        public bool isWating => isPending || status == Status.SUCCESS;
        public string hashShort => !Hash.IsEmpty() ? Hash.CutMiddle(4, 4) : Hash;
        public float progress
        {
            get
            {
                var value = BlockConfirmations / (float) Blockchain.network.RequiredBlockConfirmations;
                return value > 1f ? 1f : value;
            }
        }

        public static implicit operator bool(Transaction tx) => tx != null;

        public override string ToString() => $"network={(Blockchain.network != null ? Blockchain.network.Id : "none")}, parameters={(Content != null ? Json.Serialize(Content) : "none")}, data={(Function != null ? Json.Serialize(executionFormat) : "none")}";

        public string FunctionToString()
        {
            var values = "";
            foreach (var c in Content)
            {
                var v = c.Value.ToString();
                values += (v.Length > 20 ? v.CutMiddle(4, 4) : v) + ", ";
            }

            if (Content.Count > 0)
                values = values.Substring(0, values.Length - 2);

            return (Function ? Function.Name : "unknown") + "(" + values + ")";
        }

        public string GetLocalizedStatus()
        {
            switch (status)
            {
                case Status.FAIL:
                    return "fail".Localize();
                case Status.PENDING:
                    return "pending".Localize() + "...";
                case Status.SUCCESS:
                    return "sync".Localize() + "...";
                case Status.SUCCESS_SYNCED:
                    return "completed".Localize();
                case Status.TIMEOUT:
                    return "timeout".Localize();
                default:
                    return null;
            }
        }
    }

    public static class TransactionExtensions
    {
        public static Transaction GetLastByNonce(this List<Transaction> transactions, string address)
        {
            if (transactions == null || transactions.Count == 0)
                return null;

            Transaction lastTx = null;

            foreach (var tx in transactions)
                if (tx.isPending && tx.nonce.HasValue && tx.From == address && (!lastTx || lastTx.nonce < tx.nonce))
                    lastTx = tx;

            return lastTx;
        }

        public static void SortByDate(this List<Transaction> transactions)
        {
            transactions.Sort((x, y) => y.Date.CompareTo(x.Date));
        }
    }
}