#if SG_BLOCKCHAIN
using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;

namespace SG.BlockchainPlugin
{
    public class SundayWallet : Wallet
    {
        public static class Settings
        {
            /// <summary>
            /// This mode duplicate Ethereum like accounts
            /// </summary>
            public static bool solidityAccountDuplicateMode = true;
        }

        public static SundayWalletUI.UI UI;

        public SundayWallet()
        {
            name = Names.SundayWallet;
            nameToView = "Sunday Wallet";

            supportedBlockchains = new Blockchain[] { Blockchain.ethereum, Blockchain.polygon, Blockchain.bsc, Blockchain.NeoX };

            deepUrls = new Dictionary<RuntimePlatform, string>();

            downloadUrls = new Dictionary<RuntimePlatform, string>();
        }

        public override IEnumerator Login(Blockchain blockchain, string requestedAddress = null, Action<Result> callback = null)
        {
            Result result = null;

            Account account = null;

            if (!supportedBlockchains.Contains(blockchain))
            {
                result = new Result().SetError(Errors.BlockchainUnsupported);
                callback?.Invoke(result);
                yield break;
            }

            if (requestedAddress.IsNotEmpty())
                account = Accounts.List.Find(blockchain, requestedAddress);

            if (!account)
                yield return UI.AccountChoose(blockchain, chosenAccount => account = chosenAccount);

            if (!account)
            {
                result = new Result().SetError(Errors.OperationCanceled);
                callback?.Invoke(result);
                yield break;
            }

            if (requestedAddress.IsNotEmpty() && !account.Address.IsEqualIgnoreCase(requestedAddress))
            {
                result = new Result().SetError(Errors.AccountWrong);
                callback?.Invoke(result);
                yield break;
            }

            foreach (var supportedBlockchain in supportedBlockchains)
                loginedAccounts[supportedBlockchain] = account;

            result = new Result().SetSuccess(new DictSO { ["account"] = account.Address });

            callback?.Invoke(result);
        }

        public override IEnumerator Logout(Blockchain blockchain, Action<Result> callback = null)
        {
            loginedAccounts.Remove(blockchain);

            callback?.Invoke(new Result().SetSuccess());
            yield break;
        }

        public override IEnumerator AccountGetInfo(Blockchain blockchain, string address = null, Action<Result<decimal>> callback = null)
        {
            var account = address != null ?
                Accounts.List.Find(blockchain, address) ?? new Account(blockchain, address) :
                loginedAccounts[blockchain];

            if (!supportedBlockchains.Contains(blockchain))
                throw new NotImplementedException();

            var accoutInfoResult = new Result<decimal>();
            yield return NethereumManager.AccountUpdateBalance(account, GetRequestClientFactory(blockchain.network), accoutInfoResult);

            if (!accoutInfoResult.Success)
            {
                UI.windows.error.Open(accoutInfoResult.Error);
                callback?.Invoke(accoutInfoResult);
                yield break;
            }

            //onAccountInfoUpdated?.Invoke(blockchain, accoutInfoResult);

            callback?.Invoke(accoutInfoResult);
        }

        public override IEnumerator MessageSign(Blockchain blockchain, string message, Action<Result> callback = null)
        {
            if (!supportedBlockchains.Contains(blockchain))
            {
                yield return null;
                throw new NotImplementedException();
            }

            var result = new Result();

            yield return UI.GetPrivateKey(loginedAccounts[blockchain], result);
            if (!result.success)
            {
                callback?.Invoke(result);
                yield break;
            }

            var signature = NethereumManager.Sign(message, result.data["privateKey"].ToString());
            result = new Result().SetSuccess(new DictSO { ["signature"] = signature });

            callback?.Invoke(result);
        }

