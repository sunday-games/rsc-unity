using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG.BlockchainPlugin
{
    public abstract class Wallet
    {
        public static List<Type> GetSupportedWallets(Blockchain blockchain)
        {
            var types = new List<Type>();

            if (blockchain == Blockchain.ethereum)
            {
#if SG_BLOCKCHAIN
                //types.Add(typeof(Immutable));
                types.Add(typeof(SundayWallet));
#endif

#if ARKANE && (UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                types.Add(typeof(ArkaneWallet));
#endif

#if METAMASK && UNITY_WEBGL && !UNITY_EDITOR
                types.Add(typeof(MetaMask));
#endif

#if LUMI && (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                types.Add(typeof(LumiWallet));
#endif
            }
            else if (blockchain == Blockchain.polygon)
            {
#if SG_BLOCKCHAIN
                //types.Add(typeof(Immutable));
                types.Add(typeof(SundayWallet));
#endif

#if ARKANE && (UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                types.Add(typeof(ArkaneWallet));
#endif

#if METAMASK && UNITY_WEBGL && !UNITY_EDITOR
                types.Add(typeof(MetaMask));
#endif
            }
            else if (blockchain == Blockchain.bsc)
            {
#if SG_BLOCKCHAIN
                //types.Add(typeof(Immutable));
                types.Add(typeof(SundayWallet));
#endif

#if ARKANE && (UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                types.Add(typeof(ArkaneWallet));
#endif

#if METAMASK && UNITY_WEBGL && !UNITY_EDITOR
                types.Add(typeof(MetaMask));
#endif
            }
            else if (blockchain == Blockchain.NeoX)
            {
#if SG_BLOCKCHAIN
                //types.Add(typeof(Immutable));
                types.Add(typeof(SundayWallet));
#endif

#if METAMASK && UNITY_WEBGL && !UNITY_EDITOR
                types.Add(typeof(MetaMask));
#endif
            }
            else if (blockchain == Blockchain.eosio)
            {
#if SCATTER && (UNITY_EDITOR || UNITY_WEBGL)
                types.Add(typeof(ScatterWallet));
#endif

#if MEETONE && (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                types.Add(typeof(MeetOneWallet));
#endif
            }
            else if (blockchain == Blockchain.wax)
            {
#if SCATTER && (UNITY_EDITOR || UNITY_WEBGL)
                types.Add(typeof(ScatterWallet));
#endif

#if MEETONE && (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                types.Add(typeof(MeetOneWallet));
#endif
            }
            else if (blockchain == Blockchain.neo)
            {
#if ARKANE && (UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                types.Add(typeof(ArkaneWallet));
#endif
            }
            else if (blockchain == Blockchain.tron)
            {
#if ARKANE && (UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                types.Add(typeof(ArkaneWallet));
#endif

#if TRONLINK && UNITY_WEBGL && !UNITY_EDITOR
                types.Add(typeof(TronLinkWallet));
#endif
            }

            return types;
        }

        public static implicit operator bool(Wallet wallet) => wallet != null;

        public static List<Wallet> all;
        public static T GetWallet<T>() where T : Wallet => (T) all.Find(wallet => wallet is T);
        public static bool GetWallet(Type walletType, out Wallet wallet)
        {
            wallet = all.Find(wallet => walletType.IsInstanceOfType(wallet));
            return wallet != null;
        }

        public static Wallet Deserialize(string title)
        {
            if (title.IsEnum<Names>())
                return Deserialize(title.ToEnum<Names>());

            foreach (var wallet in all)
                if (wallet.nameToView == title)
                    return wallet;
            return null;
        }
        public static Wallet Deserialize(Names name)
        {
            foreach (var wallet in all)
                if (wallet.name == name)
                    return wallet;
            return null;
        }

        public enum Names { MetaMask, Arkane, LumiCollect, Scatter, MeetOne, TronLink, SundayWallet, Immutable }
        public Names name;
        public string nameToView;

        public Sprite iconColor => config.GetIconColor(name.ToString());
        public Sprite iconWhite => config.GetIconWhite(name.ToString());

        public Blockchain[] supportedBlockchains;

        public Dictionary<Blockchain, Account> loginedAccounts = new Dictionary<Blockchain, Account>();
        public bool IsLogined(Blockchain blockchain) => loginedAccounts.ContainsKey(blockchain);

        protected Dictionary<RuntimePlatform, string> deepUrls;
        public string deepUrl => deepUrls[Application.platform];

        protected Dictionary<RuntimePlatform, string> downloadUrls;
        public string downloadUrl => downloadUrls[Application.platform];

        public static Action<string> onProgressStart;
        public static Action<Result> onProgressEnd;

        public abstract IEnumerator Login(Blockchain blockchain, string requestedAddress = null, Action<Result> callback = null);

        //public static Action<Blockchain, Result> onAccountInfoUpdated;
        public virtual IEnumerator AccountGetInfo(Blockchain blockchain, string address = null, Action<Result<decimal>> callback = null)
        {
            yield return null;
            callback?.Invoke(new Result<decimal>(error: Errors.FeatureDontSupported));
        }

        public virtual IEnumerator Logout(Blockchain blockchain, Action<Result> callback = null)
        {
            yield return null;
            loginedAccounts.Remove(blockchain);
            callback?.Invoke(new Result().SetSuccess());
        }

        public abstract IEnumerator MessageSign(Blockchain blockchain, string message, Action<Result> callback = null);

#if SG_BLOCKCHAIN
        public virtual IEnumerator MessageSignEIP712(Blockchain blockchain, Nethereum.ABI.EIP712.TypedData<Nethereum.ABI.EIP712.Domain> typedData, Action<Result> callback = null)
        {
            yield return null;
            var messageSignResult = new Result().SetError(Errors.FeatureDontSupported);
            onProgressEnd?.Invoke(messageSignResult);
            callback?.Invoke(messageSignResult);
        }
#endif

        public abstract IEnumerator TransactionSignAndSend(Transaction tx, Action<Transaction> callback = null);

        public virtual IEnumerator TransactionSignAndReplace(Transaction tx)
        {
            yield return null;
            tx.error = Errors.FeatureDontSupported;
            tx.SetStatus(Transaction.Status.REJECTED);
        }
        public virtual IEnumerator TransactionSign(Transaction tx)
        {
            yield return null;
            tx.error = Errors.FeatureDontSupported;
            tx.SetStatus(Transaction.Status.REJECTED);
        }
        public virtual IEnumerator TransactionSend(Transaction tx)
        {
            yield return null;
            tx.error = Errors.FeatureDontSupported;
            tx.SetStatus(Transaction.Status.REJECTED);
        }

        public virtual IEnumerator TransactionSetStatus(Transaction tx, Action<Result> callback = null)
        {
            yield return null;
            tx.error = Errors.FeatureDontSupported;
            callback?.Invoke(new Result().SetError(Errors.FeatureDontSupported));
        }

        public virtual IEnumerator TransactionEstimate(Transaction tx)
        {
            yield return null;
            tx.error = Errors.FeatureDontSupported;
        }

        protected Configurator config = Configurator.Instance;

        public static Action<Result> onError;
        public static class Errors
        {
            public const string Unknown = "Unknown";
            public const string ConnectionError = "ConnectionError";
            public const string OperationCanceled = "OperationCanceled";
            public const string TooManyRequests = "Too Many Requests"; // node limit reached
            public const string WalletNotSelected = "WalletNotSelected";
            public const string WalletNotInstalled = "WalletNotInstalled";
            public const string WalletLocked = "WalletLocked";
            public const string NetworkWrong = "WrongNetwork";
            //public const string NetworkFailToSwitch = "NetworkFailToSwitch";
            public const string NetworkNeedToAddChain = "NetworkNeedToAddChain";
            public const string NetworkFailToAddChain = "NetworkFailToAddChain";
            public const string NetworkWrongButThisFixed = "NetworkWrongButThisFixed";
            public const string NetworkUnsupported = "NetworkUnsupported";
            public const string AccountWrong = "AccountIsWrong";
            public const string TransactionWillFail = "TransactionWillFail";
            public const string TransactionNotEnoughTokens = "TransactionNotEnoughTokens";
            public const string FeatureDontSupported = "FeatureDontSupported";
            public const string FeePriceNotDefined = "FeePriceNotDefined";
            public const string BlockchainUnsupported = "BlockchainUnsupported";

            // Tron only
            public const string AccountDoesNotExist = "AccountDoesNotExist";
            public const string OperationTimeout = "OperationTimeout";

            // EOS only
            public const string NotLoggedIn = "NotLoggedIn";
            public const string TransactionExpired = "TransactionExpired";
            public const string TransactionExceededCPU = "TransactionExceededCPU";
            public const string TransactionExceededRAM = "TransactionExceededRAM";
            public const string TransactionExceededNET = "TransactionExceededNET";
        }

        public static class Messages // TODO Localize
        {
            public const string LoginApprove = "Please approve access to the public address of your account";

            public static string MessageSignApprove =>
                $"Please approve the message signing to log into the {Configurator.Instance.appInfo.title}";

            public const string TransactionEstimate = "Transaction estimation";

            public const string TransactionSignApprove = "Please approve the transaction signing";

            public static string NetworkWrong_SwitchApprove(Blockchain.Network network) =>
                $"Wrong network selected in the wallet ({network.Name} is required) - please approve the network switch";

            public static string NetworkWrong_AddApprove(Blockchain.Network network) =>
                $"{network.Name} is not added to the wallet - please approve the network addition";

            public static string NetworkWrong_AddedSwitchApprove(Blockchain.Network network) =>
                $"{network.Name} added successfully - please approve the network switch";
        }
    }
}