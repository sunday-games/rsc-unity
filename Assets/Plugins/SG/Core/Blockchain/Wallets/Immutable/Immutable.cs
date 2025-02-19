#if IMMUNTABLE
using System;
using System.Numerics;
using System.Globalization;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using Immutable.Passport;
using Immutable.Passport.Model;
using Model = Immutable.Passport.Model;
using Cysharp.Threading.Tasks;

namespace SG.BlockchainPlugin
{
    public class Immutable : Wallet
    {
        private static string CLIENT_ID, REDIRECT_URL, LOGOUT_REDIRECT_URL;
        public static void Setup(string clientId, string redirectUrl = null, string logoutRedirectUrl = null)
        {
            CLIENT_ID = clientId;
            REDIRECT_URL = redirectUrl ?? "test://callback";
            LOGOUT_REDIRECT_URL = logoutRedirectUrl ?? "test://logout";
        }

        private string _environment => Configurator.production ? Model.Environment.PRODUCTION : Model.Environment.SANDBOX;

        private Passport _passport;
        private List<string> _accounts;

        public Immutable()
        {
            name = Names.Immutable;
            nameToView = "Immutable";

            supportedBlockchains = new Blockchain[] { Blockchain.Immutable };

            deepUrls = new Dictionary<RuntimePlatform, string>();

            downloadUrls = new Dictionary<RuntimePlatform, string>();
        }

        public override IEnumerator Login(Blockchain requestedBlockchain, string requestedAddress = null, Action<Result> callback = null)
        {
            onProgressStart?.Invoke(Messages.LoginApprove);

            Result result = null;

            if (!supportedBlockchains.Contains(requestedBlockchain))
            {
                result = Result.Error(Errors.BlockchainUnsupported);
                onProgressEnd?.Invoke(result);
                callback?.Invoke(result);
                yield break;
            }

            yield return Login().ToCoroutine(r => result = r);

            if (result.success &&
                result.data.TryGetString("account", out string address) &&
                result.data.TryGetString("email", out string email))
            {
                if (!requestedAddress.IsEmpty() && !address.IsEqualIgnoreCase(requestedAddress))
                {
                    result = Result.Error(Errors.AccountWrong);
                    result.errorLocalized = result.error.Localize(nameToView, requestedAddress.CutMiddle(4, 4));
                    onProgressEnd?.Invoke(result);
                    callback?.Invoke(result);
                    yield break;
                }

                foreach (var supportedBlockchain in supportedBlockchains)
                    loginedAccounts[supportedBlockchain] = new Account(supportedBlockchain, address);
            }

            onProgressEnd?.Invoke(result);

            callback?.Invoke(result);
        }

        public override IEnumerator AccountGetInfo(Blockchain blockchain, string address = null, Action<Result<decimal>> callback = null)
        {
            yield return GetBalance(address ?? loginedAccounts[blockchain].Address).ToCoroutine(result =>
            {
                //if (!result.Success)
                //    onError?.Invoke(Result.Error(result.Error));

                //onAccountInfoUpdated?.Invoke(blockchain, result);

                callback?.Invoke(result);
            });
        }

        public override IEnumerator MessageSign(Blockchain blockchain, string message, Action<Result> callback = null)
        {
            onProgressStart?.Invoke(Messages.MessageSignApprove);

            yield return SignTypedData(message).ToCoroutine(result =>
            {
                onProgressEnd?.Invoke(result);

                callback?.Invoke(result);
            });
        }

        public override IEnumerator MessageSignEIP712(Blockchain blockchain, Nethereum.ABI.EIP712.TypedData<Nethereum.ABI.EIP712.Domain> typedData, Action<Result> callback = null)
        {
            onProgressStart?.Invoke(Messages.MessageSignApprove);

            yield return SignTypedData(Nethereum.ABI.EIP712.TypedDataRawJsonConversion.ToJson(typedData)).ToCoroutine(result =>
            {
                onProgressEnd?.Invoke(result);

                callback?.Invoke(result);
            });
        }

