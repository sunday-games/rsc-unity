#if IMMUNTABLE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;

using Nethereum.Hex.HexTypes;
using Nethereum.Unity.Rpc;
using Nethereum.Unity.Metamask;

namespace SG.BlockchainPlugin
{
    public class Immutable : Wallet
    {
        private string _account;
        private void AccountChanged(string account)
        {
            _account = account;
        }

        private string _chainId;
        private Blockchain _blockchain;
        private void OnChainChanged(string chainId)
        {
            _chainId = chainId;
            _blockchain = supportedBlockchains.Find(b => b.network.Id == _chainId);
        }

        public Immutable()
        {
            name = Names.Immutable;
            nameToView = "Immutable";

            supportedBlockchains = new Blockchain[] { Blockchain.ethereum, Blockchain.polygon, Blockchain.bsc, Blockchain.NeoX };

            deepUrls = new Dictionary<RuntimePlatform, string>();

            downloadUrls = new Dictionary<RuntimePlatform, string>
            {
                [RuntimePlatform.WebGLPlayer] = "https://metamask.io"
            };
        }

        public override IEnumerator Login(Blockchain requestedBlockchain, string requestedAddress = null, Action<Result> callback = null)
        {
            onProgressStart?.Invoke(Messages.LoginApprove);

            Result loginResult = null;

            if (!supportedBlockchains.Contains(requestedBlockchain))
            {
                loginResult = new Result().SetError(Errors.BlockchainUnsupported);
                onProgressEnd?.Invoke(loginResult);
                callback?.Invoke(loginResult);
                yield break;
            }

            //// BlockchainManager.Web3.Login(r => loginResult = r, AccountChanged, OnChainChanged);
            //while (loginResult == null)
            //    yield return null;

            //if (_blockchain != null)
            //{
            //    loginResult.data["chainId"] = _chainId;
            //    loginResult.data["blockchain"] = _blockchain;
            //}
            //else
            //{
            //    Log.Error($"Now MetaMask has selected a network with id {_chainId}, which is not supported");
            //    // yield return SwitchChain(requestedBlockchain);
            //}

            //if (loginResult.success && loginResult.data.TryGetString("account", out string address))
            //{
            //    AccountChanged(address);

            //    if (!requestedAddress.IsEmpty() && !address.IsEqualIgnoreCase(requestedAddress))
            //    {
            //        loginResult = new Result().SetError(Errors.AccountWrong);
            //        loginResult.errorLocalized = loginResult.error.Localize(nameToView, requestedAddress.CutMiddle(4, 4));
            //        onProgressEnd?.Invoke(loginResult);
            //        callback?.Invoke(loginResult);
            //        yield break;
            //    }

            //    foreach (var supportedBlockchain in supportedBlockchains)
            //        loginedAccounts[supportedBlockchain] = new Account(supportedBlockchain, address);
            //}

            //if (loginResult.error.IsNotEmpty())
            //    loginResult.errorLocalized = GetErrorLocalized(requestedBlockchain, loginResult.error);

            onProgressEnd?.Invoke(loginResult);

            callback?.Invoke(loginResult);
        }

        public override IEnumerator AccountGetInfo(Blockchain blockchain, string address = null, Action<Result<decimal>> callback = null)
        {
            var result = new Result();
            yield return NethereumManager.AccountUpdateBalance(blockchain,
                address ?? loginedAccounts[blockchain].Address,
                GetRequestClientFactory(), result);

            if (!result.success)
            {
                result.data["blockchain"] = blockchain;
                onError?.Invoke(result);
                callback?.Invoke(new Result<decimal>(error: result.error));
                yield break;
            }

            //onAccountInfoUpdated?.Invoke(blockchain, result);

            callback?.Invoke(new Result<decimal>(value: result.data["balance"].ToDecimal()));
        }

        //public IEnumerator GetBlockNumber(Action<Result> callback = null)
        //{
        //    var request = new EthBlockNumberUnityRequest(GetRequestClientFactory());
        //    yield return request.SendRequest();

        //    var result = new Result();
        //    if (request.Exception != null)
        //        result.SetError(ParseMessage(request.Exception.Message));
        //    else
        //        result.SetSuccess(new DictSO { ["blockNumber"] = request.Result.Value });
        //}

        public override IEnumerator MessageSign(Blockchain blockchain, string message, Action<Result> callback = null)
        {
            onProgressStart?.Invoke(Messages.MessageSignApprove);

            var request = new EthPersonalSignUnityRequest(GetRequestClientFactory());
            yield return request.SendRequest(new HexUTF8String(message));

            var result = new Result();
            if (request.Exception != null)
                result.SetError(ParseMessage(request.Exception.Message));
            else
                result.SetSuccess(new DictSO { ["signature"] = request.Result });

            onProgressEnd?.Invoke(result);

            callback?.Invoke(result);
        }

