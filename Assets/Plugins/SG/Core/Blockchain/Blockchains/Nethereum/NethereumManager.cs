using System;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Collections;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using ListO = System.Collections.Generic.List<object>;

#if SG_BLOCKCHAIN
using Nethereum.Util;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Unity.Rpc;
using Nethereum.Unity.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.EIP712;

namespace SG.BlockchainPlugin
{
    public static class NethereumManager
    {
        public static IEnumerator AccountUpdateBalance(Account account, IUnityRpcRequestClientFactory factory, Result<decimal> result = null)
        {
            var request = new EthGetBalanceUnityRequest(factory);
            yield return request.SendRequest(account.Address, BlockParameter.CreateLatest());

            if (request.Exception != null)
            {
                Log.Blockchain.Error($"AccountGetInfo '{account.Address}' - Error: {request.Exception.Message}");
                result?.SetError(request.Exception.Message);
                yield break;
            }

            account.SetBalance(account.Blockchain.NativeToken.FromMin(request.Result.Value));
            Log.Blockchain.Debug($"AccountGetInfo '{account.Address}' - Balance: {account.Balance}");

            result?.SetSuccess(account.Balance);
        }

        public static IEnumerator AccountUpdateBalance(Blockchain blockchain, string address, IUnityRpcRequestClientFactory factory, Result result = null)
        {
            var request = new EthGetBalanceUnityRequest(factory);
            yield return request.SendRequest(address, BlockParameter.CreateLatest());

            if (request.Exception != null)
            {
                Log.Blockchain.Error($"AccountGetInfo '{address}' - Error: {request.Exception.Message}");
                result?.SetError(request.Exception.Message);
                yield break;
            }

            var balance = blockchain.NativeToken.FromMin(request.Result.Value);
            Log.Blockchain.Debug($"AccountGetInfo '{address}' - Balance: {balance}");

            result?.SetSuccess(new DictSO
            {
                ["account"] = address,
                ["balance"] = balance,
                ["balanceRaw"] = request.Result.Value,
            });
        }

        public static IEnumerator AccountUpdateTxCount(Account account, IUnityRpcRequestClientFactory factory, Result result = null)
        {
            var request = new EthGetTransactionCountUnityRequest(factory);
            yield return request.SendRequest(account.Address, BlockParameter.CreateLatest());

            if (request.Exception != null)
            {
                result?.SetError(request.Exception.Message);
                yield break;
            }

            account.TransactionCount = request.Result.Value.ToString().ToULong();
            Log.Blockchain.Debug($"AccountGetInfo '{account.Address}' - Transaction Count: {account.TransactionCount}");

            result?.SetSuccess(new DictSO
            {
                ["account"] = account.Address,
                ["transactionCount"] = account.TransactionCount,
            });
        }

        public static IEnumerator Read<T>(this SmartContract.Function function, DictSO content, Action<Result<T>> callback)
        {
            var request = new EthCallUnityRequest(function.Contract.Blockchain.network.Node.OriginalString);
            var contract = new Contract(null, function.Contract.ABI, function.Contract.Address);
            var nethereumFinction = contract.GetFunction(function.Name);
            var input = nethereumFinction.CreateCallInput(content.Values.ToArray());

            yield return request.SendRequest(input, BlockParameter.CreateLatest());

            if (request.Exception != null)
            {
                //Log.Blockchain.Error(function + " call fail. Error: " + request.Exception.Message);
                callback.Invoke(new Result<T>(error: request.Exception.Message));
                yield break;
            }

            callback?.Invoke(new Result<T>(nethereumFinction.DecodeTypeOutput<T>(request.Result)));      
        }

        public static IEnumerator Read<T>(this SmartContract.Function function, DictSO content, Func<string, T> mod, Action<Result<T>> callback)
        {
            var request = new EthCallUnityRequest(function.Contract.Blockchain.network.Node.OriginalString);
            var contract = new Contract(null, function.Contract.ABI, function.Contract.Address);
            var nethereumFinction = contract.GetFunction(function.Name);
            var input = nethereumFinction.CreateCallInput(content.Values.ToArray());

            yield return request.SendRequest(input, BlockParameter.CreateLatest());

            if (request.Exception != null)
            {
                //Log.Blockchain.Error(function + " call fail. Error: " + request.Exception.Message);
                callback.Invoke(new Result<T>(error: request.Exception.Message));
                yield break;
            }

            callback?.Invoke(new Result<T>(mod(request.Result)));
        }

