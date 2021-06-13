﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ScriptableObjectExtensionMethods
{

}

public static class ScriptableObjectHelper
{
    private const string RESOURCES = "Resources";

    public static T LoadOrCreateScriptableObjectInResources<T>(string assetPathWithoutExtension) where T : ScriptableObject
    {
        var asset = Resources.Load<T>(assetPathWithoutExtension);
        //Debug.Log("Loaded: " + asset + " at path: " + assetPathWithoutExtension);
#if UNITY_EDITOR
        if (asset == null)
        {
            string resPath = Path.Combine(Application.dataPath, RESOURCES);
            if (!Directory.Exists(resPath))
            {
                Directory.CreateDirectory(resPath);
            }

            asset = ScriptableObject.CreateInstance<T>();
            UnityEditor.AssetDatabase.CreateAsset(asset, "Assets/" + RESOURCES + "/" + assetPathWithoutExtension + ".asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
#endif
        return asset;
    }    
}