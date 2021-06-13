using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtensionsMethods
{
    public static Color FromHex(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        return Color.cyan;
    }

    public static Color WithAlpha(this Color thisColor, float a)
    {
        return new Color(thisColor.r, thisColor.g, thisColor.b, a);
    }
}