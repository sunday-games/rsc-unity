using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Button = SG.UI.Button;
using Text = TMPro.TextMeshProUGUI;
using Dropdown = TMPro.TMP_Dropdown;

namespace SG.BlockchainPlugin.SundayWalletUI
{
    public class SettingsWindow : Window
    {
        [Space(10)]
        public Dropdown nodeDropdown;
        public Text blockchainText;
        [Space(10)]
        public Button setPinButton;

        private Blockchain.Network _network => Blockchain.current.network;

        protected override void Awake()
        {
            base.Awake();

            nodeDropdown.onValueChanged.AddListener(value => _network.SetNode(value));

            setPinButton.onClick.AddListener(ui.windows.pinSet.Open);
        }

        public override void Open(Dictionary<string, object> data)
        {
            base.Open(data);

            blockchainText.text = Blockchain.current.ToString();

            nodeDropdown.ClearOptions();
            for (int i = 0; i < _network.Nodes.Length; i++)
                nodeDropdown.options.Add(new Dropdown.OptionData(_network.Nodes[i].Host));

            nodeDropdown.interactable = _network.Nodes.Length > 1;
            nodeDropdown.value = _network.NodeIndex;
            nodeDropdown.RefreshShownValue();
            ui.UpdateStyle(nodeDropdown);

            setPinButton.transform.parent.gameObject.SetActive(Accounts.PIN.IsEmpty() && !Accounts.IsNeedPIN);
        }
    }
}