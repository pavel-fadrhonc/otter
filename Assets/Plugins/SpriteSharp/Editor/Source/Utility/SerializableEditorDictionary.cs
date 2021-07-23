using System;
using System.Collections.Generic;
using UnityEngine;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    /// A Dictionary that can be successfully serialized by Unity.
    /// </summary>
    /// <typeparam name="TKey">
    /// The key type.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The value type.
    /// </typeparam>
    [Serializable]
    public class SerializableEditorDictionary<TKey, TValue> : EditorDictionary<TKey, TValue>, ISerializationCallbackReceiver {
        [SerializeField]
        private List<TKey> _keys = new List<TKey>();

        [SerializeField]
        private List<TValue> _values = new List<TValue>();

        /// <summary>
        /// Whether to check if keys and values are null during serialization.
        /// Useful for UnityEngine.Object ancestors.
        /// </summary>
        protected virtual bool AreValuesValidated {
            get {
                return false;
            }
        }

        // Save the dictionary to lists
        public void OnBeforeSerialize() {
            _keys.Clear();
            _values.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this) {
                if (pair.Key == null || (pair.Key != null && pair.Key.Equals(null)))
                    continue;

                if (AreValuesValidated && (pair.Value == null || (pair.Value != null && pair.Value.Equals(null))))
                    continue;

                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        // Load dictionary from lists
        public void OnAfterDeserialize() {
            Clear();

            if (_keys.Count != _values.Count)
                throw new Exception(
                    String.Format(
                        "There are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable.",
                        _keys.Count,
                        _values.Count
                        )
                    );

            for (int i = 0; i < _keys.Count; i++) {
                if (ReferenceEquals(_keys[i], null) || _keys[i].Equals(null))
                    continue;

                if (AreValuesValidated && (ReferenceEquals(_values[i], null) || _values[i].Equals(null)))
                    continue;

                Add(_keys[i], _values[i]);
            }

            _keys.Clear();
            _values.Clear();
        }
    }
}