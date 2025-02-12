using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG
{
    public abstract class DictionarizableObject
    {
        public abstract DictSO ToDictionary();
        public abstract DictionarizableObject FromDictionary(DictSO data);

        public DictionarizableObject() { }
    }
}
