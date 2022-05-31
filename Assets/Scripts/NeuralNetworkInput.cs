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
    public abstract string Description();
    public abstract float Input();

}

public class NetworkInput
{
    public delegate float Function();

    public Function function;
    public string name;
    public string description;
    public bool active;
}

public class NetworkOutput
{
    public delegate void Output(float value);
    public Output function;
    public string name;
    public string description;
    public bool active;
}
