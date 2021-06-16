using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using of2.VFX;

namespace of2.VFX
{
    public class VFXEditor : EditorWindow
    {

        private VFXManagerData _vfxManagerData;
        private Vector2 _scrollViewPos;
        private string _newName;
        private GameObject _newPrefab;
        private string _searchString = "";
        private bool _itemWasRenamed = false;
        private GUIStyle _styleLight;

        private VFXData _currentPRTData;

        // Add menu item named "My Window" to the Window menu
        [MenuItem("of2/VFX/Manager")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(VFXEditor));
        }

        private void OnEnable()
        {
            _itemWasRenamed = false;
            //_vfxManagerData = Resources.Load<VFXManagerData>("VFXManager/VFXManagerData");

            _vfxManagerData = AssetDatabase.LoadAssetAtPath<VFXManagerData>("Assets/of2/VFXManager/VFXManagerData.asset");

            if (_vfxManagerData == null)
            {
                // first time setup
                _vfxManagerData = ScriptableObject.CreateInstance<VFXManagerData>();
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "of2/VFXManager"));
                AssetDatabase.CreateAsset(_vfxManagerData, "Assets/of2/VFXManager/VFXManagerData.asset");
                EditorUtility.SetDirty(_vfxManagerData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                var vfxInstallerPrefabGuid = AssetDatabase.FindAssets("VFXManagerInstaller").FirstOrDefault();
                var vfxInstallerPrefabPath = AssetDatabase.GUIDToAssetPath(vfxInstallerPrefabGuid);
                var vfxInstallerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(vfxInstallerPrefabPath);
                var instantiatedPrefab = PrefabUtility.InstantiatePrefab(vfxInstallerPrefab) as GameObject;
                var vfxManager = instantiatedPrefab.GetComponentInChildren<VFXManager>();
                vfxManager.VFXManagerData = _vfxManagerData;
                
                PrefabUtility.SaveAsPrefabAssetAndConnect(instantiatedPrefab,
                    "Assets/of2/VFXManager/VFXManagerInstaller.prefab", InteractionMode.AutomatedAction);
                DestroyImmediate(instantiatedPrefab);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            _styleLight = new GUIStyle();
            _styleLight.normal.background = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.1f));

        }

        Texture2D MakeTex(int width, int height, Color color)
        {
            var pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = color;
            }

            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }


        private void OnDisable()
        {
            if (_itemWasRenamed)
                GenerateVFXList();
            EditorUtility.SetDirty(_vfxManagerData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            {
                if (GUILayout.Button("Export enum", EditorStyles.toolbarButton, GUILayout.Width(120)))
                {
                    GenerateVFXList();
                }

                _searchString = GUILayout.TextField(_searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    // Remove focus if cleared
                    _searchString = "";
                    GUI.FocusControl(null);
                }

            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("VFXs:");
            EditorGUILayout.Space();

            _scrollViewPos = EditorGUILayout.BeginScrollView(_scrollViewPos);
            int displayCount = 0;
            for (int i = 0; i < _vfxManagerData.VFXs.Count; i++)
            {
                var vfx = _vfxManagerData.VFXs[i];
                if (!string.IsNullOrEmpty(_searchString))
                {
                    if (!vfx.Name.Contains(_searchString))
                        continue;
                }
                EditorGUILayout.BeginHorizontal(displayCount % 2 == 0 ? _styleLight : GUIStyle.none);
                displayCount++;
                string prevName = vfx.Name;
                vfx.Name = EditorGUILayout.TextField(vfx.Name);
                if (vfx.Name != prevName)
                {
                    // If any entry in the list gets renamed, we need to recreate the VFXList. We don't want to do that every time a name is changed though, so we remember the change and re-create the list when window is closed.
                    _itemWasRenamed = true;
                }
                
                string vfxType = "UNKNOWN";

                if (vfx.VFXPrefab == null)
                {
                    vfxType = "EMPTY !!";
                }
                else if (vfx.VFXPrefab.GetComponentInChildren<ParticleSystem>() != null) 
                {
                    vfxType = "ParticleSystem";
                    var system = vfx.VFXPrefab.GetComponentInChildren<ParticleSystem>();
                    vfxType += " (looping=" + system.main.loop;
                    vfxType += ", scaling=" + system.main.scalingMode + ")"; 
                }else if (vfx.VFXPrefab.GetComponentInChildren<LineRenderer>() != null)
                {
                    vfxType = "LineRenderer";
                }
                else if (vfx.VFXPrefab.GetComponentInChildren<MeshRenderer>() != null)
                {
                    vfxType = "MeshRenderer";
                }
                else if (vfx.VFXPrefab.GetComponentInChildren<Sprite>() != null)
                {
                    vfxType = "Sprite";
                }
                
                EditorGUILayout.LabelField(vfxType);

                vfx.VFXPrefab = EditorGUILayout.ObjectField(vfx.VFXPrefab, typeof(VFXHolder), false) as VFXHolder;
                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(25)))
                {
                    _vfxManagerData.VFXs.RemoveAt(i);
                    GenerateVFXList();
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Add new:");
            EditorGUILayout.BeginHorizontal();
            _newName = EditorGUILayout.TextField(_newName);
            GameObject prevPrefab = _newPrefab;
            _newPrefab = EditorGUILayout.ObjectField(_newPrefab, typeof(GameObject), false) as GameObject;
            if (_newPrefab != prevPrefab && _newPrefab != null)
            {
                _newName = _newPrefab.name;
            }
            GUI.enabled = !string.IsNullOrEmpty(_newName) && _newPrefab != null;
            if (GUILayout.Button("Add"))
            {
                if (IsNameAvailable(_newName))
                {
                    int guid = GetNewUniqueID();
                    if (guid > 0)
                    {
                        VFXHolder vfxHolder = _newPrefab.GetComponent<VFXHolder>();
                        if (vfxHolder == null)
                        {
                            if (EditorUtility.DisplayDialog("Missing component", "The selected prefab does not contain a VFX Holder component, which is required for the prefab to be used in the VFX Manager. A VFX Holder component will be added automatically, if you continue.", "OK", "Cancel"))
                            {
                                vfxHolder = _newPrefab.AddComponent<VFXHolder>();
                            }
                            else
                            {
                                return;
                            }
                        }
                        VFXData newData = new VFXData();
                        newData.Name = _newName;
                        newData.GUID = guid;
                        newData.VFXPrefab = vfxHolder;
                        _vfxManagerData.VFXs.Add(newData);
                        GenerateVFXList();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Name unavailable", "There is already a VFX with the name '" + _newName + "' in the list. Please choose another name.", "OK");
                }
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        bool IsNameAvailable(string name)
        {
            // If we don't find any objects with this name in the list, the name is available
            return _vfxManagerData.VFXs.Find(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) == null;
        }

        int GetNewUniqueID()
        {
            int newId = UnityEngine.Random.Range(1, int.MaxValue);
            int firstVal = newId;
            while (_vfxManagerData.VFXs.Find(p => p.GUID == newId) != null)
            {
                if (newId >= int.MaxValue)
                    newId = 1;
                else
                    newId++;

                if (firstVal == newId)
                {
                    Debug.LogError("No available IDs left!");
                    return -1;
                }
            }

            return newId;
        }

        void GenerateVFXList()
        {
            string beginMark = "/// VFX_LIST_START";
            string endMark = "/// VFX_LIST_END";

            string guid = AssetDatabase.FindAssets("VFXList").First();
            string filePath = AssetDatabase.GUIDToAssetPath(guid);
            string contents = String.Empty;
            
            int start = 0;
            int end = 0;
            
            if (!File.Exists(filePath))
            { // first time setup
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "of2/VFXManager"));
            }
            else
            { // load existing
                contents = File.ReadAllText(filePath);
                start = contents.IndexOf(beginMark) + beginMark.Length;
                end = contents.IndexOf(endMark);
                contents = contents.Remove(start, end - start);
            }

            // Write enum
            StringWriter sw = new StringWriter();
            sw.WriteLine(System.Environment.NewLine);
            sw.WriteLine("\tpublic enum EVFX {");
            sw.WriteLine("\t\tNONE" + " = " + 0 + ",");
            for (int i = 0; i < _vfxManagerData.VFXs.Count; i++)
            {
                string newLine = "\t\t" + _vfxManagerData.VFXs[i].Name.ToUpperInvariant() + " = " + _vfxManagerData.VFXs[i].GUID + ",";
                sw.WriteLine(newLine);
            }
            sw.WriteLine("\t}" + System.Environment.NewLine);

            // Write enum to string
            sw.WriteLine("\tpublic static string Get(EVFX vfx){");
            sw.WriteLine("\tstring vfxId = null;" + System.Environment.NewLine + "\tswitch(vfx){");

            for (int i = 0; i < _vfxManagerData.VFXs.Count; i++)
            {
                string enumString = _vfxManagerData.VFXs[i].Name.ToUpperInvariant();
                sw.WriteLine("\t\tcase EVFX." + enumString + ":");
                sw.WriteLine("\t\t\tvfxId = \"" + _vfxManagerData.VFXs[i].Name + "\";");
                sw.WriteLine("\t\t\tbreak;");
            }

            sw.WriteLine("\t}" + System.Environment.NewLine + "\treturn vfxId;");
            sw.WriteLine("\t}");

            contents = contents.Insert(start, sw.ToString());
            
            File.WriteAllText(filePath, contents);

            EditorUtility.SetDirty(_vfxManagerData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    [CustomPropertyDrawer(typeof(VFXList.EVFX))]
    public class VFXListDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Get the current selected index, then get the names, sort them and find new index
            int selectedIndex = property.enumValueIndex;
            List<string> names = new List<string>(Enum.GetNames(typeof(VFXList.EVFX)));
            names.Sort();
            if (selectedIndex > names.Count || selectedIndex < 0)
                selectedIndex = 0;
            selectedIndex = names.IndexOf(property.enumNames[selectedIndex]);
            selectedIndex = EditorGUI.Popup(position, selectedIndex, names.ToArray());
            VFXList.EVFX ps = (VFXList.EVFX)Enum.Parse(typeof(VFXList.EVFX), names[selectedIndex], true);
            selectedIndex = Array.IndexOf(Enum.GetValues(typeof(VFXList.EVFX)), ps);
            property.enumValueIndex = selectedIndex;

            EditorGUI.EndProperty();
        }
    }
}