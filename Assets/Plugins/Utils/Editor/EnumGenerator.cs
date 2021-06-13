using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace of2.Utils
{
    public class EnumGeneratorWindow : EditorWindow
    {
        private const string EnumAssemblyNameContains = "Assembly-CSharp";
        
        private static Type _currentType;
        private static string _currentTypeName = "";
        private static string _enumSearchFilter;

        private static List<Type> allEnums;
        private static List<string> _allEnumNames;
        
        private static List<string> _existingNames;
        private static List<int> _existingValues;

        private static bool _showEditEnum;
        private static bool _showAddEnum;
        
        private static string _existingTypeNewRecordName;

        private static string _newEnumSavePath;
        private static string _newEnumTypeName;
        private static string _newEnumTypeNamespace;
        private static List<string> _newEnumNames = new List<string>();
        private static List<int> _newEnumValues = new List<int>();
        private static string _newEnumRecordName;
        // Add menu named "My Window" to the Window menu
        [MenuItem("of2/Enum Generator")]
        static void Init()
        {
            OnReload();
            
            // Get existing open window or if none, make a new one:
            EnumGeneratorWindow window = (EnumGeneratorWindow) EditorWindow.GetWindow(typeof(EnumGeneratorWindow));
            window.minSize = new Vector2(600, 600);
            window.Show();
        }
        
        [InitializeOnLoadMethod]
        static void OnReload()
        {
            allEnums = GetAllEnums();
            _allEnumNames = allEnums.Select(e => e.FullName).ToList();
        }

        void OnGUI()
        {
            //EditorGUILayout.LabelField("Manage Enum");
            EditorGUI.HelpBox(new Rect(new Vector2(300, 5), new Vector2(500, 50)),
                "Enum Generator allows you to add or remove enum values from you enum without worrying about order. " +
                "For that it regenerates whole file that contains the enum so make sure to only edit enums that are in separate files.",
                MessageType.Info);
            
            EditorGUILayout.Space(55);
            // enum save directory
            if (String.IsNullOrEmpty(_newEnumSavePath))
                _newEnumSavePath = Application.dataPath;

            #region Edit Enum Part

            _showEditEnum = EditorGUILayout.BeginFoldoutHeaderGroup(_showEditEnum, "Edit Enum");

            if (_showEditEnum)
            {
                _showAddEnum = false;
                
                EditorGUILayout.Space(30);

                // select enum dialog
                var currentIndex = String.IsNullOrEmpty(_currentTypeName) ? 0 : _allEnumNames.IndexOf(_currentTypeName);
                EditorGUILayout.BeginHorizontal();
                var filteredEnumNames = _allEnumNames;
                    if (!String.IsNullOrEmpty(_enumSearchFilter))
                        filteredEnumNames = _allEnumNames.Where(en => en.Contains(_enumSearchFilter)).ToList();
                var selectedEnumIdx = EditorGUILayout.Popup("Manage enum", currentIndex, filteredEnumNames.ToArray());
                _enumSearchFilter = EditorGUILayout.TextField("Filter:", _enumSearchFilter);
                EditorGUILayout.EndHorizontal();
                _currentTypeName = _allEnumNames[selectedEnumIdx];
                _currentType = allEnums[selectedEnumIdx];

                // display selected enum values and names
            
                EditorGUILayout.Space(30);
            
                EditorGUILayout.LabelField(_currentType.Name);

                DisplayEnumValuesAndNames();
                
                // add new
                _existingTypeNewRecordName = EditorGUILayout.TextField("New Record Name:", _existingTypeNewRecordName);
                if (GUILayout.Button("Add"))
                {
                    // check for duplicate
                    if (_existingNames.Contains(_existingTypeNewRecordName))
                        EditorGUILayout.LabelField($"<color=\"red\">Enum record {_existingTypeNewRecordName} already added.</color>", new GUIStyle() {richText = true});
                    else
                    {
                        int newEnumVal = 0;

                        do
                        {
                            newEnumVal = Random.Range(1, Int32.MaxValue - 1);
                        } while (_existingValues.Contains(newEnumVal));

                        _existingNames.Add(_existingTypeNewRecordName);   
                        _existingValues.Add(newEnumVal);
                    }
                }
                
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            #endregion
            
            EditorGUILayout.Space(20);

            #region Add Enum Part

            _showAddEnum = EditorGUILayout.BeginFoldoutHeaderGroup(_showAddEnum, "Add Enum");

            if (_showAddEnum)
            {
                _showEditEnum = false;
                
                EditorGUILayout.BeginHorizontal();
            
                EditorGUILayout.LabelField("Enum Save Folder",_newEnumSavePath);
            
                if (GUILayout.Button("Choose save folder"))
                {
                    _newEnumSavePath = EditorUtility.OpenFolderPanel("Choose enum save folder", _newEnumSavePath, "");
                }

                EditorGUILayout.EndHorizontal();
                
                _newEnumTypeName = EditorGUILayout.TextField("Enum Name:", _newEnumTypeName);
                _newEnumTypeNamespace = EditorGUILayout.TextField("Enum Namespace:", _newEnumTypeNamespace);
                
                // display added
                for (int i = 0; i < _newEnumNames.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _newEnumNames[i] = EditorGUILayout.TextField("Name:", _newEnumNames[i]);
                    EditorGUILayout.LabelField("Value:", _newEnumValues[i].ToString());
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Space(10);
            
                // display new
                EditorGUILayout.BeginHorizontal();
                _newEnumRecordName = EditorGUILayout.TextField("New Record Name:", _newEnumRecordName);
                if (GUILayout.Button("Add"))
                {
                    // check for duplicate
                    if (_newEnumNames.Contains(_newEnumRecordName))
                        EditorGUILayout.LabelField($"<color=\"red\">Enum record {_newEnumRecordName} already added.</color>", new GUIStyle() {richText = true});
                    else
                    {
                        int newEnumVal = 0;

                        do
                        {
                            newEnumVal = Random.Range(1, Int32.MaxValue - 1);
                        } while (_newEnumValues.Contains(newEnumVal));

                        _newEnumNames.Add(_newEnumRecordName);   
                        _newEnumValues.Add(newEnumVal);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion
            
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(30);
            
            // save enum values and names into file
            if (GUILayout.Button("Save"))
            {
                if (_showEditEnum)
                    SaveEditedEnum();
                else if (_showAddEnum)
                    SaveNewEnum();
            }
        }

        private static void DisplayEnumValuesAndNames()
        {
            var names = GetNames();
            var values = GetValues();

            for (int i = 0; i < names.Count; i++)
            {
                var name = names[i];
                var val = values[i];

                EditorGUILayout.BeginHorizontal();

                names[i] = EditorGUILayout.TextField("Name:", name);
                EditorGUILayout.LabelField("Value:", val.ToString());
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void SaveNewEnum()
        {
            StringWriter sw = new StringWriter();
            
            var tab = "";
            var typeName = _newEnumTypeName;
            if (!String.IsNullOrEmpty(_newEnumTypeNamespace))
            {
                tab += "\t";

                sw.WriteLine($"namespace {_newEnumTypeNamespace}");
                sw.WriteLine("{");
            }

            sw.WriteLine($"{tab}public enum {typeName} ");
            sw.WriteLine(tab + "{");

            var names = _newEnumNames;
            var values = _newEnumValues;
            
            // enum content
            for (int i = 0; i < names.Count; i++)
            {
                sw.WriteLine($"{tab}\t{names[i]} = {values[i]},");
            }

            sw.WriteLine(tab + "}" + System.Environment.NewLine);

            if (!String.IsNullOrEmpty(_newEnumTypeNamespace))
            {
                sw.WriteLine("}");
            }

            File.WriteAllText(_newEnumSavePath + Path.DirectorySeparatorChar + _newEnumTypeName + ".cs", sw.ToString());       
            
            AssetDatabase.Refresh();            
        }
        
        private static void SaveEditedEnum()
        {
            var filename = _currentType.Name + ".cs";
            var assets = AssetDatabase.FindAssets(_currentType.Name);
            if (assets == null || assets.Length == 0)
            {
                Debug.Log($"Could not find file {filename}. Are you sure that the enum is in the file that is named same as the enum?");
                return;
            }

            string guid = assets.First();
            string filePath = AssetDatabase.GUIDToAssetPath(guid);

            StringWriter sw = new StringWriter();
            
            var tab = "";
            var typeName = _currentTypeName;
            if (!String.IsNullOrEmpty(_currentType.Namespace))
            {
                tab += "\t";
                typeName = _currentTypeName.Substring(_currentTypeName.LastIndexOf(".") + 1);
                
                sw.WriteLine($"namespace {_currentType.Namespace}");
                sw.WriteLine("{");
            }

            sw.WriteLine($"{tab}public enum {typeName} ");
            sw.WriteLine(tab + "{");

            var names = GetNames();
            var values = GetValues();
            
            // enum content
            for (int i = 0; i < names.Count; i++)
            {
                sw.WriteLine($"{tab}\t{names[i]} = {values[i]},");
            }

            sw.WriteLine(tab + "}" + System.Environment.NewLine);

            if (!String.IsNullOrEmpty(_currentType.Namespace))
            {
                sw.WriteLine("}");
            }

            File.WriteAllText(filePath, sw.ToString());       
            
            AssetDatabase.Refresh();
        }
        
        private static List<Type> GetAllEnums()
        {
            var enums = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains(EnumAssemblyNameContains))
                .SelectMany(x => x.GetTypes())
                .Where(t => t.IsEnum && t.IsPublic);
            return new List<Type>(enums);
        }
        
        private static List<string> GetNames()
        {
            if (_existingNames == null || _existingNames.Count == 0)
                _existingNames = Enum.GetNames(_currentType).ToList();

            return _existingNames;
        }
        
        private static List<int> GetValues()
        {
            if (_existingValues == null || _existingValues.Count == 0)
                _existingValues = (Enum.GetValues(_currentType) as int[]).ToList();

            return _existingValues;
        }

        // private static void SetNames(List<string> names)
        // {
        //     _names.Clear();
        //     _names.AddRange(names);
        // }
        //
        // private static void SetValues(List<int> values)
        // {
        //     _values.Clear();
        //     _values.AddRange(values);
        // }
    }
}