using System;
using System.Collections.Generic;

namespace SG.BlockchainPlugin
{
    [Serializable]
    public class BlockchainAddressDictionary : UI.SerializableDictionary<Blockchain.Names, string> { }

#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(BlockchainAddressDictionary))]
    public class BlockchainAddressDictionaryDrawer : UI.DictionaryDrawer<Blockchain.Names, string> { }
#endif

    public static class BlockchainAddressDictionaryExtensions
    {
        public static Dictionary<Blockchain, SmartContract> ToContracts(this BlockchainAddressDictionary dict,
            string standart, string name = null)
        {
            var contracts = new Dictionary<Blockchain, SmartContract>(dict.Count);
            foreach (var address in dict)
            {
                var blockchain = address.Key.ToBlockchain();
                contracts[blockchain] = new SmartContract()
                {
                    Name = name ?? standart,
                    Blockchain = blockchain,
                    Address = address.Value,
                    ABI = SmartContract.StandartABI(standart)
                };
            }
            return contracts;
        }
    }
}