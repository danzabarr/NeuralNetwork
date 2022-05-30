using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName ="Neural Network", menuName ="Neural Network")]
public class NeuralNetworkSave : ScriptableObject
{
    public NeuralNetwork network;
}
