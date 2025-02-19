using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SG.BlockchainPlugin
{
    public class BlockchainManager : MonoBehaviour
    {
        public static BlockchainManager Instance;

#if UNITY_WEBGL && SG_BLOCKCHAIN
        public static Web3 Web3;
#endif

        public static bool IsSupported(params Blockchain.Names[] blockchainNames)
        {
            if (!Instance)
                return false;

            foreach (var blockchainName in blockchainNames)
                if (Instance.SupportedBlockchainNames.Contains(blockchainName))
                    return true;
            return false;
        }
        public static bool IsSupported(params Wallet.Names[] walletNames)
        {
            if (!Instance)
                return false;

            foreach (var walletName in walletNames)
                if (Instance.SupportedWalletNames.Contains(walletName))
                    return true;
            return false;
        }

        public static string Defines()
        {
            if (!Instance)
                Instance = Configurator.Instance.GetComponentInChildren<BlockchainManager>();

            string defines = "";
            if (IsSupported(Blockchain.Names.Ethereum))
                defines += "-define:ETHEREUM" + Const.lineBreak;
            if (IsSupported(Blockchain.Names.Polygon))
                defines += "-define:POLYGON" + Const.lineBreak;
            if (IsSupported(Blockchain.Names.BSC))
                defines += "-define:BSC" + Const.lineBreak;
            if (IsSupported(Blockchain.Names.EOSIO))
                defines += "-define:EOSIO" + Const.lineBreak;
            if (IsSupported(Blockchain.Names.WAX))
                defines += "-define:WAX" + Const.lineBreak;
            if (IsSupported(Blockchain.Names.Tron))
                defines += "-define:TRON" + Const.lineBreak;
            if (IsSupported(Blockchain.Names.NEO))
                defines += "-define:NEO" + Const.lineBreak;

            if (IsSupported(Wallet.Names.Arkane))
                defines += "-define:ARKANE" + Const.lineBreak;
            if (IsSupported(Wallet.Names.MetaMask))
                defines += "-define:METAMASK" + Const.lineBreak;
            if (IsSupported(Wallet.Names.TronLink))
                defines += "-define:TRONLINK" + Const.lineBreak;
            if (IsSupported(Wallet.Names.Scatter))
                defines += "-define:SCATTER" + Const.lineBreak;
            if (IsSupported(Wallet.Names.Immutable))
                defines += "-define:IMMUNTABLE" + Const.lineBreak;
            if (IsSupported(Wallet.Names.MeetOne))
                defines += "-define:MEETONE" + Const.lineBreak;
            if (IsSupported(Wallet.Names.LumiCollect))
                defines += "-define:LUMI" + Const.lineBreak;

            return defines;
        }

        public static string SetupIndexHTML_Head()
        {
            string head = "";

            if (IsSupported(Wallet.Names.Arkane))
                head += @"
    <script src='js/blockchain/arkane-wallet.min.js'></script>
";

            if (IsSupported(Wallet.Names.TronLink))
                head += @"
	<script src='js/blockchain/TronWeb.js'></script>
	<script src='js/blockchain/tron-blockchain.min.js'></script>
";

            return head;
        }

        public static string SetupIndexHTML_OnSuccess()
        {
            string onSuccess = "";

            if (IsSupported(Wallet.Names.Arkane))
                onSuccess += $@"
                window.ArkaneWallet = window.initializeArkaneWallet(unityNotifier(bindedSendMessage, '{Wallet.Names.Arkane}'));
";

            if (IsSupported(Wallet.Names.MetaMask))
                onSuccess += $@"
			    nethereumUnityInstance = unityInstance;
";

            if (IsSupported(Wallet.Names.TronLink))
                onSuccess += $@"
		        const getTronProvider = async () => {{
			        if (!!window.tronWeb)
				        return window.tronWeb;
			        else
				        console.log('Non-Tron browser detected. You should consider trying TronLink!');
		        }};

		        window.TronBlockchain = window.initializeTronBlockchain(await getTronProvider(), unityNotifier(bindedSendMessage, '{Wallet.Names.TronLink}'));
";

            return onSuccess;
        }

        public Blockchain.Names DefaultBlockchainName;
        public Blockchain.Names[] SupportedBlockchainNames;
        [NonSerialized] public Blockchain[] SupportedBlockchains;
        public Wallet.Names[] SupportedWalletNames;

        public static TokenView[] TokenViews;
        public static TokenView GetTokenView(string name) => TokenViews.Find(v => v.name.IsEqualIgnoreCase(name));

        public static Token GetToken(string name)
        {
            // :( 
            if (name == "Blueprints")
                name = "Blueprint";
            else if (name == "Rewards")
                name = "Reward";

            var view = GetTokenView(name);
            if (view != null)
                return view.Token;

            if (Instance != null)
                foreach (var blockchain in Instance.SupportedBlockchains)
                    if (blockchain.NativeToken.Name == name)
                        return blockchain.NativeToken;

            return null;
        }

        public IEnumerator Init(Result initResult)
        {
            Instance = this;

            //  if (OxServer == null) OxServer = gameObject.AddComponent<OxServer>();
#if UNITY_WEBGL && SG_BLOCKCHAIN
            if (Web3 == null)
            {
                Web3 = new GameObject("Web3").AddComponent<Web3>();
                Web3.transform.SetParent(transform);
            }
#endif
            if (Blockchain.ethereum == null && IsSupported(Blockchain.Names.Ethereum))
            {
                Blockchain.ethereum = new Ethereum();
                Blockchain.ethereum.Setup();
            }
            if (Blockchain.polygon == null && IsSupported(Blockchain.Names.Polygon))
            {
                Blockchain.polygon = new Polygon();
                Blockchain.polygon.Setup();
            }
            if (Blockchain.bsc == null && IsSupported(Blockchain.Names.BSC))
            {
                Blockchain.bsc = new BSC();
                Blockchain.bsc.Setup();
            }
            if (Blockchain.Immutable == null && IsSupported(Blockchain.Names.Immutable))
            {
                Blockchain.Immutable = new ImmutableEVM();
                Blockchain.Immutable.Setup();
            }
            if (Blockchain.NeoX == null && IsSupported(Blockchain.Names.NeoX))
            {
                Blockchain.NeoX = new NeoX();
                Blockchain.NeoX.Setup();
            }
            if (Blockchain.tron == null && IsSupported(Blockchain.Names.Tron))
            {
                Blockchain.tron = new Tron();
                Blockchain.tron.Setup();
            }
            if (Blockchain.neo == null && IsSupported(Blockchain.Names.NEO))
            {
                Blockchain.neo = new NEO();
                Blockchain.neo.Setup();
            }
            if (Blockchain.eosio == null && IsSupported(Blockchain.Names.EOSIO))
            {
                Blockchain.eosio = new EOSIO();
                Blockchain.eosio.Setup();
                if (Blockchain.eosio.network.Nodes.Length > 1)
                    yield return Blockchain.eosio.FindAndSetAvailableNode(Blockchain.eosio.network);
            }
            if (Blockchain.wax == null && IsSupported(Blockchain.Names.WAX))
            {
                Blockchain.wax = new WAX();
                Blockchain.wax.Setup();
                if (Blockchain.wax.network.Nodes.Length > 1)
                    yield return Blockchain.wax.FindAndSetAvailableNode(Blockchain.wax.network);
            }

            Blockchain.current = DefaultBlockchainName.ToBlockchain();

            SupportedBlockchains = new Blockchain[SupportedBlockchainNames.Length];
            for (int i = 0; i < SupportedBlockchains.Length; i++)
                SupportedBlockchains[i] = SupportedBlockchainNames[i].ToBlockchain();

            Wallet.all = new List<Wallet>();
            foreach (var blockchain in SupportedBlockchains)
                blockchain.SetupWallets();

            SundayWalletUI.UI.Setup();
            Accounts.AccountsLoad();

            TokenViews = Resources.LoadAll<TokenView>("Tokens");
            Token.USDT = GetTokenView("USDT").SetupToken();
            Token.USDC = GetTokenView("USDC").SetupToken();
            Blockchain.bsc.NativeWrappedToken = GetTokenView("WBNB").SetupToken();
        }

        private void OnDestroy()
        {
            Blockchain.eosio = null;
            Blockchain.tron = null;
            Blockchain.ethereum = null;
            Blockchain.neo = null;
            Blockchain.NeoX = null;
            Blockchain.polygon = null;
            Blockchain.bsc = null;
            Blockchain.wax = null;
            Blockchain.Immutable = null;
            Blockchain.current = null;
        }
    }
}