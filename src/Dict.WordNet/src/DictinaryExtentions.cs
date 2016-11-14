using System.Collections.Generic;

namespace DictionaryExtentions
{
    public static class DictionaryExtentions
    {
        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue: class
        {
            TValue value;
            return (dict.TryGetValue(key, out value)) ? value : null;
        }
    }
}