        public override IEnumerator MessageSignEIP712(Blockchain blockchain, Nethereum.ABI.EIP712.TypedData<Nethereum.ABI.EIP712.Domain> typedData, Action<Result> callback = null)
        {
            if (!supportedBlockchains.Contains(blockchain))
            {
                yield return null;
                throw new NotImplementedException();
            }

            var result = new Result();

            yield return UI.GetPrivateKey(loginedAccounts[blockchain], result);
            if (!result.success)
            {
                callback?.Invoke(result);
                yield break;
            }

            var signature = NethereumManager.SignTypedData(typedData, result.data["privateKey"].ToString());
            result = new Result().SetSuccess(new DictSO { ["signature"] = signature });

            callback?.Invoke(result);
        }

        public override IEnumerator TransactionSignAndSend(Transaction tx, Action<Transaction> callback = null)
        {
            Log.Info($"Blockchain - SignAndSendTransaction: " + tx.ToString());

            var from = Accounts.List.Find(tx.Blockchain, tx.From);

            if (!supportedBlockchains.Contains(tx.Blockchain))
                throw new NotImplementedException();

            UI.loader.Show("Transaction estimation...");

            var txCountResult = new Result();
            NethereumManager.AccountUpdateTxCount(from, GetRequestClientFactory(tx.Blockchain.network), txCountResult)
                .Start();

            var nativeBalanceResult = new Result<decimal>();
            NethereumManager.AccountUpdateBalance(from, GetRequestClientFactory(tx.Blockchain.network), nativeBalanceResult)
                .Start();

            decimal? tokenBalance = null;
            Token token = null;
            if (tx.FunctionIsNativeTokenTransfer)
            {
                token = tx.Blockchain.NativeToken;
                tokenBalance = default;
            }
            else if (tx.Function.Name == "transfer")
            {
                token = BlockchainManager.GetToken(tx.Payload["tokenName"].ToString());

                token.GetBalance(tx.Blockchain, from.Address,
                    callback: balance => tokenBalance = balance)
                    .Start();
            }

            var feeResult = new Result();
            tx.Blockchain.GetFeePrice(result: feeResult)
                    .Start();

            if (tx.gasEstimation == default)
                yield return TransactionEstimate(tx);

            var deadline = Time.time + 10f;

            while (Time.time < deadline &&
                !(txCountResult.success && nativeBalanceResult.Success && feeResult.success && (token == null || tokenBalance != null)))
                yield return null;

            // TODO if Time.time > deadline when what? 

            UI.loader.Hide();

            //if (tx.error.IsNotEmpty())
            //    tx.error = null; // Ignore if tx will fail

            if (!tx.nonce.HasValue)
                tx.nonce = GetNonce(from);

            yield return UI.TransactionConfirm(
                        new DictSO
                        {
                            ["tx"] = tx,
                            ["token"] = token,
                            ["tokenBalance"] = tokenBalance != null ? tokenBalance.Value : default,
                        },
                        result =>
                        {
                            if (result == null)
                            {
                                tx.error = Errors.OperationCanceled;
                                return;
                            }

                            tx.feeMax = result.GetDecimal("feeMax");
                            tx.feeMaxPriority = result.GetDecimal("feeMaxPriority");

                            tx.gasLimit = result.GetBigInteger("gasLimit");

                            if (result.TryGetString("recipient", out var recipient))
                            {
                                if (tx.FunctionIsNativeTokenTransfer && tx.To.IsEmpty())
                                    tx.To = recipient;
                                else if (tx.Content.TryGetString("recipient", out string r) && r == TransferTransaction.PLACEHOLDER_ADDRESS)
                                    tx.Content["recipient"] = recipient;
                            }

                            if (result.TryGetDecimal("assetAmount", out var assetAmount))
                            {
                                if (tx.FunctionIsNativeTokenTransfer && tx.NativeTokenAmount == 0)
                                    tx.NativeTokenAmount = assetAmount;
                                else if (tx.Content.TryGetBigInteger("amount", out BigInteger amount) && amount == 0 && token)
                                    tx.Content["amount"] = token.ToMin(assetAmount);
                            }

                        });

            if (tx.error.IsNotEmpty())
            {
                tx.SetStatus(Transaction.Status.REJECTED);
                UI.windows.error.Open(tx.error);
                callback?.Invoke(tx);
                yield break;
            }

            var privateKeyResult = new Result();
            yield return UI.GetPrivateKey(from, privateKeyResult);
            if (!privateKeyResult.success)
            {
                tx.SetStatus(Transaction.Status.REJECTED);
                tx.error = privateKeyResult.error;
                UI.windows.error.Open(tx.error);
                callback?.Invoke(tx);
                yield break;
            }
            var txRequest = GetTransactionRequest(tx.Blockchain.network, privateKeyResult.data["privateKey"].ToString());

            UI.loader.Show("Transaction submission...");
            yield return NethereumManager.TransactionSignAndSend(tx, txRequest);
            UI.loader.Hide();

            if (tx.Hash.IsEmpty())
            {
                tx.SetStatus(Transaction.Status.REJECTED);
                UI.windows.error.Open(tx.error);
                callback?.Invoke(tx);
                yield break;
            }

            tx.SetStatus(Transaction.Status.PENDING);

            callback?.Invoke(tx);
        }