        public override IEnumerator MessageSignEIP712(Blockchain blockchain, Nethereum.ABI.EIP712.TypedData<Nethereum.ABI.EIP712.Domain> typedData, Action<Result> callback = null)
        {
            onProgressStart?.Invoke(Messages.MessageSignApprove);

            var request = new EthSignTypedDataV4UnityRequest(GetRequestClientFactory());
            yield return request.SendRequest(Nethereum.ABI.EIP712.TypedDataRawJsonConversion.ToJson(typedData));

            var result = new Result();
            if (request.Exception != null)
                result.SetError(ParseMessage(request.Exception.Message));
            else
                result.SetSuccess(new DictSO { ["signature"] = request.Result });

            onProgressEnd?.Invoke(result);

            callback?.Invoke(result);
        }

        public override IEnumerator TransactionSignAndSend(Transaction tx, Action<Transaction> callback = null)
        {
            Log.Info($"Blockchain - TransactionSignAndSend: " + tx.ToString());

            if (tx.Blockchain != _blockchain)
            {
                yield return SwitchChain(tx.Blockchain);

                if (tx.Blockchain != _blockchain)
                {
                    Log.Error($"Blockchain - TransactionSignAndSend - Wrong Network: current is {_chainId}, but {tx.Blockchain.network.Id} required)");
                    tx.error = Errors.NetworkWrong;
                    tx.SetStatus(Transaction.Status.REJECTED);
                    callback?.Invoke(tx);
                    yield break;
                }
            }

            if (_account.IsNotEmpty() && !tx.From.IsEqualIgnoreCase(_account))
            {
                Log.Error($"Blockchain - TransactionSignAndSend - Wrong Account: current is {_account}, but {tx.From} required)");
                tx.error = Errors.AccountWrong;
                tx.SetStatus(Transaction.Status.REJECTED);
                callback?.Invoke(tx);
                yield break;
            }

            onProgressStart?.Invoke(Messages.TransactionEstimate);

            //if (tx.Function.FeeModifier != null && supportedBlockchains.Contains(tx.Blockchain))
            //{
            //    yield return TransactionEstimate(tx);
            //    if (!tx.error.IsEmpty())
            //    {
            //        tx.SetStatus(Transaction.Status.REJECTED);
            //        onProgressEnd?.Invoke(tx.Blockchain, new Result().SetError(tx.error)); // TODO Move tx.error to Result
            //        callback?.Invoke(tx);
            //        yield break;
            //    }
            //}

            if (tx.gasEstimation == default)
                yield return TransactionEstimate(tx);
            tx.gasLimit = new BigInteger((decimal) tx.gasEstimation * tx.Blockchain.gasMultiplier);

            onProgressStart?.Invoke(Messages.TransactionSignApprove);

            yield return NethereumManager.TransactionSignAndSend(tx, GetTransactionRequest());

            if (tx.error.IsNotEmpty())
            {
                tx.errorLocalized = GetErrorLocalized(tx.Blockchain, tx.error);
                tx.SetStatus(Transaction.Status.REJECTED);
                onProgressEnd?.Invoke(new Result().SetError(tx.error)); // TODO Move tx.error to Result
                callback?.Invoke(tx);
                yield break;
            }

            tx.SetStatus(Transaction.Status.PENDING);
            onProgressEnd?.Invoke(new Result().SetSuccess()); // TODO Move tx.error to Result

            callback?.Invoke(tx);
        }

        public override IEnumerator TransactionEstimate(Transaction tx)
        {
            if (!supportedBlockchains.Contains(tx.Blockchain))
                throw new NotImplementedException();

            yield return NethereumManager.TransactionEstimate(tx, GetRequestClientFactory());

            // TODO Remove?
            tx.errorLocalized = GetErrorLocalized(tx.Blockchain, tx.error);
        }

        public override IEnumerator TransactionSetStatus(Transaction tx, Action<Result> callback = null)
        {
            if (!supportedBlockchains.Contains(tx.Blockchain))
                throw new NotImplementedException();

            yield return NethereumManager.TransactionSetStatus(tx, GetRequestClientFactory(), callback);
        }

        //private IEnumerator SwitchChain(Blockchain blockchain)
        //{
        //    onProgressStart?.Invoke(Messages.NetworkWrong_AddApprove(blockchain.network));

