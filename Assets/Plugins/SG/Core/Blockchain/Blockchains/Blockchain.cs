using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG.BlockchainPlugin
{
    public static class BlockchainExtensions
    {
        public static Blockchain ToBlockchain(this Blockchain.Names name) => Blockchain.Deserialize(name.ToString());
    }

    public abstract class Blockchain
    {
        public static EOSIO eosio;
        public static Tron tron;
        public static Ethereum ethereum;
        public static NEO neo;
     
        public static Polygon polygon;
        public static WAX wax;
        //public static FreeTON FreeTON;
        public static BSC bsc;
        public static NeoX NeoX;

        public static Blockchain current;

        public static Blockchain Deserialize(int id) => id.ToEnum<Names>().ToBlockchain();
        public static Blockchain Deserialize(string name)
        {
            if (name.IsEmpty())
                return null;

            name = name.ToLower();
            if (name.Contains("eosio") || name.Contains("eos"))
                return eosio;
            else if (name.Contains("tron") || name.Contains("trx"))
                return tron;
            else if (name.Contains("ethereum") || name.Contains("eth"))
                return ethereum;
            else if (name.Contains("neox"))
                return NeoX;
            else if (name.Contains("neo"))
                return neo;
            else if (name.Contains("polygon") || name.Contains("matic"))
                return polygon;
            else if (name.Contains("bsc") || name.Contains("bnb"))
                return bsc;
            else if (name.Contains("wax"))
                return wax;
            else
                return null;
        }

        public static Blockchain IdentifyAddress(string address)
        {
            foreach (var blockchain in BlockchainManager.Instance.SupportedBlockchains)
                if (blockchain.IsValidAddress(address))
                    return blockchain;

            return null;
        }

        public static implicit operator bool(Blockchain b) => b != null;

        [HideInInspector]
        public Names name;
        public enum Names { EOSIO, Tron, Ethereum, NEO, Polygon, WAX, FreeTON, BSC, NeoX }

        public int id => (int) name;

        public override string ToString() => name.ToString();

        public Sprite iconColor => config.GetIconColor(name.ToString());
        public Sprite iconWhite => config.GetIconWhite(name.ToString());

        public Token FeeToken;
        public decimal feePriceMin;
        public decimal feePriceWarning = decimal.MaxValue;

        public static Action<Blockchain> onFeeInfoUpdated;
        public DateTime feeInfoLastUpdated = default;
        public decimal feeBase;
        public List<decimal> feePrioritySuggestion;

        public Token GasToken;
        public ulong gasMax;
        public decimal gasMultiplier = 1m;

        public Token NativeToken;
        public Token NativeWrappedToken;
        public void NativeTokenSetup(Token token, SmartContract contract, SmartContract.Function transferFunction, SmartContract.Function balanceFunction = null)
        {
            NativeToken = token;

            NativeToken.TransferFunction = new Dictionary<Blockchain, SmartContract.Function> { [this] = transferFunction };

            contract.Functions.Add(transferFunction.Name, transferFunction);
            transferFunction.Contract = contract;

            NativeToken.Contracts = new Dictionary<Blockchain, SmartContract> { [contract.Blockchain] = contract };
        }

        public Network network;
        protected Network networkTest, networkProd;
        public class Network
        {
            public string Name;
            public string Id;
            public string GetCurrencyUrl = "https://sunday.games/buy-cryptocurrency";
            public ulong RequiredBlockConfirmations;

            public int NodeIndex;
            public Uri Node => Nodes[NodeIndex];
            public Uri[] Nodes;

            public Explorer explorer;
            public class Explorer
            {
                public string Url;

                public Explorer(string url) { Url = url; }

                public virtual string GetTxURL(string tx) => Url + "transaction/" + tx;
                public virtual string GetAddressURL(string address) => Url + "address/" + address;
                public virtual string GetNonFungibleTokenURL(string address, int id) => Url + "token/" + address + "?a=" + id;
            }

            public Network(string name,
                string id,
                TimeSpan blockCreationTime,
                ulong requiredBlockConfirmations = 10,
                string getCurrencyUrl = null,
                string[] nodeUrls = null,
                Explorer explorer = null)
            {
                Name = name;
                Id = id;
                BlockCreationTime = blockCreationTime;
                RequiredBlockConfirmations = requiredBlockConfirmations;

                if (!getCurrencyUrl.IsEmpty())
                    GetCurrencyUrl = getCurrencyUrl;

                if (nodeUrls != null && nodeUrls.Length > 0)
                {
                    Nodes = new Uri[nodeUrls.Length];
                    for (int i = 0; i < nodeUrls.Length; i++)
                        Nodes[i] = new Uri(nodeUrls[i]);
                }
                NodeIndex = PlayerPrefs.GetInt(_nodeIndexPrefs, 0);

                this.explorer = explorer;
            }

            public TimeSpan BlockCreationTime;
            private DateTime _blockDate;
            public bool IsBlockNumberActual => _blockNumber.HasValue && DateTime.UtcNow - _blockDate < BlockCreationTime;

            private ulong? _blockNumber;
            public ulong? BlockNumber
            {
                get => _blockNumber;
                set
                {
                    if (!_blockNumber.HasValue || value > _blockNumber)
                    {
                        _blockDate = DateTime.UtcNow;
                        _blockNumber = value;
                    }
                }
            }

            private string _nodeIndexPrefs => $"Ox.BlockchainPlugin.{Id}.nodeIndex";
            public void SetNode(int index)
            {
                if (NodeIndex == index)
                    return;

                if (index >= Nodes.Length)
                {
                    Log.Blockchain.Error($"SetNode Error: No node url with such index ({index}/{Nodes.Length - 1})");
                    return;
                }

                NodeIndex = index;

                PlayerPrefs.SetInt(_nodeIndexPrefs, NodeIndex);

                Log.Blockchain.Info($"SetWallet: {NodeIndex}");
            }

            public string NodeName(int index)
            {
                var nodeName = Nodes[index].Host;

                var parts = nodeName.Split('.');
                if (parts.Length > 1)
                    return parts[parts.Length - 2];

                return nodeName;
            }
        }

        public virtual void Setup() { }

        public void SetupWallets()
        {
            wallets.Clear();
            foreach (var walletType in Wallet.GetSupportedWallets(this))
            {
                if (!Wallet.GetWallet(walletType, out Wallet wallet))
                {
                    wallet = (Wallet) Activator.CreateInstance(walletType, true);
                    Wallet.all.Add(wallet);

                    Log.Blockchain.Info("Wallet setuped: " + wallet.name);
                }

                wallets.Add(wallet);
            }

            SetWallet(PlayerPrefs.GetInt(walletIndexPrefs, -1));
        }

        [HideInInspector]
        public List<Wallet> wallets = new List<Wallet>();
        public Wallet wallet { get; private set; }
        public int walletIndex => wallet ? wallets.IndexOf(wallet) : -1;
        string walletIndexPrefs => $"Ox.BlockchainPlugin.{name}.wallet";

        public static Action<Wallet> onWalletSet;
        public void SetWallet(Wallet wallet) => SetWallet(wallets.IndexOf(wallet));
        public void SetWallet(int value)
        {
            if (walletIndex == value)
                return;

            if (value >= wallets.Count)
            {
                Log.Blockchain.Error($"{name} - SetWallet Error: No wallet with such index ({value}/{wallets.Count - 1})");
                return;
            }

            wallet = value < 0 ? null : wallets[value];

            PlayerPrefs.SetInt(walletIndexPrefs, value);

            Log.Blockchain.Info($"{name} - SetWallet: {(wallet ? wallet.nameToView : "None")}");

            onWalletSet?.Invoke(wallet);
        }

        protected Configurator config = Configurator.Instance;

        public virtual Dictionary<string, object> GetTxDefaultExecutionFormat(Transaction tx) => null;

        public virtual object CurrencyToTxFormat(decimal value) => NativeToken.ToMin(value);

        public virtual decimal CurrencyFromTxFormat(object value) => NativeToken.FromMin(value.ToBigInteger());

        public abstract bool IsValidAddress(string address);
        public virtual bool IsValidPrivateKey(string privateKey) => !privateKey.IsEmpty() && privateKey.Length >= 64;

        // public void OpenAddressUrl(string address) => Utils.OpenLink(network.explorer.GetAddressURL(address));

        public abstract string RandomAddress();

        public virtual string RandomTxHash() => Utils.RandomString(64);

        public Account loginedAccount => wallet && wallet.IsLogined(this) ? wallet.loginedAccounts[this] : null;

        public virtual IEnumerator GetFeePrice(bool showLoading = false, Result result = null)
        {
            yield break;
        }
    }
}