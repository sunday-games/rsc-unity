using System;
using System.Linq;
using System.Collections.Generic;

namespace SG.BlockchainPlugin
{
    public class SmartContract : Account, IDictionarizable<SmartContract>
    {
        public static string StandartABI(string standart)
        {
            var abiFileName = "abi_" + standart;
            var abiFile = (UnityEngine.TextAsset) UnityEngine.Resources.Load(abiFileName);

            if (abiFile == null)
            {
                Log.Error("Fail to find Resources/" + abiFileName);
                return null;
            }

            return abiFile.text;
        }

        private string _abi;
        public string ABI
        {
            get => _abi;
            set
            {
                _abi = value;

                if (_abi.IsNotEmpty())
                {
                    try
                    {
                        var data = Json.Deserialize(_abi);

                        object functionList = null;

                        if (Blockchain == Blockchain.eosio || Blockchain == Blockchain.wax)
                            functionList = (data as Dictionary<string, object>)["structs"];
                        else if (Blockchain == Blockchain.neo)
                            functionList = (data as Dictionary<string, object>)["methods"];
                        else
                            functionList = data;

                        foreach (Dictionary<string, object> functionData in (List<object>) functionList)
                            if (functionData.IsValue("name"))
                            {
                                var function = new Function(this, functionData);

                                if (!Functions.ContainsKey(function.Name))
                                    Functions.Add(function.Name, function);
                            }
                    }
                    catch
                    {
                        Log.Error($"SmartContract - {Blockchain}.{Name} - Fail to parse ABI: {_abi}");
                    }
                }
            }
        }

        public Dictionary<string, object> Meta;

        public Dictionary<string, Function> Functions = new Dictionary<string, Function>();
        public class Function
        {
            public SmartContract Contract;

            public string Name;
            public bool Payable = false;
            public Dictionary<string, string> Inputs = new Dictionary<string, string>();
            public Dictionary<string, string> Outputs = new Dictionary<string, string>();
            //public Func<ulong, ulong> FeeModifier = null;
            public ulong Fee;

            public Function() { }

            public Function(SmartContract contract, Function function)
            {
                Contract = contract;
                Name = function.Name;
                Payable = function.Payable;
                Inputs = new Dictionary<string, string>(function.Inputs);
            }

            public Function(SmartContract contract, Dictionary<string, object> data)
            {
                Contract = contract;
                Name = data.GetString("name");
                Payable = data.GetBool("payable");


                object inputTypesList = null;
                if (data.IsValue("inputs"))
                    inputTypesList = data["inputs"];
                else if (data.IsValue("fields"))
                    inputTypesList = data["fields"];
                else if (data.IsValue("parameters"))
                    inputTypesList = data["parameters"];

                if (inputTypesList != null)
                    foreach (Dictionary<string, object> inputData in inputTypesList as List<object>)
                        Inputs[inputData["name"].ToString()] = inputData["type"].ToString();

                object outputTypesList = null;
                if (data.IsValue("outputs"))
                    outputTypesList = data["outputs"];

                if (outputTypesList != null)
                    foreach (Dictionary<string, object> outputData in outputTypesList as List<object>)
                        Outputs[outputData["name"].ToString()] = outputData["type"].ToString();
            }

            //public Function SetFeeModifier(Func<ulong, ulong> modifier) { FeeModifier = modifier; return this; }

            public override string ToString()
            {
                var output = Outputs.Count > 0 ? Outputs.First().Value : "void";

                var input = string.Empty;
                if (Inputs.Count > 0)
                {
                    foreach (var i in Inputs)
                        input += i.Value + " " + i.Key + ", ";
                    input = input.Substring(0, input.Length - 2);
                }

                return output + " " + Name + "(" + input + ")";
            }

            public static implicit operator bool(Function f) => f != null;
        }

        public SmartContract() { }

        public new SmartContract FromDictionary(Dictionary<string, object> data)
        {
            Name = data.GetString("name");

            Address = data.GetString("address");

            Blockchain = Blockchain.Deserialize(data.GetString("bcId"));
            if (!Blockchain)
                return null;

            if (data.IsValue("abi"))
                ABI = Json.Serialize(data["abi"]);
            else if (data.TryGetString("standart", out string standart))
                ABI = StandartABI(standart);

            if (data.Count > 4)
            {
                data.Remove("name");
                data.Remove("address");
                data.Remove("bcId");
                data.Remove("abi");
                data.Remove("standart");
                Meta = data;
            }

            return this;
        }

        public new Dictionary<string, object> ToDictionary() => throw new NotImplementedException();
    }
}