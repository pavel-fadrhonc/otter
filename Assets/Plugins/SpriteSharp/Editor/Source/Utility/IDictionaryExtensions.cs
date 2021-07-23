using System.Collections.Generic;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    ///     <see cref="IDictionaryExtensions" /> helper extensions.
    /// </summary>
    internal static class IDictionaryExtensions {
        public static bool Compare<TKey, TValue>(this IDictionary<TKey, TValue> dict1, IDictionary<TKey, TValue> dict2) {
            if (dict1 == dict2) return true;
            if (dict1 == null || dict2 == null) return false;
            if (dict1.Count != dict2.Count) return false;

            EqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;
            foreach (KeyValuePair<TKey, TValue> kvp in dict1) {
                TValue value2;
                if (!dict2.TryGetValue(kvp.Key, out value2)) return false;
                if (!valueComparer.Equals(kvp.Value, value2)) return false;
            }

            return true;
        }
    }
}
