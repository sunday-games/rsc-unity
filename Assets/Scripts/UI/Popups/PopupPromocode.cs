using UnityEngine;
using UnityEngine.UI;

namespace SG.RSC
{
    public class PopupPromocode : Popup
    {
        [Space(10)]
        public ParticleSystem starsParticles;
        public InputField inputField;
        public GameObject enterСode;
        public GameObject loading;
        public Text resultText;

        [Space(10)]
        public GameObject vkButton;
        public GameObject twitterButton;
        public GameObject facebookButton;

        public override void Init()
        {
            inputField.text = "";

            enterСode.SetActive(true);

            loading.SetActive(false);
            resultText.gameObject.SetActive(false);
            starsParticles.Stop();

            vkButton.SetActive(Localization.language == SystemLanguage.Russian);

            if (string.IsNullOrEmpty(server.links.twitter)) twitterButton.SetActive(false);
            if (string.IsNullOrEmpty(server.links.fbGroupShort)) facebookButton.SetActive(false);
            if (string.IsNullOrEmpty(server.links.vkGroup)) vkButton.SetActive(false);
        }

        public void EnterCode()
        {
            if (!enterСode.activeSelf) return;

            enterСode.SetActive(false);
            loading.SetActive(true);

            string specialMessage = Promocode.ActivateIfSpecial(inputField.text);

            if (!string.IsNullOrEmpty(specialMessage))
                ShowSpecialMessage(specialMessage);
            else if (inputField.text.Length > 5)
                server.VerifyPromocode(inputField.text, Callback);
            else
                Callback(null);
        }

        void Callback(Download download)
        {
            loading.SetActive(false);
            resultText.gameObject.SetActive(true);

            if (download == null || !download.isSuccess || download.responseDict == null)
            {
                resultText.text = Localization.Get("promocodeInvalid");
            }
            else
            {
                var promocode = new Promocode(download.responseDict);

                if (promocode.isAlreadyActivated)
                {
                    resultText.text = Localization.Get("promocodeInvalid");
                }
                else
                {
                    resultText.text = Localization.Get("promocodeValid");

                    promocode.Activate();
                }
            }

            Invoke("Init", 2f);
        }

        void ShowSpecialMessage(string message)
        {
            loading.SetActive(false);
            resultText.gameObject.SetActive(true);

            resultText.text = message;
            starsParticles.Play();

            Invoke("Init", 2f);
        }
    }
}