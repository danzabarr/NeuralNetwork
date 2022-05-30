using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private static List<Item> list = new List<Item>();

    public int score;

    public void Start()
    {
        list.Add(this);
    }

    public void OnDestroy()
    {
        list.Remove(this);
    }

    public void OnTriggerEnter(Collider other)
    {
        NeuralNetworkAgent agent = other.GetComponentInParent<NeuralNetworkAgent>();
        if (agent)
        {
            agent.score += score;
            Destroy(gameObject);
        }
    }
    

    public static Item Nearest(Vector3 origin, ref float squareDistance)
    {
        return SceneManager.Nearest(origin, list, ref squareDistance);
    }
}
