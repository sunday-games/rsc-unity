using System;
using System.Collections;
using System.Collections.Generic;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;
using SG.BlockchainPlugin;

namespace SG
{
    public static class SundayServer
    {
        private static AppInfo appInfo => Configurator.Instance.appInfo;

        private static Blockchain[] _supportedBlockchains;
        private static Blockchain[] supportedBlockchains => _supportedBlockchains != null ?
            _supportedBlockchains :
            _supportedBlockchains = Blockchain.current.wallet.supportedBlockchains;

        public static IEnumerator AbiGet(bool cache = false, Action<SmartContract[]> callback = null)
        {
            var abiFileName = $"{appInfo.name}_abi_{(Configurator.production ? "prod" : "test")}";

            var abi = string.Empty;

            if (cache)
            {
                var file = Resources.Load<TextAsset>(abiFileName);
                if (file)
                    abi = file.text;

                if (abi.IsEmpty() && PlayerPrefs.HasKey(abiFileName))
                    abi = PlayerPrefs.GetString(abiFileName);

                if (!abi.IsEmpty())
                {
                    callback?.Invoke(Json.Deserialize(abi).ToClassArray<SmartContract>());
                    yield break;
                }
            }

            var download = new Download(appInfo.blockchainServer + "/api/contract/actual");

            yield return download.RequestCoroutine();

            if (!download.success)
            {
                callback?.Invoke(null);
                yield break;
            }

            abi = download.responseText;

            if (cache)
            {
#if UNITY_EDITOR
                if (Utils.IsPlatformEditor())
                    Utils.SaveToFile(Configurator.resourcesPath + abiFileName + ".json", abi);
                else
#endif
                    PlayerPrefs.SetString(abiFileName, abi);
            }

            callback?.Invoke(Json.Deserialize(abi).ToClassArray<SmartContract>());
        }

        public static IEnumerator TransactionEstimate(Transaction tx, Action<Result> callback = null)
        {
            Result result;

            if (!supportedBlockchains.Contains(tx.Blockchain))
            {
                tx.error = Wallet.Errors.FeatureDontSupported;
                tx.SetStatus(Transaction.Status.REJECTED);
                callback?.Invoke(new Result().SetError(tx.error));
                yield break;
            }

            var download = new Download(appInfo.blockchainServer + $"/api/blockchain/{GetBC(tx.Blockchain)}/transaction/estimate",
                tx.executionFormat);

            yield return download.RequestCoroutine();

            if (!download.success)
            {
                tx.error = download.errorMessage;
                tx.SetStatus(Transaction.Status.REJECTED);
                callback?.Invoke(new Result().SetError(tx.error));
                yield break;
            }

            result = new Result(download.responseDict);

            if (!result.success)
            {
                tx.error = result.error;
                tx.SetStatus(Transaction.Status.REJECTED);
                callback?.Invoke(result);
                yield break;
            }

            tx.gasEstimation = result.data["limit"].ToULong();

            callback?.Invoke(result);
        }

        public static IEnumerator TransactionSend(Transaction tx)
        {
            var download = new Download(appInfo.blockchainServer + $"/api/blockchain/{GetBC(tx.Blockchain)}/transaction/send",
                new DictSO { ["data"] = tx.Signed });

            yield return download.RequestCoroutine();

            var sendTxResult = new Result(download.responseDict);

            if (!sendTxResult.success)
            {
                tx.error = sendTxResult.error;
                tx.SetStatus(Transaction.Status.REJECTED);
                yield break;
            }

            tx.Hash = sendTxResult.data["transactionId"].ToString();
        }

        public static IEnumerator TransactionSendFromServerAccount(Blockchain blockchain, string method, string[] parameters, Action<Result> callback = null)
        {
            var download = new Download(appInfo.blockchainServer + "/api/blockchain/contract/trigger",
                new DictSO
                {
                    ["bcId"] = blockchain.id,
                    ["method"] = method,
                    ["parameters"] = parameters,
                });

            yield return download.RequestCoroutine();

            if (!download.success)
            {
                callback?.Invoke(new Result().SetError(download.errorMessage));
                yield break;
            }

            callback?.Invoke(new Result().SetSuccess(download.responseDict));
        }

        //public static IEnumerator TransactionSetStatus(Transaction tx, Action<Result> callback = null)
        //{
        //    Log.Info($"OxServer - TransactionSetStatus: {tx.Hash} (age: {tx.age.TotalSeconds} sec)");

        //    if (supportedBlockchains.Contains(tx.Blockchain))
        //    {
        //        yield return NethereumManager.TransactionSetStatus(tx, callback);
        //    }
        //    else
        //    {
        //        var download = new Download(appInfo.blockchainServer + $"/api/blockchain/{GetBC(tx.Blockchain)}/transaction/{tx.Hash}");
        //        yield return download.RequestCoroutine();

        //        if (!download.success || download.responseDict == null || !download.responseDict.IsValue("status"))
        //        {
        //            callback?.Invoke(new Result().SetError(download.errorMessage));
        //            yield break;
        //        }

        //        var statusResult = new Result(download.responseDict);

        //        var status = statusResult.data.GetEnum<Transaction.Status>("status");

        //        if (tx.Blockchain == Blockchain.neo &&
        //            tx.status == Transaction.Status.PENDING && status == Transaction.Status.SUCCESS)
        //            yield return TransactionForceSync(tx);
        //        else
        //            tx.SetStatus(status);

        //        callback?.Invoke(statusResult);
        //    }
        //}

        public static IEnumerator TransactionForceSync(Transaction tx)
        {
            Log.Info($"OxServer - TransactionForceSync: {tx.Hash}");

            var download = new Download(appInfo.blockchainServer + $"/api/event/{GetBC(tx.Blockchain)}/{tx.Hash}/sync", type: Download.Type.POST);
            yield return download.RequestCoroutine();

            if (download.success)
                tx.SetStatus(Transaction.Status.SUCCESS);
        }

        // TODO Fix this workaround
        private static string GetBC(Blockchain blockchain) => blockchain.name == Blockchain.Names.EOSIO ? "EOS" : blockchain.name.ToString().ToUpper();
    }
}