        public override IEnumerator TransactionSignAndSend(Transaction tx, Action<Transaction> callback = null)
        {
            Log.Info($"Blockchain - TransactionSignAndSend: " + tx.ToString());

            if (!supportedBlockchains.Contains(tx.Blockchain))
            {
                Log.Error($"Blockchain - TransactionSignAndSend - Wrong Network: current is {supportedBlockchains[0]}, but {tx.Blockchain.network.Id} required)");
                tx.error = Errors.NetworkWrong;
                tx.SetStatus(Transaction.Status.REJECTED);
                callback?.Invoke(tx);
                yield break;
            }

            if (_accounts.Count == 0 || !_accounts.Contains(tx.From))
            {
                Log.Error($"Blockchain - TransactionSignAndSend - Wrong Account: current is {(_accounts.Count > 0 ? _accounts[0] : "null")}, but {tx.From} required)");
                tx.error = Errors.AccountWrong;
                tx.SetStatus(Transaction.Status.REJECTED);
                callback?.Invoke(tx);
                yield break;
            }

            //onProgressStart?.Invoke(Messages.TransactionEstimate);

            //if (tx.gasEstimation == default)
            //    yield return TransactionEstimate(tx);
            //tx.gasLimit = new BigInteger((decimal)tx.gasEstimation * tx.Blockchain.gasMultiplier);

            onProgressStart?.Invoke(Messages.TransactionSignApprove);

            yield return SendTransaction(tx).ToCoroutine();

            if (tx.error.IsNotEmpty())
            {
                tx.SetStatus(Transaction.Status.REJECTED);
                onProgressEnd?.Invoke(Result.Error(tx.error)); // TODO Move tx.error to Result
                callback?.Invoke(tx);
                yield break;
            }

            tx.SetStatus(Transaction.Status.PENDING);
            onProgressEnd?.Invoke(Result.Success()); // TODO Move tx.error to Result

            callback?.Invoke(tx);
        }

        //public override IEnumerator TransactionEstimate(Transaction tx)
        //{
        //    if (!supportedBlockchains.Contains(tx.Blockchain))
        //        throw new NotImplementedException();

        //    yield return NethereumManager.TransactionEstimate(tx, GetRequestClientFactory());

        //    // TODO Remove?
        //    tx.errorLocalized = GetErrorLocalized(tx.Blockchain, tx.error);
        //}

        public override IEnumerator TransactionSetStatus(Transaction tx, Action<Result> callback = null)
        {
            if (!supportedBlockchains.Contains(tx.Blockchain))
                throw new NotImplementedException();

            yield return GetTxStatus(tx).ToCoroutine(callback);
        }

        public override IEnumerator Logout(Blockchain blockchain, Action<Result> callback = null)
        {
            onProgressStart?.Invoke(Messages.LoginApprove); // TODO

            yield return Logout().ToCoroutine(result =>
            {
                onProgressEnd?.Invoke(result);

                callback?.Invoke(result);
            });
        }

        private async UniTask<Result> Init()
        {
            if (_passport != null)
            {
                Log.Blockchain.Warning("Blockchain - Immutable - Fail to Init because already inited");
                return Result.Success();
            }

            if (CLIENT_ID == null)
            {
                Log.Blockchain.Error("Blockchain - Immutable - Fail to Init because CLIENT_ID = null. Please Call Setup first");
                return Result.Error("CLIENT_ID = null");
            }

            // Passport.LogLevel = Immutable.Passport.Core.Logging.LogLevel.Info;

            _passport = await Passport.Init(CLIENT_ID, _environment, REDIRECT_URL, LOGOUT_REDIRECT_URL);
            Log.Blockchain.Info("Blockchain - Immutable - Init: " + (_passport != null));
            return Result.Success();
        }

        private bool IsPKCESupported()
        {
#if (UNITY_ANDROID && !UNITY_EDITOR_WIN) || (UNITY_IPHONE && !UNITY_EDITOR_WIN) || UNITY_STANDALONE_OSX || UNITY_WEBGL
            return true;
#else
            return false;
#endif
        }

        private async UniTask<Result> Login()
        {
            if (_passport == null)
            {
                var initResult = await Init();

                if (!initResult.success)
                    return initResult;
            }

            if (await _passport.HasCredentialsSaved())
            {
                Log.Blockchain.Info($"Blockchain - Immutable - Login(useCachedSession: true)");
                await _passport.Login(useCachedSession: true);
            }
            else
            {
                try
                {
                    if (IsPKCESupported())
                    {
                        Log.Blockchain.Info($"Blockchain - Immutable - LoginPKCE()");
                        await _passport.LoginPKCE();
                    }
                    else
                    {
                        Log.Blockchain.Info($"Blockchain - Immutable - Login(useCachedSession: false)");
                        await _passport.Login(useCachedSession: false);
                    }

                }
                catch (OperationCanceledException)
                {
                    return Result.Error(Errors.OperationCanceled);
                    ;
                }
                catch (Exception ex)
                {
                    await Logout();
                    return Result.Error(ex.Message);
                }
            }

            var email = await _passport.GetEmail();
            Log.Blockchain.Info($"Blockchain - Immutable - GetEmail: {email}");

            await _passport.ConnectEvm();

            _accounts = await _passport.ZkEvmRequestAccounts();
            Log.Blockchain.Info("Blockchain - Immutable - ZkEvmRequestAccounts: " + (_accounts.Count > 0 ? string.Join(", ", _accounts) : "No accounts found"));

            if (_accounts == null || _accounts.Count == 0)
            {
                // TODO
                return Result.Error("No accounts found");
            }

            return Result.Success(new DictSO { ["email"] = email, ["account"] = _accounts[0] });
        }

