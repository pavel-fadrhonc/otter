using System.Collections.Generic;
using System.Linq;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    internal static class HashCodeUtility {
        public static int CalculateItemsHashCode<TKey, TValue>(IDictionary<TKey, TValue> dictionary) {
            if (dictionary == null)
                return 0;

            int hashCode = 0;
            foreach (var pair in dictionary.OrderBy(pair => pair.Key.GetHashCode())) {
                hashCode ^= pair.Key.GetHashCode();
                hashCode ^= (pair.Value != null ? pair.Value.GetHashCode() : 0);
            }

            return hashCode;
        }
    }
}