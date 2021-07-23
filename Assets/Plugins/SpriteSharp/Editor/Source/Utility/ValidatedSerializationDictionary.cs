using System;
using System.Collections.Generic;
using System.Linq;
using LostPolygon.SpriteSharp.fastJSON;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    /// <summary>
    /// A dictionary that can remove null values during JSON serialization.
    /// </summary>
    /// <typeparam name="TKey">
    /// The key type.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The value type.
    /// </typeparam>
    internal class ValidatedSerializationDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IJSONSerializationCallbackReceiver {
        protected virtual bool AreValuesValidated {
            get {
                return false;
            }
        }

        protected virtual bool AdditionalSerializationValidationPredicate(KeyValuePair<TKey, TValue> pair) {
            return true;
        }

        protected virtual bool AdditionalDeserializationValidationPredicate(KeyValuePair<TKey, TValue> pair) {
            return true;
        }

        private void ValidateValues(Func<KeyValuePair<TKey, TValue>, bool> predicate) {
            KeyValuePair<TKey, TValue>[] validValues = this.Where(predicate).ToArray();
            if (validValues.Length == Count)
                return;

            Clear();
            foreach (KeyValuePair<TKey, TValue> pair in validValues) {
                Add(pair.Key, pair.Value);
            }
        }

        private bool SerializeValidationPredicate(KeyValuePair<TKey, TValue> pair) {
            if (pair.Key == null || (pair.Key != null && pair.Key.Equals(null)))
                return false;

            if (AreValuesValidated && (pair.Value == null || (pair.Value != null && pair.Value.Equals(null))) || !AdditionalSerializationValidationPredicate(pair))
                return false;

            return true;
        }

        private bool DeserializeValidationPredicate(KeyValuePair<TKey, TValue> pair) {
            if (ReferenceEquals(pair.Key, null) || pair.Key.Equals(null))
                return false;

            if (AreValuesValidated && (ReferenceEquals(pair.Value, null) || pair.Value.Equals(null)) || !AdditionalDeserializationValidationPredicate(pair))
                return false;

            return true;
        }

        #region IJSONSerializationCallbackReceiver

        public void OnBeforeSerialize() {
            ValidateValues(SerializeValidationPredicate);
        }

        public void OnAfterDeserialize() {
            ValidateValues(DeserializeValidationPredicate);
        }

        #endregion
    }
}