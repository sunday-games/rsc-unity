using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DictSS = System.Collections.Generic.Dictionary<string, string>;

namespace SG.BlockchainPlugin
{
    public class EOSIO : Blockchain
    {
        public class Explorer : Network.Explorer
        {
            public Explorer(string url) : base(url) { }
            public override string GetAddressURL(string address) => Url + "account/" + address;
        }

        public override void Setup()
        {
            name = Names.EOSIO;

            NativeTokenSetup(
                new Token("EOS", Token.Standarts.Native, decimals: 4),
                new SmartContract() { Blockchain = this, Address = "eosio.token", Name = "EOSIO Token" },
                new SmartContract.Function() // https://developers.eos.io/eosio-cleos/reference#cleos-transfer
                {
                    Name = "transfer",
                    Payable = true,
                    Inputs = new DictSS
                    {
                        ["sender"] = "TEXT",
                        ["recipient"] = "TEXT",
                        ["amount"] = "UINT",
                        ["memo"] = "TEXT",
                    }
                });

            var system = new SmartContract() { Blockchain = this, Address = "eosio", Name = "EOSIO" };

            stakeFunction = new SmartContract.Function()
            {
                Name = "delegatebw",
                Contract = system,
                Payable = true,
                Inputs = new DictSS
                {
                    ["from"] = "TEXT",
                    ["receiver"] = "TEXT",
                    ["stake_net_quantity"] = "TEXT",
                    ["stake_cpu_quantity"] = "TEXT",
                }
            };

            unstakeFunction = new SmartContract.Function()
            {
                Name = "undelegatebw",
                Contract = system,
                Payable = true,
                Inputs = new DictSS
                {
                    ["from"] = "TEXT",
                    ["receiver"] = "TEXT",
                    ["unstake_net_quantity"] = "TEXT",
                    ["unstake_cpu_quantity"] = "TEXT",
                }
            };

            buyramFunction = new SmartContract.Function()
            {
                Name = "buyram",
                Contract = system,
                Payable = true,
                Inputs = new DictSS { ["payer"] = "TEXT", ["receiver"] = "TEXT", ["tokens"] = "TEXT" }
            };

            sellramFunction = new SmartContract.Function()
            {
                Name = "sellram",
                Contract = system,
                Payable = true,
                Inputs = new DictSS { ["account"] = "TEXT", ["bytes"] = "UINT" }
            };

            network = Configurator.production ?
                new Network("EOS Mainnet",
                    id: "aca376f206b8fc25a6ed44dbdc66547c36c6c33e3a119ffbeaef943642f0e906",
                    blockCreationTime: TimeSpan.FromSeconds(5),
                    nodeUrls: new string[]
                    {
                        "https://api.main.alohaeos.com:443",
                        // "https://nodes.get-scatter.com:443",
                        // "https://api.eosdetroit.io:443",
                    },
                    explorer: new Explorer("https://bloks.io/"))
                :
                new Network("EOS Jungle Testnet",
                    id: "e70aaab8997e1dfce58fbfac80cbbb8fecec7b99cf982a9444273cbc64c41473",
                    blockCreationTime: TimeSpan.FromSeconds(5),
                    nodeUrls: new string[]
                    {
                        "https://api.jungle.alohaeos.com:443",
                        // "https://jungle2.cryptolions.io:443",   
                    },
                    explorer: new Explorer("https://jungle.bloks.io/"));
        }

        public IEnumerator FindAndSetAvailableNode(Network network)
        {
            var downloads = new Download[network.Nodes.Length];

            while (true)
            {
                var allDownloadsComplete = true;

                for (int i = 0; i < downloads.Length; ++i)
                    if (downloads[i] == null)
                    {
                        downloads[i] = new Download(network.Nodes[i].OriginalString + "/v1/chain/get_info")
                            .IgnoreError();

                        downloads[i].Run();

                        allDownloadsComplete = false;
                    }
                    else if (!downloads[i].completed)
                    {
                        allDownloadsComplete = false;
                    }
                    else if (downloads[i].success)
                    {
                        network.NodeIndex = i;
                        Log.Blockchain.Info($"EOSIO - {network.Node.DnsSafeHost} node is setuped");
                        yield break;
                    }

                if (allDownloadsComplete)
                    yield break;
                else
                    yield return null;
            }
        }

