using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Game-agnostic utility functions to help make your Unity game!
 * Created by Brett Taylor.
 */
public static class LayerUtils {
    public static bool IsLayerInLayermask(int layer, LayerMask layermask) {
        return layermask == (layermask | (1 << layer));
    }
    public static bool IsLayer(GameObject go, string layerName) {
        return go.layer == LayerMask.NameToLayer(layerName);
    }
}
