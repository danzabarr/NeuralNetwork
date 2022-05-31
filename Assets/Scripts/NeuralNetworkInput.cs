using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NeuralNetworkAgent))]
public abstract class NeuralNetworkInput : MonoBehaviour
{
    public NeuralNetworkAgent Agent { get; private set; }
    public void Awake()
    {
        Agent = GetComponent<NeuralNetworkAgent>();
    }

    public abstract float Input();

}