        public IEnumerator TransactionSignAndSend_AutoConfirm(Transaction tx, decimal feeMax)
        {
            Log.Info($"Blockchain - SignAndSendTransaction: " + tx.ToString());

            var from = Accounts.List.Find(tx.Blockchain, tx.From);

            if (!supportedBlockchains.Contains(tx.Blockchain))
                throw new NotImplementedException();

            UI.loader.Show("Transaction estimation...");
            yield return NethereumManager.AccountUpdateTxCount(from, GetRequestClientFactory(tx.Blockchain.network));

            yield return NethereumManager.AccountUpdateBalance(from, GetRequestClientFactory(tx.Blockchain.network));

            if (tx.gasEstimation == default)
                yield return TransactionEstimate(tx);
            tx.gasLimit = new BigInteger((decimal) tx.gasEstimation * tx.Blockchain.gasMultiplier);

            yield return tx.Blockchain.GetFeePrice();
            UI.loader.Hide();

            if (tx.error.IsNotEmpty())
            {
                tx.SetStatus(Transaction.Status.REJECTED);
                UI.windows.error.Open(tx.error);
                yield break;
            }

            if (!tx.nonce.HasValue)
                tx.nonce = GetNonce(from);

            // TransactionConfirm
            tx.feeMax = feeMax;

            var privateKeyResult = new Result();
            yield return UI.GetPrivateKey(from, privateKeyResult);
            if (!privateKeyResult.success)
            {
                tx.SetStatus(Transaction.Status.REJECTED);
                tx.error = privateKeyResult.error;
                UI.windows.error.Open(tx.error);
                yield break;
            }
            var txRequest = GetTransactionRequest(tx.Blockchain.network, privateKeyResult.data["privateKey"].ToString());

            UI.loader.Show("Transaction submission...");
            yield return NethereumManager.TransactionSignAndSend(tx, txRequest);
            UI.loader.Hide();

            if (tx.Hash.IsEmpty())
            {
                tx.SetStatus(Transaction.Status.REJECTED);
                UI.windows.error.Open(tx.error);
                yield break;
            }

            tx.SetStatus(Transaction.Status.PENDING);
        }

