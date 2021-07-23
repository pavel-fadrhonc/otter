using System.Collections.Generic;
using System.Linq;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    /// <summary>
    /// Utilities for working with <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    internal static class IDictionaryUtility {
        public static void CalculateDiff<TKey, TValue>(
            IDictionary<TKey, TValue> dictionaryBase,
            IDictionary<TKey, TValue> dictionaryTarget,
            out KeyValuePair<TKey, TValue>[] added,
            out KeyValuePair<TKey, TValue>[] removed,
            out KeyValuePair<TKey, TValue>[] remained,
            IEqualityComparer<KeyValuePair<TKey, TValue>> comparer = null) {

            added =
                dictionaryBase
                .Except(dictionaryTarget, comparer)
                .ToArray();

            removed =
                dictionaryTarget
                .Except(dictionaryBase, comparer)
                .ToArray();

            remained =
                dictionaryBase
                .Intersect(dictionaryTarget, comparer)
                .ToArray();
        }
    }
}