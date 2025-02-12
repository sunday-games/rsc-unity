using UnityEngine;
using System;
using System.Linq;
using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.BlockchainPlugin
{
    [CreateAssetMenu(menuName = "Sunday/Blockchain/" + nameof(RequestExecutor), fileName = nameof(RequestExecutor), order = 0)]
    public class RequestExecutor : ScriptableObject
    {
        [TextArea(1, 100)] public string ContractABI;
        public TextAsset ContractABIFile;
        [TextArea(1, 100)] public string Functions;
        [UI.Button("ParseButton_OnClick")] public bool Parse;

        [Space(20)]
        public string FunctionName;
        public string[] ParamValues;

        [Space(20)]
        public Blockchain.Names BlockchainName;
        protected Blockchain Blockchain => BlockchainName.ToBlockchain();
        public BlockchainAddressDictionary Addresses, AddressesDebug;
        public string RequiredAccount;

        [Space(20)]
        [UI.Button("ExecuteButton_OnClick")] public bool Execute;

        [Space(20)]
        [TextArea(1, 100)] public string Result;

        private SmartContract _contract;

        public void ParseButton_OnClick()
        {
            _contract = new SmartContract()
            {
                Name = name,
                Blockchain = Blockchain,
                Address = (Configurator.production ? Addresses : AddressesDebug)[BlockchainName],
                ABI = ContractABIFile != null ? ContractABIFile.text : ContractABI
            };

            Functions = string.Empty;
            foreach (var f in _contract.Functions)
                Functions += f.Value.ToString() + Const.lineBreak;

            if (FunctionName.IsNotEmpty() &&
                _contract.Functions.TryGetValue(FunctionName, out var function) &&
                function.Inputs.Count != ParamValues.Length)
            {
                ParamValues = function.Inputs.Values.ToArray();
            }
        }

        public void ExecuteButton_OnClick()
        {
            if (!Blockchain || !Blockchain.loginedAccount)
            {
                Log.Blockchain.Error($"Please login first");
                return;
            }

            if (RequiredAccount.IsNotEmpty() && !Blockchain.loginedAccount.Address.IsEqualIgnoreCase(RequiredAccount))
            {
                Log.Blockchain.Error($"Please login to " + RequiredAccount);
                return;
            }

            if (_contract == null)
                ParseButton_OnClick();

            if (_contract == null ||
                !_contract.Functions.TryGetValue(FunctionName, out var function) ||
                function.Inputs.Count != ParamValues.Length)
            {
                Log.Blockchain.Error($"Wrong function " + FunctionName);
                return;
            }

            var content = new DictSO();
            int i = 0;
            foreach (var input in function.Inputs)
                content[input.Key] = ParamValues[i++];

            Result = "Waiting...";
            function.Read<object>(content, result => Result = result.Success ? result.Value.ToString() : "Fail")
                .Start();
        }
    }
}
