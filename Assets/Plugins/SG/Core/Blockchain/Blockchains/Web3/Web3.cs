#if UNITY_WEBGL && SG_BLOCKCHAIN
using System;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using UnityEngine;

using Nethereum.Hex.HexTypes;
using Nethereum.Unity.Metamask;

namespace SG.BlockchainPlugin
{
    public class Web3 : MonoBehaviour
    {
        private bool _isMetamaskInitialised = false;

        private Action<Result> _onEnable;
        private Action<string> _onAccountChanged;
        private Action<string> _onChainChanged;

        public void Login(Action<Result> onEnable, Action<string> onAccountChanged, Action<string> onChainChanged)
        {
            if (!MetamaskInterop.IsMetamaskAvailable())
            {
                onEnable.Invoke(new Result().SetError(Wallet.Errors.WalletNotInstalled));
                return;
            }

            _onEnable = onEnable;
            _onAccountChanged = onAccountChanged;
            _onChainChanged = onChainChanged;

            MetamaskInterop.EnableEthereum(gameObject.name, nameof(OnEnableSuccess), nameof(OnEnableFail));
        }

        public void OnEnableSuccess(string address)
        {
            Log.Info("Blockchain - OnEnableSuccess: " + address);

            if (!_isMetamaskInitialised)
            {
                MetamaskInterop.EthereumInit(gameObject.name, nameof(AccountChanged), nameof(OnChainChanged));
                MetamaskInterop.GetChainId(gameObject.name, nameof(OnChainChanged), nameof(OnError));
                _isMetamaskInitialised = true;
            }

            _onEnable?.Invoke(new Result().SetSuccess(new DictSO { ["account"] = address }));
        }

        public void OnEnableFail(string error)
        {
            Log.Info("Blockchain - OnEnableFail: " + error);
            _onEnable?.Invoke(new Result().SetError(error));
        }

        public void AccountChanged(string address)
        {
            Log.Info("Blockchain - OnAccountChanged: " + address);
            _onAccountChanged?.Invoke(address);
        }

        public void OnChainChanged(string hexChainId)
        {
            Log.Info($"Blockchain - OnChainChanged: " + hexChainId);
            _onChainChanged?.Invoke(new HexBigInteger(hexChainId).Value.ToString());
        }

        public void OnError(string message)
        {
            Log.Error("OnError: " + message);
            // TODO
        }
    }
}
#endif // UNITY_WEBGL && SG_BLOCKCHAIN