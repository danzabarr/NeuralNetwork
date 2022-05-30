using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MoreGizmos
{
    public static void DrawCircle(Vector3 origin, float radius, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle0 = 2 * Mathf.PI * i / segments;
            float angle1 = 2 * Mathf.PI * ((i + 1) % segments) / segments;

            Gizmos.DrawLine(
                new Vector3(Mathf.Cos(angle0), 0, Mathf.Sin(angle0)) * radius + origin,
                new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius + origin);
        }
    }
}