        public override Dictionary<string, object> GetTxDefaultExecutionFormat(Transaction tx)
        {
            if (tx.Function.Inputs.Count > 0)
                CheckTypes(tx.Function.Inputs.Values.ToArray(), tx.Content.Values.ToArray());

            var parameters = new Dictionary<string, object>();

            if (tx.FunctionIsNativeTokenTransfer)
                parameters.Add("memo", ToMemoFormat(tx.Content));
            else
                foreach (var pair in tx.Content)
                    parameters.Add(pair.Key, pair.Value);

            parameters["from"] = tx.From;
            if (!parameters.IsValue("to"))
                parameters["to"] = tx.To;
            if (tx.NativeTokenAmount > 0)
                parameters["quantity"] = CurrencyToTxFormat(tx.NativeTokenAmount);

            return parameters;
        }
        string ToMemoFormat(Dictionary<string, object> parameters)
        {
            var memo = "";
            foreach (var pair in parameters)
                memo += pair.Key + ":" + pair.Value + ",";
            return memo.Remove(memo.Length - 1, 1);
        }

        void CheckTypes(string[] functionTypes, object[] txValues)
        {
            if (functionTypes.Length != txValues.Length)
                return;

            for (int i = 0; i < functionTypes.Length; i++)
                foreach (var type in types)
                    if (functionTypes[i] == type.Key && !type.Value(txValues[i]))
                        Log.Blockchain.Error($"Transaction - Parameter #{i} most be a {type.Key}, but '{txValues[i]}' is not");
        }
        Dictionary<string, Func<object, bool>> types = new Dictionary<string, Func<object, bool>>
        {
            { "string", value => { try { Convert.ToString(value); return true; } catch { return false; } } },
            { "uint8", value => { try { Convert.ToUInt16(value); return true; } catch { return false; } } },
            { "uint16", value => { try { Convert.ToUInt16(value); return true; } catch { return false; } } },
            { "uint32", value => { try { Convert.ToUInt32(value); return true; } catch { return false; } } },
            { "uint64", value => { try { Convert.ToUInt64(value); return true; } catch { return false; } } },
            { "name", value => { try { return eosio.IsValidAddress(Convert.ToString(value)); } catch { return false; } } },
        };

        public override object CurrencyToTxFormat(decimal price)
        {
            return $"{price.ToString(Utils.GetStringFormater(NativeToken.Decimals, '0'), System.Globalization.CultureInfo.InvariantCulture)} {NativeToken.Name}";
        }

        public override decimal CurrencyFromTxFormat(object price)
        {
            return price.ToString().Replace($" {NativeToken.Name}", "").ToDecimal();
        }

        char[] addressForbiddenChars = new char[] { '6', '7', '8', '9', '0' };
        public override bool IsValidAddress(string address)
        {
            // Account Example: sundayiscool or mission.x
            return !address.IsEmpty() && address.Length <= 12 && address.IndexOfAny(addressForbiddenChars) < 0;
        }
        public override string RandomAddress()
        {
            return Utils.RandomString(12, "abcdefghijklmnopqrstuvwxyz12345");
        }

        public Transaction GetTransactionTransfer(string from, string to, decimal amount)
        {
            // TODO
            throw new NotImplementedException();
        }

        public Transaction GetTransactionStake(decimal netValue = 0, decimal cpuValue = 0, string receiver = null)
        {
            if (!loginedAccount)
                return null;

            return new Transaction(
                function: stakeFunction,
                content: new Dictionary<string, object>
                {
                    ["receiver"] = receiver ?? loginedAccount.Address,
                    ["stake_net_quantity"] = CurrencyToTxFormat(netValue),
                    ["stake_cpu_quantity"] = CurrencyToTxFormat(cpuValue),
                });
        }

        public Transaction GetTransactionUnstake(decimal netValue = 0, decimal cpuValue = 0, string receiver = null)
        {
            if (!loginedAccount)
                return null;

            return new Transaction(
                function: unstakeFunction,
                content: new Dictionary<string, object>
                {
                    ["receiver"] = receiver ?? loginedAccount.Address,
                    ["unstake_net_quantity"] = CurrencyToTxFormat(netValue),
                    ["unstake_cpu_quantity"] = CurrencyToTxFormat(cpuValue),
                });
        }

        public Transaction GetTransactionBuyRAM(decimal tokens, string receiver = null)
        {
            if (!loginedAccount)
                return null;

            return new Transaction(
                function: buyramFunction,
                content: new Dictionary<string, object>
                {
                    ["payer"] = loginedAccount.Address,
                    ["receiver"] = receiver ?? loginedAccount.Address,
                    ["tokens"] = CurrencyToTxFormat(tokens),
                });
        }

