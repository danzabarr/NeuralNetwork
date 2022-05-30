using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public Item prefab;
    public int radius;
    public int count;
    public int clumpRadius;
    public Vector2Int clumpCount;

    private List<Item> items;

    public static List<T> Spawn<T>(T prefab, int n, Vector3 origin, Transform parent, float radius, ref List<T> list) where T : Component
    {
        if (list == null)
            list = new List<T>();

        for (int i = 0; i < n; i++)
        {
            Vector3 position = Random.insideUnitCircle * radius;
            position.z = position.y;
            position.y = 0;
            position += origin;
            Quaternion rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
            list.Add(Instantiate(prefab, position, rotation, parent));
        }
        return list;
    }

    public void Spawn()
    {
        items = new List<Item>();

        if (clumpCount.x > 1)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 position = Random.insideUnitCircle * radius;
                position.z = position.y;
                position.y = 0;
                position += transform.position;

                Spawn(prefab, Random.Range(clumpCount.x, clumpCount.y + 1), position, transform, clumpRadius, ref items);
            }
            return;
        }

        Spawn(prefab, count, transform.position, transform, radius, ref items);
    }

    public void Clear()
    {
        if (items == null)
            return;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i])
                Destroy(items[i].gameObject);
        }

        items = new List<Item>();
    }
}