        public static IEnumerator Read<T, R>(this SmartContract.Function function, DictSO content, Func<T, R> mod, Action<Result<R>> callback)
        {
            var request = new EthCallUnityRequest(function.Contract.Blockchain.network.Node.OriginalString);
            var contract = new Contract(null, function.Contract.ABI, function.Contract.Address);
            var nethereumFinction = contract.GetFunction(function.Name);
            var input = nethereumFinction.CreateCallInput(content.Values.ToArray());

            yield return request.SendRequest(input, BlockParameter.CreateLatest());

            if (request.Exception != null)
            {
                //Log.Blockchain.Error(function + " call fail. Error: " + request.Exception.Message);
                callback.Invoke(new Result<R>(error: request.Exception.Message));
                yield break;
            }

            callback?.Invoke(new Result<R>(mod(nethereumFinction.DecodeTypeOutput<T>(request.Result))));
        }

        public static IEnumerator TransactionSignAndSend(Transaction tx, IContractTransactionUnityRequest request)
        {
            if (tx.FunctionIsNativeTokenTransfer)
            {
                var nativeTokenTransferRequest = new EthTransferUnityRequest(request);

                if (tx.feeMaxPriority > 0)
                    yield return nativeTokenTransferRequest.TransferEther(tx.To, tx.NativeTokenAmount,
                        gas: tx.gasLimit,
                        maxPriorityFeePerGas: tx.Blockchain.FeeToken.ToMin(tx.feeMaxPriority),
                        maxFeePerGas: tx.Blockchain.FeeToken.ToMin(tx.feeMax));
                else if (tx.feeMax > 0)
                    yield return nativeTokenTransferRequest.TransferEther(tx.To, tx.NativeTokenAmount,
                        gas: tx.gasLimit,
                        gasPriceGwei: tx.feeMax);
                else
                    yield return nativeTokenTransferRequest.TransferEther(tx.To, tx.NativeTokenAmount,
                        gas: tx.gasLimit);

                request.Exception = nativeTokenTransferRequest.Exception;
                request.Result = nativeTokenTransferRequest.Result;
            }
            else
            {
                yield return request.SignAndSendTransaction(tx.GetInput());
            }

            if (request.Exception != null)
            {
                Log.Blockchain.Error($"TransactionSignAndSend - Error: {request.Exception.Message} {request.Exception.Source}/n{tx}");
                tx.error = request.Exception.Message;
                tx.errorData = new DictSO
                {
                    ["message"] = request.Exception.Message,
                    [Result.RAW_ERROR_KEY] = request.Exception.Source,
                };
                yield break;
            }

            tx.Hash = request.Result;
        }

        public static IEnumerator TransactionEstimate(Transaction tx, IUnityRpcRequestClientFactory factory, Action<Result> callback = null)
        {
            var request = new EthEstimateGasUnityRequest(factory);

            //if (tx.FunctionMessage != null)
            //{
            //    tx.FunctionMessage.FromAddress = tx.From;

            //    yield return request.SendRequest(tx.FunctionMessage.CreateCallInput(tx.Function.Contract.Address));
            //}
            //else
            yield return request.SendRequest(tx.GetInput());

            if (request.Exception != null)
            {
                Log.Blockchain.Error($"TransactionEstimate fail - {request.Exception.Message}: {request.Exception.Source}");
                tx.error = request.Exception.Message.Contains("failing transaction") ?
                    Wallet.Errors.TransactionWillFail :
                    request.Exception.Message;
                tx.errorData = new DictSO
                {
                    ["message"] = request.Exception.Message,
                    [Result.RAW_ERROR_KEY] = request.Exception.Source,
                };

                yield break;
            }

            tx.gasEstimation = request.Result.Value.ToString().ToULong();

            Log.Blockchain.Info("TransactionEstimate: " + tx.gasEstimation);

            callback?.Invoke(new Result().SetSuccess());
        }

