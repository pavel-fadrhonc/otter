using System;
using System.Collections;
using System.Collections.Generic;
using Utils;
using UnityRandom = UnityEngine.Random;

public static class ListExtensionMethods
{
    public static void EraseSwap<T>(this List<T> list, int index)
    {
        int lastIndex = list.Count - 1;
        list[index] = list[lastIndex];
        list.RemoveAt(lastIndex);
    }

    public static void Shuffle<T>(this List<T> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var t = list[i];
            var r = UnityRandom.Range(i, list.Count);
            list[i] = list[r];
            list[r] = t;
        }
    }
    
    public static T PopLast<T>(this IList<T> list)
    {
        int itemCount = list.Count;
        if (itemCount == 0)
        {
            throw new InvalidOperationException("List is empty!");
        }

        T item = list[itemCount - 1];
        list.RemoveAt(itemCount - 1);
        return item;
    }

    public static T PopFirst<T>(this IList<T> list)
    {
        int itemCount = list.Count;
        if (itemCount == 0)
        {
            throw new InvalidOperationException("List is empty!");
        }

        T item = list[0];
        list.RemoveAt(0);
        return item;
    }

    public static T Random<T>(this IList<T> list)
    {
        return list.Count > 0 ? list[UnityRandom.Range(0, list.Count)] : default;
    }

    public static NoAllocReadOnlyCollection<T> ToReadOnlyNoAlloc<T>(this List<T> list)
    {
        return new NoAllocReadOnlyCollection<T>(list);
    }
}

public static class ArrayExtensionMethods
{
    public static int IndexOf<T>(this T[] array, T needle)
    {
        for (var i = 0; i < array.Length; i++)
        {
            if (array[i].Equals(needle))
                return i;
        }
        return -1;
    }

    public static void Clear<T>(this T[] array)
    {
        System.Array.Clear(array, 0, array.Length);
    }
}