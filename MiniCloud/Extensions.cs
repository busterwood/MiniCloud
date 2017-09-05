using System.Collections.Specialized;
using System.Collections.Generic;
namespace MiniCloud
{
    static class Extensions
    {
        public static void AddRange(this StringDictionary dict, IEnumerable<KeyValuePair<string, string>> values)
        {
            foreach (var pair in values)
            {
                dict.Add(pair.Key, pair.Value);
            }
        }
    }
}