using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.BlockchainPlugin
{
    public class Token : Currency
    {
        public static Token USDT, USDC;

        public Standarts Standart;
        public enum Standarts { Native, ERC20, ERC721, ERC1155, Custom }
        public bool IsNFT => Standart == Standarts.ERC721 || Standart == Standarts.ERC1155;

        public Dictionary<Blockchain, SmartContract> Contracts;
        public Dictionary<Blockchain, SmartContract.Function> TransferFunction;

        public string MetaUrl;

        public Action<long, Blockchain, decimal> OnTotalSupplyUpdated;
        public decimal TotalSupplyLimit;
        public Dictionary<long, Dictionary<Blockchain, decimal>> TotalSupplyAmounts = new Dictionary<long, Dictionary<Blockchain, decimal>>();

        public decimal TotalSupplyAmount(long id = 0)
        {
            decimal totalSupplyAmount = 0;
            if (TotalSupplyAmounts.TryGetValue(id, out var TotalSupplyAmountsId))
                foreach (var totalSupply in TotalSupplyAmountsId.Values)
                    totalSupplyAmount += totalSupply;
            return totalSupplyAmount;
        }

        public string OwnerAddress;
        public string AuthorityAddress;
        public int OwnerFee;

        public Dictionary<Blockchain, Dictionary<Token, SmartContract>> UniswapPairContracts;
        public void SetUniswapContracts(ERC20View.UniswapPairData[] UniswapPairs)
        {
            if (UniswapPairs == null || UniswapPairs.Length == 0)
                return;

            UniswapPairContracts = new Dictionary<Blockchain, Dictionary<Token, SmartContract>>();
            foreach (var pair in UniswapPairs)
            {
                var blockchain = pair.BlockchainName.ToBlockchain();
                if (blockchain == null)
                    continue;

                if (!UniswapPairContracts.ContainsKey(blockchain))
                    UniswapPairContracts[blockchain] = new Dictionary<Token, SmartContract>();

                UniswapPairContracts[blockchain][pair.TokenView.Token] = new SmartContract()
                {
                    Blockchain = blockchain,
                    Address = pair.Address,
                    ABI = SmartContract.StandartABI("UniswapPair")
                };
            }
        }

        public Token(string name, Standarts standart = Standarts.Custom, int decimals = 0, string sign = null, string nameMin = null) :
            base(name, decimals, sign, nameMin)
        {
            Standart = standart;
        }

        public void SetContracts(BlockchainAddressDictionary addresses)
        {
            Contracts = new Dictionary<Blockchain, SmartContract>(addresses.Count);
            TransferFunction = new Dictionary<Blockchain, SmartContract.Function>(addresses.Count);

            Contracts = addresses.ToContracts(Standart.ToString(), Name);

            foreach (var c in Contracts)
            {
                if (Standart == Standarts.ERC20)
                    TransferFunction[c.Key] = Contracts[c.Key].Functions["transfer"];
                else if (Standart == Standarts.ERC1155)
                    TransferFunction[c.Key] = Contracts[c.Key].Functions["safeTransferFrom"];
            }

            if (!IsNFT)
                TotalSupplyAmounts[0] = new Dictionary<Blockchain, decimal>();
        }

        public SmartContract GetContract(Blockchain blockchain)
        {
            if (!Contracts.ContainsKey(blockchain))
            {
                Log.Blockchain.Error(blockchain + " is not supported by " + Name);
                return null;
            }

            return Contracts[blockchain];
        }

        public string GetAddress(Blockchain blockchain) => GetContract(blockchain)?.Address;

        public IEnumerator Transfer(Blockchain blockchain, string recipient, decimal amount, long? id = null, Action<Transaction> callback = null)
        {
            if (!Contracts.TryGetValue(blockchain, out var contract))
            {
                Log.Blockchain.Error(blockchain + " is not supported by " + Name);
                yield break;
            }

            yield return blockchain.wallet.TransactionSignAndSend(
                new TransferTransaction(this, blockchain, recipient, amount, id), callback);
        }

        public IEnumerator Transfer_AutoConfirm(Blockchain blockchain, string recipient, decimal amount, decimal feeMax, long? id = null)
        {
            if (!Contracts.TryGetValue(blockchain, out var contract))
            {
                Log.Blockchain.Error(blockchain + " is not supported by " + Name);
                yield break;
            }
#if SG_BLOCKCHAIN
            yield return (blockchain.wallet as SundayWallet).TransactionSignAndSend_AutoConfirm(
                new TransferTransaction(this, blockchain, recipient, amount, id),
                feeMax);
#endif
        }

        public IEnumerator GetBalance(Blockchain blockchain, string address, long? id = null, Action<decimal?> callback = null)
        {
            if (!Contracts.TryGetValue(blockchain, out var contract))
            {
                Log.Blockchain.Error(Wallet.Errors.BlockchainUnsupported);
                var result = Result.Error(Wallet.Errors.BlockchainUnsupported); // TODO
                callback?.Invoke(null);
                yield break;
            }

            //var startTime = UnityEngine.Time.time;
            //Log.Blockchain.Debug($"Balance of {blockchain}.{Name}[{id}] is updated: {balance} ({UnityEngine.Time.time - startTime})");

            if (Standart == Standarts.ERC20)
                yield return contract.Functions["balanceOf"].Read<BigInteger, decimal>(
                    new DictSO { ["address"] = address },
                    interimResult => FromMin(interimResult),
                    OnResult);

            else if (Standart == Standarts.ERC1155 && id.HasValue)
                yield return contract.Functions["balanceOf"].Read<BigInteger, decimal>(
                    new DictSO { ["address"] = address, ["id"] = id },
                    interimResult => interimResult.ToDecimal(),
                    OnResult);

            else if (Standart == Standarts.Native)
                yield return blockchain.wallet.AccountGetInfo(blockchain, address, OnResult);

            void OnResult(Result<decimal> result)
            {
                if (!result.Success)
                {
                    Log.Blockchain.Error($"Balance of {blockchain}.{Name}{(id.HasValue ? $"[{id}]" : "")} call fail. Error: " + result.Error);
                    callback?.Invoke(null);
                    return;
                }

                Log.Blockchain.Debug($"Balance of {blockchain}.{Name}{(id.HasValue ? $"[{id}]" : "")} is updated: {result.Value})");
                callback?.Invoke(result.Value);
            }
        }

        public IEnumerator GetTotalSupply(Blockchain blockchain, long id = 0, Action<decimal?> callback = null)
        {
            if (!Contracts.TryGetValue(blockchain, out var contract))
            {
                Log.Blockchain.Error(blockchain + " is not supported by " + Name);
                callback?.Invoke(null);
                yield break;
            }

            if (!TotalSupplyAmounts.ContainsKey(id))
                TotalSupplyAmounts[id] = new Dictionary<Blockchain, decimal>();

            if (TotalSupplyAmounts[id].TryGetValue(blockchain, out var totalSupply))
            {
                Log.Blockchain.Debug($"Total supply of {Name}[{id}] is already updated: {totalSupply}");
                callback?.Invoke(totalSupply);
                yield break;
            }

            //var startTime = UnityEngine.Time.time;
            //Log.Blockchain.Debug($"Total supply of {Name}[{id}] is updated: {totalSupply} ({UnityEngine.Time.time - startTime})");

            if (Standart == Standarts.ERC20)
                yield return contract.Functions["totalSupply"].Read<BigInteger, decimal>(
                    new DictSO(),
                    interimResult => FromMin(interimResult),
                    OnResult);

            else if (Standart == Standarts.ERC1155)
                yield return contract.Functions["totalSupply"].Read<BigInteger, decimal>(
                    new DictSO { ["id"] = id },
                    interimResult => interimResult.ToDecimal(),
                    OnResult);

            void OnResult(Result<decimal> result)
            {
                if (!result.Success)
                {
                    Log.Blockchain.Error($"Total supply of {Name}[{id}] call fail. Error: " + result.Error);
                    callback?.Invoke(null);
                    return;
                }

                TotalSupplyAmounts[id][blockchain] = result.Value;

                Log.Blockchain.Debug($"Total supply of {Name}[{id}] is updated: {result.Value}");

                callback?.Invoke(result.Value);

                OnTotalSupplyUpdated?.Invoke(id, blockchain, result.Value);
            }
        }

        public class MintTransaction : Transaction
        {
            //public const string MINTER_ADD = "MINTER_ADD"; // ERC20 addMinter(address newMinter)
            //public const string MINTER_REMOVE = "MINTER_REMOVE"; // ERC20 removeMinter(address existedMinter)
            public const string TYPE = "MINT";

            public MintTransaction(Token token, Blockchain blockchain, string recipient, decimal amount, long? id = null, string reason = null, Action<Transaction> callback = null) :
                base(TYPE, payload: new DictSO { ["tokenName"] = token.Name })
            {
                var contract = token.Contracts[blockchain];
                var amountMin = token.Standart == Standarts.ERC20 ? token.ToMin(amount) : amount.ToBigInteger();

                if (token.Standart == Standarts.ERC20)
                    SetFunction(contract.Functions["mint"],
                        new DictSO { ["recipient"] = recipient, ["amount"] = amountMin, ["reason"] = reason });

                else if (token.Standart == Standarts.ERC1155 && id.HasValue)
                    SetFunction(contract.Functions["mint"],
                        new DictSO { ["recipient"] = recipient, ["id"] = id, ["amount"] = amountMin, ["data"] = "0x" });
            }
        }
        public IEnumerator Mint(Blockchain blockchain, string recipient, decimal amount, long? id = null, string reason = null, Action<Transaction> callback = null)
        {
            if (!Contracts.TryGetValue(blockchain, out var contract))
            {
                Log.Blockchain.Error(blockchain + " is not supported by " + Name);
                yield break;
            }

            if (!blockchain.loginedAccount || !blockchain.loginedAccount.Address.IsEqualIgnoreCase(OwnerAddress))
            {
                Log.Blockchain.Error("Wrong address. Required: " + OwnerAddress);
                yield break;
            }

            yield return blockchain.wallet.TransactionSignAndSend(
                new MintTransaction(this, blockchain, recipient, amount, id, reason), callback);
        }

        public class BurnTransaction : Transaction
        {
            public const string TYPE = "BURN";

            public BurnTransaction(Token token, Blockchain blockchain, string address, decimal amount, long? id = null, string reason = null, Action<Transaction> callback = null) :
                base(TYPE, payload: new DictSO { ["tokenName"] = token.Name })
            {
                var contract = token.Contracts[blockchain];
                var amountMin = token.Standart == Standarts.ERC20 ? token.ToMin(amount) : amount.ToBigInteger();

                if (token.Standart == Standarts.ERC20)
                    SetFunction(contract.Functions["burn"],
                        new DictSO { ["amount"] = amountMin, ["reason"] = reason });

                else if (token.Standart == Standarts.ERC1155 && id.HasValue)
                    SetFunction(contract.Functions["burn"],
                        new DictSO { ["account"] = address, ["id"] = id, ["amount"] = amountMin });
            }
        }
        public IEnumerator Burn(Blockchain blockchain, decimal amount, long? id = null, string reason = null, Action<Transaction> callback = null)
        {
            if (!Contracts.TryGetValue(blockchain, out var contract))
            {
                Log.Blockchain.Error(blockchain + " is not supported by " + Name);
                yield break;
            }

            if (!blockchain.loginedAccount)
            {
                Log.Blockchain.Error("Please login first");
                yield break;
            }

            yield return blockchain.wallet.TransactionSignAndSend(
                new BurnTransaction(this, blockchain, blockchain.loginedAccount.Address, amount, id, reason), callback);
        }

        public IEnumerator GetIsApproved(Blockchain blockchain, string accountAddress, string operatorAddress, decimal amount, Action<bool> callback = null)
        {
            if (!Contracts.TryGetValue(blockchain, out var contract))
            {
                Log.Blockchain.Error(blockchain + " is not supported by " + Name);
                yield break;
            }

            bool isApproved = false;

            if (Standart == Standarts.ERC20)
                yield return contract.Functions["allowance"].Read<BigInteger, decimal>(
                    new DictSO { ["owner"] = accountAddress, ["spender"] = operatorAddress },
                    interimResult => FromMin(interimResult),
                    result => isApproved = result.Success && result.Value >= amount);

            else if (Standart == Standarts.ERC1155)
                yield return contract.Functions["isApprovedForAll"].Read<bool>(
                    new DictSO { ["account"] = accountAddress, ["operator"] = operatorAddress },
                    result => isApproved = result.Success && result.Value);

            callback?.Invoke(isApproved);
        }

        public class ApproveTransaction : Transaction
        {
            public const string TYPE = "APPROVE";

            public ApproveTransaction(Token token, Blockchain blockchain, string operatorAddress, decimal amount) :
                base(TYPE, payload: new DictSO { ["tokenName"] = token.Name })
            {
                var contract = token.Contracts[blockchain];

                if (token.Standart == Standarts.ERC20)
                    SetFunction(contract.Functions["approve"],
                        new DictSO { ["operator"] = operatorAddress, ["amount"] = token.ToMin(amount) });

                else if (token.Standart == Standarts.ERC1155)
                    SetFunction(contract.Functions["setApprovalForAll"],
                        new DictSO { ["operator"] = operatorAddress, ["approved"] = amount > 0 });
            }
        }
        public IEnumerator Approve(Blockchain blockchain, string operatorAddress, decimal amount, Action<Transaction> callback = null)
        {
            if (!Contracts.TryGetValue(blockchain, out var contract))
            {
                Log.Blockchain.Error(blockchain + "is not supported by " + Name);
                yield break;
            }

            yield return blockchain.wallet.TransactionSignAndSend(
                new ApproveTransaction(this, blockchain, operatorAddress, amount), callback);
        }

        public IEnumerator GetUniswapRate(Blockchain bc = null, Token usd = null, Action<decimal> callback = null)
        {
            var pairContracts = (Standart == Standarts.Native ? bc.NativeWrappedToken : this).UniswapPairContracts;

            if (bc == null)
                bc = pairContracts.First().Key;

            if (usd == null)
                usd = pairContracts[bc].First().Key;

            if (!pairContracts.TryGetValue(bc, out var contracts) || !contracts.TryGetValue(usd, out var contract))
            {
                Log.Blockchain.Error($"Unable to swap {Name} to {usd.Name}");
                callback?.Invoke(default);
                yield break;
            }

            decimal exchangeRate = default;
#if SG_BLOCKCHAIN
            yield return contract.Functions["getReserves"].Read(
                new DictSO(),
                interimResult => Nethereum.Contracts.FunctionOuputDTOExtensions.DecodeOutput(new Uniswap.GetReserves.Output(), interimResult),
                result =>
                {
                    if (result.Success && result.Value.ReserveB > 0)
                    {
                        exchangeRate = UsdRate = usd.FromMin(result.Value.ReserveA) / FromMin(result.Value.ReserveB);
                        Log.Info($"UniswapRate: 1 {this} = {exchangeRate.ToStringFormated()} {usd}");
                    }
                });
#endif
            callback?.Invoke(exchangeRate);
        }

        public IEnumerator GrantMinterRole(Blockchain blockchain, string minter, Action<Transaction> callback = null)
        {
            if (!Contracts.TryGetValue(blockchain, out var contract))
            {
                Log.Blockchain.Error(blockchain + " is not supported by " + Name);
                yield break;
            }

            if (!blockchain.loginedAccount || !blockchain.loginedAccount.Address.IsEqualIgnoreCase(OwnerAddress))
            {
                Log.Blockchain.Error("Wrong address. Required: " + OwnerAddress);
                yield break;
            }

            var transaction = new Transaction("ADD_MINTER");
            transaction.SetFunction(contract.Functions["addMinter"], new DictSO { ["newMinter"] = minter });

            yield return blockchain.wallet.TransactionSignAndSend(transaction, callback);
        }
    }
}