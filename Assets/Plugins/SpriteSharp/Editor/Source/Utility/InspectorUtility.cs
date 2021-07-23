using System;
using System.Collections.Generic;
using UnityEditor;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    /// <summary>
    /// Helper for drawing multi-object fields.
    /// </summary>
    /// <typeparam name="T">The data container type.</typeparam>
    internal static class InspectorUtility<T> {
        /// <summary>
        /// Checks whether the field values are different and returns the
        /// field value of the last element.
        /// </summary>
        /// <param name="dataContainers">
        /// The data containers.
        /// </param>
        /// <param name="valueGetterFunc">
        /// The function that retrieves a specific field from <typeparamref name="T"/>.
        /// </param>
        /// <param name="value">
        /// The field value of the last <paramref name="dataContainers"/> element.
        /// </param>
        /// <returns>
        /// True if field values are different, false otherwise.
        /// </returns>
        /// <typeparam name="TField">The data container type.</typeparam>
        public static bool IsMixedValues<TField>(
            T[] dataContainers,
            Func<T, TField> valueGetterFunc,
            out TField value
            ) {
            value = default(TField);
            bool isValueSet = false;
            bool isValueMixed = false;

            foreach (T dataContainer in dataContainers) {
                if (isValueSet && !EqualityComparer<TField>.Default.Equals(value, valueGetterFunc(dataContainer))) {
                    isValueMixed = true;
                }

                value = valueGetterFunc(dataContainer);
                isValueSet = true;
            }

            return isValueMixed;
        }

        /// <summary>
        /// Draws a single field.
        /// </summary>
        /// <param name="dataContainers">
        /// The data containers.
        /// </param>
        /// <param name="valueGetterFunc">
        /// The function that retrieves a specific field from <typeparamref name="T"/>.
        /// </param>
        /// <param name="valueSetterAction">
        /// The field value of the last <paramref name="dataContainers"/> element.
        /// </param>
        /// <param name="drawerFunc">
        /// The function that draws a GUI for the field.
        /// </param>
        /// <returns>
        /// True if GUI was changed, false otherwise.
        /// </returns>
        /// <typeparam name="TField">The data container type.</typeparam>
        public static bool DrawField<TField>(
            T[] dataContainers,
            Func<T, TField> valueGetterFunc,
            Action<int, T, TField> valueSetterAction,
            Func<TField, TField> drawerFunc
            ) {
            TField newValue;
            return DrawField(dataContainers, valueGetterFunc, valueSetterAction, drawerFunc, out newValue);
        }

        /// <summary>
        /// Draws a single field.
        /// </summary>
        /// <param name="dataContainers">
        /// The data containers.
        /// </param>
        /// <param name="valueGetterFunc">
        /// The function that retrieves a specific field from <typeparamref name="T"/>.
        /// </param>
        /// <param name="valueSetterAction">
        /// The field value of the last <paramref name="dataContainers"/> element.
        /// </param>
        /// <param name="drawerFunc">
        /// The function that draws a GUI for the field.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        /// <returns>
        /// True if GUI was changed, false otherwise.
        /// </returns>
        /// <typeparam name="TField">The data container type.</typeparam>
        public static bool DrawField<TField>(
            T[] dataContainers,
            Func<T, TField> valueGetterFunc,
            Action<int, T, TField> valueSetterAction,
            Func<TField, TField> drawerFunc,
            out TField newValue
            ) {
            bool isChanged = false;
            TField value;
            bool isValueMixed = IsMixedValues(dataContainers, valueGetterFunc, out value);

            if (isValueMixed) {
                EditorGUI.showMixedValue = true;
            }

            EditorGUI.BeginChangeCheck();
            value = drawerFunc(value);
            if (EditorGUI.EndChangeCheck()) {
                for (int i = 0; i < dataContainers.Length; i++) {
                    T settings = dataContainers[i];
                    valueSetterAction(i, settings, value);
                }

                isChanged = true;
            }

            EditorGUI.showMixedValue = false;
            newValue = value;

            return isChanged;
        }
    }
}