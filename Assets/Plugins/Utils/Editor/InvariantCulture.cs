using UnityEngine;
using System.Collections;
using System.Globalization;
using System.Threading;
using UnityEditor;

public class InvariantCulture : MonoBehaviour
{
    [MenuItem("OakusGames/Invariant Culture")]
    public static void MakeInvariantCulture()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        MakeInvariantCulture();
    }
}