        private async UniTask<Result> Logout()
        {
            try
            {
                if (IsPKCESupported())
                {
                    Log.Blockchain.Info($"Blockchain - Immutable - LogoutPKCE()");
                    await _passport.LogoutPKCE();
                    return Result.Success();
                }
                else
                {
                    Log.Blockchain.Info($"Blockchain - Immutable - Logout()");
                    await _passport.Logout();
                    return Result.Success();
                }
            }
            catch (Exception ex)
            {
                return Result.Error(ex.Message);
            }
        }

        private async UniTask<Result<decimal>> GetBalance(string address)
        {
            try
            {
                string balanceHex = await _passport.ZkEvmGetBalance(address);

                var balance = BigInteger.Parse(balanceHex.Replace("0x", ""), NumberStyles.HexNumber);
                if (balance < 0)
                    balance = BigInteger.Parse("0" + balanceHex.Replace("0x", ""), NumberStyles.HexNumber);



                return new Result<decimal>(Blockchain.Immutable.NativeToken.FromMin(balance));
            }
            catch (Exception ex)
            {
                Log.Blockchain.Error("Blockchain - Immutable - GetBalance Error: " + ex.Message);
                return new Result<decimal>(ex.Message);
            }
        }

        private async UniTask<Result> SignTypedData(string data)
        {
            try
            {
                string signature = await _passport.ZkEvmSignTypedDataV4(data);
                return Result.Success(new DictSO { ["signature"] = signature });
            }
            catch (Exception ex)
            {
                Log.Blockchain.Error($"Failed to retrieve transaction receipt: {ex.Message}");
                return Result.Error(ex.Message);
            }
        }

        private async UniTask<Transaction> SendTransaction(Transaction tx)
        {
            try
            {
                var input = tx.GetInput();
                var request = new TransactionRequest
                {
                    to = input.To,
                    value = input.Value.ToString(),
                    data = input.Data
                };

                //var response = await _passport.ZkEvmSendTransactionWithConfirmation(request);
                //tx.Hash = response.transactionHash;
                //tx.SetStatus(await GetTxStatus(tx));
                // Log.Blockchain.Info($"Blockchain - Immutable - Transaction hash: {tx.Hash}\nStatus: {tx.status}");

                tx.Hash = await _passport.ZkEvmSendTransaction(request);
                Log.Blockchain.Info($"Blockchain - Immutable - Transaction hash: {tx.Hash}");
            }
            catch (Exception ex)
            {
                Log.Blockchain.Error($"Failed to send transaction: {ex.Message}");
                tx.error = ex.Message;
            }

            return tx;
        }

        private async UniTask<Result> GetTxStatus(Transaction tx)
        {
            tx.status = ParseStarus(await PollStatus(tx.Hash));

            return Result.Success(new DictSO { ["status"] = tx.status.ToString() });

            Transaction.Status ParseStarus(string status)
            {
                switch (status)
                {
                    case "1":
                    case "0x1": return Transaction.Status.SUCCESS;
                    case "0":
                    case "0x0": return Transaction.Status.FAIL;
                    case null: return Transaction.Status.PENDING;
                    default: return Transaction.Status.UNKNOWN;
                }
            }
        }

        private static async UniTask<string> PollStatus(string txHash)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var response = await Passport.Instance.ZkEvmGetTransactionReceipt(txHash);
                    if (response.status == null)
                    {
                        // The transaction is still being processed, poll for status again
                        await UniTask.Delay(delayTimeSpan: TimeSpan.FromSeconds(1), cancellationToken: cancellationTokenSource.Token);
                    }
                    else
                    {
                        return response.status;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Task was canceled due to timeout
            }

            return null; // Timeout or could not get transaction receipt
        }
    }
}
#endif // IMMUNTABLE