        public Transaction GetTransactionSellRAM(long bytes)
        {
            if (!loginedAccount)
                return null;

            return new Transaction(
                function: sellramFunction,
                content: new Dictionary<string, object>
                {
                    ["account"] = loginedAccount.Address,
                    ["bytes"] = bytes,
                });
        }

        public SmartContract.Function stakeFunction; // https://developers.eos.io/eosio-cleos/reference#cleos-system-delegatebw
        public SmartContract.Function unstakeFunction; // https://developers.eos.io/eosio-cleos/reference#cleos-system-undelegatebw
        public SmartContract.Function buyramFunction; // https://developers.eos.io/eosio-cleos/reference#cleos-system-buyram
        public SmartContract.Function sellramFunction; // https://developers.eos.io/eosio-cleos/reference#cleos-system-sellram
    }

    public class ResultEOS : Result
    {
        public ResultEOS(string json) : base(json) { HandleError(); }
        public ResultEOS(Dictionary<string, object> json) : base(json) { HandleError(); }

        void HandleError()
        {
            var rawError = data.IsValue("raw_error") ? data["raw_error"].ToString() : null;

            if (error.IsEmpty() && rawError.IsEmpty())
                return;

            var rawErrorDict = Json.Deserialize(rawError) as Dictionary<string, object>;
            if (rawErrorDict != null)
            {
                var outError = new Dictionary<string, string> {
                    { "error", string.Empty },
                    { "message", string.Empty },
                    { "code", string.Empty },
                    { "what", string.Empty },
                };

                ParseErrorData(rawErrorDict, new string[] { "message", "error", "code", "what" }, outError);

                var code = outError["code"].IsEmpty() ? 0 : outError["code"].ToInt();

                // if (!outError["what"].IsEmpty()) error = outError["what"].ToLower();
                if (!outError["message"].IsEmpty())
                    error = outError["message"].ToLower();
                else if (!outError["error"].IsEmpty())
                    error = outError["error"].ToLower();

                foreach (var e in outError)
                    data.Add(e.Key, e.Value);

                // EOSIO API ERROR CODE SPECIFICATION
                // https://docs.google.com/spreadsheets/d/1uHeNDLnCVygqYK-V01CFANuxUwgRkNkrmeLm9MLqu9c

                if (code > 3000000)
                {
                    switch (code)
                    {
                        case 3040005:
                            error = Wallet.Errors.TransactionExpired;
                            break;
                        case 3050003:
                            if (rawError.Contains("overdrawn balance"))
                                error = Wallet.Errors.TransactionNotEnoughTokens;
                            else if (rawError.Contains("account does not exist"))
                                error = "Account does not exist";
                            break;
                        case 3080001:
                            error = Wallet.Errors.TransactionExceededRAM;
                            break;
                        case 3080002:
                            error = Wallet.Errors.TransactionExceededNET;
                            break;
                        case 3080004:
                            error = Wallet.Errors.TransactionExceededCPU;
                            break;
                            // case 3050008: error = Wallet.Errors.???; break;
                    }

                    return;
                }
            }

            if (error != null)
            {
                if (error.Contains("user rejected") ||
                    error.Contains("user did not allow"))
                    error = Wallet.Errors.OperationCanceled;

                else if (error.Contains("scatter locked") ||
                    error.Contains("socket not available") ||
                    error.Contains("no connection") ||
                    error.Contains("unable to connect") ||
                    error.Contains("unable to read data"))
                    error = Wallet.Errors.WalletLocked;

                else if (error.Contains("object reference not set to an instance"))
                    error = Wallet.Errors.NotLoggedIn;

                else if (error.Contains("internal service error"))
                    error = Wallet.Errors.TransactionWillFail;
            }
            else
            {
                error = Wallet.Errors.Unknown;
            }
        }

        void ParseErrorData(Dictionary<string, object> where, string[] what, Dictionary<string, string> to)
        {
            foreach (var data in where)
            {
                foreach (var key in what)
                    if (data.Key.ToLower() == key && data.Value != null &&
                            (to[key].IsEmpty() || data.Value.ToString().Length > to[key].Length))
                        to[key] = data.Value.ToString();

                if (data.Value as Dictionary<string, object> != null)
                    ParseErrorData(data.Value as Dictionary<string, object>, what, to);
                else if (data.Value as List<object> != null)
                    foreach (var item in data.Value as List<object>)
                        if (item as Dictionary<string, object> != null)
                            ParseErrorData(item as Dictionary<string, object>, what, to);
            }
        }
    }
}