        public static IEnumerator TransactionSetStatus(Transaction tx, IUnityRpcRequestClientFactory factory, Action<Result> callback = null)
        {
            if (tx.Hash.IsEmpty())
            {
                Log.Blockchain.Error($"TransactionSetStatus - Error: Hash is null");
                yield break;
            }

            var request = new EthGetTransactionReceiptUnityRequest(factory);
            yield return request.SendRequest(tx.Hash);

            if (request.Exception != null)
            {
                Log.Blockchain.Error($"TransactionSetStatus '{tx.Hash}' - Error: {request.Exception.Message}");
                callback?.Invoke(new Result().SetError(request.Exception.Message));
                yield break;
            }

            if (request.Result == null)
            {
                Log.Blockchain.Debug($"TransactionSetStatus '{tx.Hash}' - Status: {Transaction.Status.PENDING}");
                tx.SetStatus(Transaction.Status.PENDING);
                callback?.Invoke(new Result().SetSuccess(new DictSO { ["status"] = tx.status.ToString() }));
                yield break;
            }

            if (!tx.BlockNumber.HasValue)
            {
                tx.BlockNumber = request.Result.BlockNumber.Value.ToString().ToULong();
                Log.Blockchain.Debug($"TransactionSetStatus '{tx.Hash}' - BlockNumber: {tx.BlockNumber}");
            }

            if (!tx.Blockchain.network.BlockNumber.HasValue)
            {
                tx.Blockchain.network.BlockNumber = tx.BlockNumber;
                Log.Blockchain.Debug($"BlockNumberUpdate - BlockNumber: {tx.Blockchain.network.BlockNumber}");
            }

            if (request.Result.Status.Value == 0)
            {
                tx.SetStatus(Transaction.Status.FAIL);
                callback?.Invoke(new Result().SetSuccess(new DictSO { ["status"] = tx.status.ToString() }));
                yield break;
            }

            if (!tx.Blockchain.network.IsBlockNumberActual)
                yield return BlockNumberUpdate(tx.Blockchain.network);

            tx.SetBlockConfirmations((ulong) (tx.Blockchain.network.BlockNumber - tx.BlockNumber));

            if (tx.progress == 1f)
                tx.SetStatus(Transaction.Status.SUCCESS);

            callback?.Invoke(new Result().SetSuccess(new DictSO { ["status"] = tx.status.ToString() }));
        }

        public static IEnumerator BlockNumberUpdate(Blockchain.Network network, Action<ulong> callback = null)
        {
            var request = new EthBlockNumberUnityRequest(network.Node.OriginalString);
            yield return request.SendRequest();

            if (request.Result == null)
            {
                if (request.Exception != null)
                    Log.Blockchain.Error($"BlockNumberUpdate - Error: {request.Exception.Message}");
                callback?.Invoke(default);
            }

            var blockNumber = request.Result.Value.ToString().ToULong();

            network.BlockNumber = blockNumber;

            Log.Blockchain.Debug($"BlockNumberUpdate - BlockNumber: {blockNumber}");
            callback?.Invoke(blockNumber);
        }

        public static Account AccountCreate(Blockchain blockchain, string name = null, string privateKey = null)
        {
            try
            {
                var key = privateKey != null ? new EthECKey(privateKey) : EthECKey.GenerateKey();

                return new Account(
                    blockchain,
                    key.GetPublicAddress(),
                    key.GetPubKey().ToHex(prefix: true),
                    privateKey != null ? privateKey : key.GetPrivateKey(),
                    name);
            }
            catch
            {
                return null;
            }
        }

        public static byte[] GetEthereumMessageHash(string message) =>
            GetMessageHash("\u0019Ethereum Signed Message:\n" + message.Length + message);

        public static byte[] GetMessageHash(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            return Sha3Keccack.Current.CalculateHash(bytes);
        }

