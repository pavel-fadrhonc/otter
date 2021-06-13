using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public static class GameObjectExtensionsMethods
{
    public static T CreateInvincibleInstance<T>(string name = null) where T : MonoBehaviour
    {
        GameObject go = new GameObject();
        go.name = string.IsNullOrEmpty(name) ? typeof(T).Name : name;
        go.AddComponent<T>();
        MonoBehaviour.DontDestroyOnLoad(go);
        return go.GetComponent<T>();
    }

    public static T GetComponentInParent<T>(this GameObject gameObject, bool includeInactive) where T : MonoBehaviour
    {
        if (!includeInactive)
        {
            return gameObject.GetComponentInParent<T>();
        }

        T component = null;
        Transform t = gameObject.transform;
        while (t != null)
        {
            component = t.GetComponent<T>();
            if (component != null)
            {
                break;
            }
            t = t.parent;
        }
        return component;
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : MonoBehaviour
    {
        var comp = gameObject.GetComponent<T>();
        if (comp == null)
            comp = gameObject.AddComponent<T>();

        return comp;
    }
}