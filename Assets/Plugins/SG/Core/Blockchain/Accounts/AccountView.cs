using UnityEngine;

namespace SG.BlockchainPlugin
{
    [CreateAssetMenu(menuName = "Sunday/" + nameof(AccountView), fileName = nameof(AccountView), order = 0)]
    public class AccountView : ScriptableObject
    {
        public Blockchain.Names BlockchainName;
        public Blockchain Blockchain => BlockchainName.ToBlockchain();
        public string PrivateKey;


        public string Address
        {
            get
            {
                if (_address.IsEmpty())
#if SG_BLOCKCHAIN
                    _address = new Nethereum.Signer.EthECKey(PrivateKey).GetPublicAddress();
#else
                    Log.Error("Fail to get Address. Turn on blockchain feature!");
#endif
                return _address;
            }
        }

        public string Signature
        {
            get
            {
                if (_signature.IsEmpty())
                    _signature = Configurator.FindInstance().appInfo.GetSignature(PrivateKey);
                return _signature;
            }
        }

        [Space]
        [ReadOnly] [SerializeField] private string _address;
        [ReadOnly] [SerializeField] private string _signature;
        [UI.Button("UpdateCache_OnClick")] public bool UpdateCache;
        public void UpdateCache_OnClick()
        {
            Log.Info("Address " + Address + ", Signature " + Signature);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [Space(20)]
        [UI.Button("AddToWallet_OnClick")] public bool AddToWallet;
        public void AddToWallet_OnClick() =>
           Accounts.AccountAdd(Accounts.AccountCreate(Blockchain, name, PrivateKey));

        [Space(20)]
        [UI.Button("OpenExplorer_OnClick")] public bool OpenExplorer;
        public void OpenExplorer_OnClick() =>
            UI.Helpers.OpenLink(Blockchain.current.network.explorer.GetAddressURL(Address));
    }
}