        public static TypedData<Domain> GetTypedData<T>(Blockchain.Network network, T message, Type[] messageTypes,
            string domainName, string domainVersion, string verifyingContract)
        {
            var types = new Type[messageTypes.Length + 1];
            types[0] = typeof(Domain);
            for (int i = 1; i < types.Length; i++)
                types[i] = messageTypes[i - 1];

            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = domainName,
                    Version = domainVersion,
                    ChainId = network.Id.ToBigInteger(),
                    VerifyingContract = verifyingContract
                },
                PrimaryType = messageTypes[0].Name,
                Types = MemberDescriptionFactory.GetTypesMemberDescription(types),
                Message = MemberValueFactory.CreateFromMessage(message)
            };
        }

        public static string Sign(string message, string privateKey) =>
            SignHash(GetEthereumMessageHash(message), privateKey);

        public static string SignTypedData(TypedData<Domain> typedData, string privateKey)
        {
            var bytes = Eip712TypedDataSigner.Current.EncodeTypedData(typedData);
            var hash = Sha3Keccack.Current.CalculateHash(bytes);
            var signature = new EthECKey(privateKey).SignAndCalculateV(hash);
            return EthECDSASignature.CreateStringSignature(signature);
        }

        public static string SignHash(byte[] hash, string privateKey)
        {
            var key = new EthECKey(privateKey);
            var signature = key.SignAndCalculateV(hash);
            return EthECDSASignature.CreateStringSignature(signature);
        }

        /// <returns>Address of signer</returns>
        public static string RecoverSigner(string originalData, string signedData) =>
            new MessageSigner().HashAndEcRecover(originalData, signedData);

        public static string GetData(Transaction tx)
        {
            if (tx.Function.Contract.ABI.IsEmpty())
                return "0";

            if (tx.FunctionMessage != null)
                return tx.GetInput().Data;
            else
                return new Contract(null, tx.Function.Contract.ABI, tx.Function.Contract.Address)
                    .GetFunction(tx.Function.Name).GetData(GetFunctionValues(tx));
        }

        private static object[] GetFunctionValues(Transaction tx)
        {
            var values = new ListO();

            var i = 0;
            var inputTypes = tx.Function.Inputs.Values.ToArray();
            foreach (var pair in tx.Content)
            {
                var inputType = inputTypes[i++];

                if ((inputType == "bytes" || inputType == "bytes32") && pair.Value is string)
                    values.Add(HexByteConvertorExtensions.HexToByteArray(pair.Value.ToString()));
                else
                    values.Add(pair.Value);
            }

            return values.ToArray();
        }

        public static TransactionInput GetInput(this Transaction tx)
        {
            TransactionInput input;

            if (tx.FunctionMessage != null)
            {
                tx.FunctionMessage.FromAddress = tx.From;

                if (tx.gasLimit != default)
                    tx.FunctionMessage.Gas = new HexBigInteger(tx.gasLimit);

                input = tx.FunctionMessage.CreateTransactionInput(tx.To);
            }
            else
            {
                var function = new Contract(null, tx.Function.Contract.ABI, tx.To).GetFunction(tx.Function.Name);

                input = function.CreateTransactionInput(
                    from: tx.From,
                    gas: new HexBigInteger(tx.gasLimit),
                    value: new HexBigInteger(tx.Blockchain.NativeToken.ToMin(tx.NativeTokenAmount)),
                    functionInput: GetFunctionValues(tx));
            }

            if (tx.nonce.HasValue)
                input.Nonce = new HexBigInteger(tx.nonce.ToBigInteger());

            if (tx.feeMaxPriority > 0 && tx.Blockchain == Blockchain.ethereum)
            {
                input.MaxPriorityFeePerGas = new HexBigInteger(tx.Blockchain.FeeToken.ToMin(tx.feeMaxPriority));
                input.MaxFeePerGas = new HexBigInteger(tx.Blockchain.FeeToken.ToMin(tx.feeMax));
            }
            else if (tx.feeMax > 0)
                input.GasPrice = new HexBigInteger(tx.Blockchain.FeeToken.ToMin(tx.feeMax));

            return input;
        }
    }
}
#else
namespace SG.BlockchainPlugin
{
    public static class NethereumManager
    {
        public static IEnumerator Read<T>(this SmartContract.Function function, DictSO content, Action<T> callback)
        {
            yield break;
        }

        public static IEnumerator Read<T>(this SmartContract.Function function, DictSO content, Action<Result<T>> callback)
        {
            yield break;
        }

        public static IEnumerator Read<T>(this SmartContract.Function function, DictSO content, Func<string, T> mod, Action<Result<T>> callback)
        {
            yield break;
        }

        public static IEnumerator Read<T, R>(this SmartContract.Function function, DictSO content, Func<T, R> mod, Action<Result<R>> callback)
        {
            yield break;
        }
    }
}
#endif