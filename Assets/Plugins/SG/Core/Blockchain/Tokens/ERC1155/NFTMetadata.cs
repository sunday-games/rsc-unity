using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG.BlockchainPlugin
{
    public class NFTMetadata
    {
        public string Name { get; protected set; }
        public string Descr { get; protected set; }
        public DictSO Properties { get; protected set; } = new DictSO();

        public NFTMetadata(DictSO data)
        {
            if (data.TryGetString("name", out var name))
                Name = name;

            if (data.TryGetString("description", out var descr))
                Descr = descr;

            LoadProperties(data);
        }

        public void LoadProperties(DictSO data)
        {
            if (data.TryGetDict("properties", out var properties))
            {
                Properties = properties;
            }
            else if (data.TryGetList("attributes", out var attributes))
            {
                foreach (DictSO item in attributes)
                    Properties.Add(item.GetString("trait_type"), item.GetString("value"));
            }
        }
    }
}