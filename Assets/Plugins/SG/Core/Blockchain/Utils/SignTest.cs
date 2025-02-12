#if SG_BLOCKCHAIN
using UnityEngine;
using System;

namespace SG.BlockchainPlugin
{
    [CreateAssetMenu(menuName = "Sunday/Blockchain/" + nameof(SignTest), fileName = nameof(SignTest), order = 0)]
    public class SignTest : ScriptableObject
    {
        public ServerData Server;
        [Serializable]
        public class ServerData
        {
            public string PrivateKey = "500a9f5d84ab6eb0105e3551807681fe727f2e8b60ab256f1e57ce7a62cdf9a0";

            public string PublicAddress =>
                new Nethereum.Signer.EthECKey(PrivateKey).GetPublicAddress();

            public string GetCertificate(string data) =>
                NethereumManager.SignHash(NethereumManager.GetMessageHash(data), PrivateKey);
        }

        [Space(20)] public string Data;

        [Space(20)] [UI.Button("GetCertificateButton_OnClick")] public bool GetCertificate;
        public void GetCertificateButton_OnClick()
        {
            if (Data.IsEmpty())
            {
                Log.Warning("Please enter data first");
                return;
            }

            Certificate = Server.GetCertificate(Data);
        }

        [Space(20)] public string Certificate;

        [Space(20)] [UI.Button("ValidateCertificateButton_OnClick")] public bool ValidateCertificate;
        public void ValidateCertificateButton_OnClick()
        {
            if (Data.IsEmpty())
            {
                Log.Warning("Please enter data first");
                return;
            }

            if (Certificate.IsEmpty())
            {
                Log.Warning("Please enter certificate first");
                return;
            }

            try
            {
                var signerAddress = NethereumManager.RecoverSigner(Data, Certificate);

                if (signerAddress.IsEqualIgnoreCase(Server.PublicAddress))
                    Log.Info("The certificate is valid");
                else
                    Log.Error("The certificate is NOT valid. Original signer is " + signerAddress);
            }
            catch
            {
                Log.Error("The certificate is NOT valid");
            }
        }
    }
}
#endif