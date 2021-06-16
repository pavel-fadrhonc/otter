using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Plugins.Zenject.OptionalExtras.Signals.Unity.Editor
{
    [CustomEditor(typeof(RaiseSignal))]
    public class RaiseSignalEditor : UnityEditor.Editor
    {
        List<string> _signalTypesAssembly;
        List<string> _signalTypesWithoutAssembly;         
        
        void OnEnable()
        {
            _signalTypesAssembly = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(ISignal).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => x.AssemblyQualifiedName).ToList();
            
            _signalTypesWithoutAssembly= AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(ISignal).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => x.FullName).ToList();                  
        }
        
        public override void OnInspectorGUI()
        {
            RaiseSignal raiseSignal = (RaiseSignal) target;
            
            var signalTypeField = typeof(RaiseSignal).GetField("_signalType", BindingFlags.Instance | BindingFlags.NonPublic);
            var signalTypeValue = signalTypeField.GetValue(raiseSignal) as string;
            
            var currentTypeIdx = signalTypeValue != null && _signalTypesAssembly.Contains(signalTypeValue) ? _signalTypesAssembly.IndexOf(signalTypeValue) : 0;

            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label(new GUIContent()
            {
                text = "Signal type: ",
                tooltip = "Allows to select from any signal in the project that implements ISignal interface. If it has parameters and Odin plugin is in the project it offers to setup the signal parameters as well."
            });
            
            var selectedTypeIdx = EditorGUILayout.Popup(currentTypeIdx, _signalTypesWithoutAssembly.ToArray());
            
            EditorGUILayout.EndHorizontal();

            if (signalTypeValue == null || currentTypeIdx != selectedTypeIdx)
            {
                signalTypeField.SetValue(raiseSignal, _signalTypesAssembly[selectedTypeIdx]);
                raiseSignal.Init();
            }

            var signalType = Type.GetType(signalTypeField.GetValue(raiseSignal) as string);
            if (signalType == null)
                return;
            
            var fireAbstractField = typeof(RaiseSignal).GetField("_fireAbstract", BindingFlags.Instance | BindingFlags.NonPublic);
            var fireAbstractValue = (bool) fireAbstractField.GetValue(raiseSignal);
            fireAbstractValue = EditorGUILayout.Toggle(
                new GUIContent("Fire Abstract",
                    "Fires signal as abstract. Signal needs to be declared with interfaces otherwise exception is thrown."),
                fireAbstractValue);
            fireAbstractField.SetValue(raiseSignal, fireAbstractValue);
            
#if ODIN_INSPECTOR

            var signalFields = signalType.GetFields();
            var parametersField = typeof(RaiseSignal).GetField("_parameters", BindingFlags.Instance | BindingFlags.NonPublic);
            if (parametersField.GetValue(raiseSignal) == null)
                parametersField.SetValue(raiseSignal, new object[signalFields.Length]);
            
            var parameters = parametersField.GetValue(raiseSignal) as object[];
            if (parameters.Length < signalFields.Length)
            {
                var newParamsArray = new object[signalFields.Length];
                parameters = newParamsArray;
            }
            for (var index = 0; index < signalFields.Length; index++)
            {
                var signalField = signalFields[index];
                object val = null;
                var fieldName = signalField.Name;

                if (typeof(UnityEngine.Object).IsAssignableFrom(signalField.FieldType))
                {
                    val = EditorGUILayout.ObjectField(fieldName,(parameters.Length > index ? parameters[index] : null) as UnityEngine.Object,
                        signalField.FieldType, true);
                }
                else if (signalField.FieldType.IsAssignableFrom(typeof(UnityEngine.Bounds)))
                {
                    val = EditorGUILayout.BoundsField(fieldName,(UnityEngine.Bounds)(parameters.Length > index && parameters[index] != null  ? parameters[index] : new Bounds()));                    
                }
                else if (signalField.FieldType.IsAssignableFrom(typeof(UnityEngine.Color)))
                {
                    val = EditorGUILayout.ColorField(fieldName,(UnityEngine.Color) (parameters.Length > index && parameters[index] != null  ? parameters[index] : Color.black));
                }
                else if (signalField.FieldType.IsAssignableFrom(typeof(UnityEngine.AnimationCurve)))
                {
                    val = EditorGUILayout.CurveField(fieldName,
                        (parameters.Length > index ? parameters[index] : null) as UnityEngine.AnimationCurve);
                }
                else if (signalField.FieldType.IsAssignableFrom(typeof(float)))
                {
                    val = EditorGUILayout.FloatField(fieldName,(float) (parameters.Length > index && parameters[index] != null  ? parameters[index] : 0f));
                }
                else if (signalField.FieldType.IsAssignableFrom(typeof(Gradient)))
                {
                    val = EditorGUILayout.GradientField(fieldName,
                        (parameters.Length > index ? parameters[index] : null) as UnityEngine.Gradient);
                }
                else if (signalField.FieldType.IsEnum)
                {
                    var enumIntVal = (int) (parameters.Length > index && parameters[index] != null ? parameters[index] : 0);
                    var enumStrings = Enum.GetNames(signalField.FieldType);
                    var enumInts = new List<int>(Enum.GetValues(signalField.FieldType) as int[]);
                    var selectedIndex = enumInts.IndexOf(enumIntVal);
                    if (selectedIndex == -1) selectedIndex = 0;
                    var newSelectedIndex = EditorGUILayout.Popup(fieldName, selectedIndex, enumStrings);
                    val = enumInts[newSelectedIndex];
                }
                else if (signalField.FieldType.IsAssignableFrom(typeof(int)))
                {
                    val = EditorGUILayout.IntField(fieldName,(int) (parameters.Length > index && parameters[index] != null ? parameters[index] : 0));
                }
                // else if (signalField.FieldType.IsAssignableFrom(typeof(int)))
                // {
                //     EditorGUILayout.LayerField((int) (parameters.Length > index ? parameters[index] : 0));
                // }
                else if (signalField.FieldType.IsAssignableFrom(typeof(Rect)))
                {
                    val = EditorGUILayout.RectField(fieldName,(Rect) (parameters.Length > index && parameters[index] != null  ? parameters[index] : new Rect()));
                }
                else if (signalField.FieldType.IsAssignableFrom(typeof(Vector2)))
                {
                    val = EditorGUILayout.Vector2Field(fieldName,(Vector2) (parameters.Length > index && parameters[index] != null  ? parameters[index] : Vector2.zero));
                }
                else if (signalField.FieldType.IsAssignableFrom(typeof(Vector3)))
                {
                    val = EditorGUILayout.Vector2Field(fieldName,(Vector3) (parameters.Length > index && parameters[index] != null  ? parameters[index] : Vector3.zero));
                }
                else if (signalField.FieldType.IsAssignableFrom(typeof(Vector4)))
                {
                    val = EditorGUILayout.Vector2Field(fieldName,(Vector4) (parameters.Length > index && parameters[index] != null  ? parameters[index] : Vector4.zero));
                }
                else
                {
                    GUILayout.Label($"Signal parameter {signalField.FieldType.Name} not supported.");
                    continue;
                }

                if (val != parameters[index])
                {
                    parameters[index] = val;
                    EditorUtility.SetDirty(target);        
                }
            }
            
            parametersField.SetValue(raiseSignal, parameters);
#endif
            if (Application.isPlaying)
            {
                GUILayout.Space(10);
                var raise = GUILayout.Button("Raise Signal");
                if (raise)
                    raiseSignal.Raise();
            }


#if !ODIN_INSPECTOR
            EditorGUILayout.HelpBox("Cannot serialize signal parameters. Include Odin and then you will be able to raise signal of any type inherited from ISignal with any parameters that are Unity-Serializable. Otherwise you can only raise signals without specifying parameters.", MessageType.Warning);
#endif

        }
    }
}