        //    var addChainRequest = new WalletAddEthereumChainUnityRequest(GetRequestClientFactory());
        //    yield return addChainRequest.SendRequest(
        //        new Nethereum.RPC.HostWallet.AddEthereumChainParameter
        //        {
        //            ChainId = new HexBigInteger(BigInteger.Parse(blockchain.network.Id)),
        //            BlockExplorerUrls = new List<string> { blockchain.network.explorer.Url },
        //            ChainName = blockchain.network.Name,
        //            NativeCurrency = new Nethereum.RPC.HostWallet.NativeCurrency()
        //            {
        //                Name = blockchain.NativeToken.Name,
        //                Symbol = blockchain.NativeToken.Name,
        //                Decimals = blockchain.NativeToken.Decimals.ToUInt(),
        //            },
        //            RpcUrls = new List<string> { blockchain.network.Node.OriginalString },
        //        });
        //}

        private IEnumerator SwitchChain(Blockchain blockchain)
        {
            onProgressStart?.Invoke(Messages.NetworkWrong_SwitchApprove(blockchain.network));

            var chainIdRequest = new WalletSwitchEthereumChainUnityRequest(GetRequestClientFactory());
            yield return chainIdRequest.SendRequest(
                new Nethereum.RPC.HostWallet.SwitchEthereumChainParameter { ChainId = new HexBigInteger(BigInteger.Parse(blockchain.network.Id)) });
            yield return new WaitForSeconds(1f);

            if (chainIdRequest.Exception != null)
            {
                Log.Error("EthChainIdUnityRequest Error " + chainIdRequest.Exception.Message);

                onProgressStart?.Invoke(Messages.NetworkWrong_AddApprove(blockchain.network));

                var addChainRequest = new WalletAddEthereumChainUnityRequest(GetRequestClientFactory());
                yield return addChainRequest.SendRequest(
                    new Nethereum.RPC.HostWallet.AddEthereumChainParameter
                    {
                        ChainId = new HexBigInteger(BigInteger.Parse(blockchain.network.Id)),
                        BlockExplorerUrls = new List<string> { blockchain.network.explorer.Url },
                        ChainName = blockchain.network.Name,
                        NativeCurrency = new Nethereum.RPC.HostWallet.NativeCurrency()
                        {
                            Name = blockchain.NativeToken.Name,
                            Symbol = blockchain.NativeToken.Name,
                            Decimals = blockchain.NativeToken.Decimals.ToUInt(),
                        },
                        RpcUrls = new List<string> { blockchain.network.Node.OriginalString },
                    });
                yield return new WaitForSeconds(1f);

                if (addChainRequest.Exception == null)
                {
                    onProgressStart?.Invoke(Messages.NetworkWrong_AddedSwitchApprove(blockchain.network));
                    chainIdRequest = new WalletSwitchEthereumChainUnityRequest(GetRequestClientFactory());
                    yield return chainIdRequest.SendRequest(
                        new Nethereum.RPC.HostWallet.SwitchEthereumChainParameter { ChainId = new HexBigInteger(BigInteger.Parse(blockchain.network.Id)) });
                    yield return new WaitForSeconds(1f);
                }
            }

            onProgressEnd?.Invoke(blockchain != _blockchain ? new Result().SetError(Errors.OperationCanceled) : new Result().SetSuccess());
        }

        private string GetErrorLocalized(Blockchain blockchain, string error)
        {
            string errorLocalized = null;

            if (error.IsNotEmpty())
            {
                if (error == Errors.NetworkWrong)
                {
                    errorLocalized = error.Localize(nameToView, blockchain.network.Name);
                    if (!(blockchain is Ethereum))
                        errorLocalized += Const.doubleLineBreak +
                            (error + "_ADD").Localize($"\nNew RPC URL = {blockchain.network.Node}\nChainID = {blockchain.network.Id}\nSymbol = {blockchain.NativeToken.Name}");
                }
                else if (error == Errors.WalletLocked || error == Errors.WalletNotInstalled)
                {
                    errorLocalized = error.Localize(nameToView);
                }
            }

            return errorLocalized;
        }

        private MetamaskRequestRpcClientFactory GetRequestClientFactory() =>
            new MetamaskRequestRpcClientFactory(_account, null, 60000);

        private MetamaskTransactionUnityRequest GetTransactionRequest() =>
            new MetamaskTransactionUnityRequest(_account, GetRequestClientFactory());

        private string ParseMessage(string message)
        {
            if (message.Contains("User denied message signature"))
                return Errors.OperationCanceled;

            return message;
        }
    }
}
#endif // METAMASK && UNITY_WEBGL