        public override IEnumerator TransactionSignAndReplace(Transaction tx)
        {
            Log.Info($"Blockchain - TransactionSignAndResend: " + tx.ToString());

            tx.error = null;

            var old = (tx.Hash, tx.feeMax, tx.feeMaxPriority, tx.gasLimit, tx.gasEstimation);
            var from = Accounts.List.Find(tx.Blockchain, tx.From);

            if (!supportedBlockchains.Contains(tx.Blockchain))
                throw new NotImplementedException();

            UI.loader.Show("Transaction estimation...");
            yield return NethereumManager.AccountUpdateTxCount(from, GetRequestClientFactory(tx.Blockchain.network));

            yield return NethereumManager.AccountUpdateBalance(from, GetRequestClientFactory(tx.Blockchain.network));

            if (tx.gasEstimation == default)
                yield return TransactionEstimate(tx);
            UI.loader.Hide();

            if (tx.error.IsNotEmpty())
            {
                UI.windows.error.Open(tx.error);
                yield break;
            }

            yield return UI.TransactionConfirm(
                new DictSO
                {
                    ["tx"] = tx,
                    ["replacement"] = true,
                },
                result =>
                {
                    if (result == null)
                    {
                        tx.error = Errors.OperationCanceled;
                        return;
                    }

                    // Tx still pending and user was set higher feePrice
                    if (tx.isWating && tx.progress == 0f &&
            result.TryGetDecimal("feeMax", out decimal feeMax) && feeMax > tx.feeMax &&
            result.TryGetDecimal("feeMaxPriority", out decimal feeMaxPriority))
                    {
                        tx.feeMax = feeMax;
                        tx.feeMaxPriority = feeMaxPriority;
                        tx.Hash = null;
                    }

                    tx.gasLimit = result.GetBigInteger("gasLimit");
                });

            if (tx.Hash.IsNotEmpty()) // Replacement canceled
                yield break;

            var privateKey = from.GetPrivateKey();
            if (privateKey.IsEmpty())
            {
                tx.error = "Enter PIN first";
                UI.windows.error.Open(tx.error);
                yield break;
            }
            var txRequest = GetTransactionRequest(tx.Blockchain.network, privateKey);

            UI.loader.Show("Transaction submission...");
            yield return NethereumManager.TransactionSignAndSend(tx, txRequest);
            UI.loader.Hide();

            if (tx.Hash.IsEmpty()) // // Replacement fail
            {
                tx.feeMax = old.feeMax;
                tx.feeMaxPriority = old.feeMaxPriority;
                tx.gasLimit = old.gasLimit;
                tx.gasEstimation = old.gasEstimation;
                tx.Hash = old.Hash;

                UI.windows.error.Open(tx.error);
                yield break;
            }

            tx.Replace(old.Hash);
        }

        public override IEnumerator TransactionEstimate(Transaction tx)
        {
            if (!supportedBlockchains.Contains(tx.Blockchain))
                throw new NotImplementedException();

            yield return NethereumManager.TransactionEstimate(tx, GetRequestClientFactory(tx.Blockchain.network));
        }

        public override IEnumerator TransactionSetStatus(Transaction tx, Action<Result> callback = null)
        {
            if (!supportedBlockchains.Contains(tx.Blockchain))
                throw new NotImplementedException();

            yield return NethereumManager.TransactionSetStatus(tx, GetRequestClientFactory(tx.Blockchain.network), callback);
        }

        static ulong? GetNonce(Account account)
        {
            var lastTx = TransactionManager.transactions.GetLastByNonce(account.Address);
            if (lastTx)
                return account.TransactionCount > lastTx.nonce + 1 ? account.TransactionCount : lastTx.nonce + 1;

            return account.TransactionCount;
        }

        private static Nethereum.Unity.Rpc.TransactionSignedUnityRequest GetTransactionRequest(Blockchain.Network network, string privateKey) =>
            new Nethereum.Unity.Rpc.TransactionSignedUnityRequest(network.Node.OriginalString, privateKey, BigInteger.Parse(network.Id));

        private static Nethereum.Unity.Rpc.UnityWebRequestRpcClientFactory GetRequestClientFactory(Blockchain.Network network) =>
            new Nethereum.Unity.Rpc.UnityWebRequestRpcClientFactory(network.Node.OriginalString);
    }
}
#endif