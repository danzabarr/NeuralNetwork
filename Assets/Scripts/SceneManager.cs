using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneManager 
{
    public static NeuralNetworkAgent[] agents;
    public static Item[] food;

    public static void CacheArrays()
    {
        agents = Object.FindObjectsOfType<NeuralNetworkAgent>();
        food = Object.FindObjectsOfType<Item>();
    }
    public static T[] GetUnderlyingArray<T>(this List<T> list)
    {
        var field = list.GetType().GetField("_items",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        return (T[])field.GetValue(list);
    }

    public static T Nearest<T>(Vector3 origin, IEnumerable<T> list, ref float squareDistance) where T : Component
    {
        T nearest = null;
        foreach (T i in list)
        {
            float d = Vector3.SqrMagnitude(i.transform.position - origin);
            if (d < squareDistance)
            {
                squareDistance = d;
                nearest = i;
            }
        }
        return nearest;
    }
}

