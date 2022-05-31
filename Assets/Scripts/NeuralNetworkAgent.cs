using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public abstract class NeuralNetworkAgent : MonoBehaviour
{
    public NeuralNetwork network;
    public float[] values;

    public float score = 0.0f;

    public Vector2 randomWeightLimits, weightLimits, mutationLimits;
    public float mutationChance;

    public abstract float Input(int index);
    public abstract void Output(int index, float value);
    public abstract string InputName(int index);
    public abstract string OutputName(int index);
    
    public void OnValidate()
    {
        if (network.weights == null || network.weights.Length != network.CountWeights)
            network.InitArrays();
        InitValues();
        RecalculateValues();
    }

    public void Update()
    {
        TriggerSensors();
        RecalculateValues();
        TriggerActions();
    }

    public int Mutate()
    {
        return network.Mutate(mutationChance, mutationLimits.x, mutationLimits.y, weightLimits.x, weightLimits.y);
    }

    public void Randomise()
    {
        network.Randomise(randomWeightLimits.x, randomWeightLimits.y, NeuralNetwork.Cube);
    }

    public void TriggerSensors()
    {
        for (int i = 0; i < network.inputs; i++)
            values[i] = Input(i);
    }

    public void TriggerActions()
    {
        for (int i = 0; i < network.outputs; i++)
        {
            int index = i + network.inputs + network.width * network.depth;
            float value = values[index];
            Output(i, value);
        }
    }

    public void InitValues()
    {
        values = new float[network.inputs + network.width * network.depth + network.outputs];
    }

    public void RecalculateValues()
    {
        if (values == null)
            InitValues();

        network.RecalculateValues(values);
    }

    public void ClearValue(int index)
    {
        SetValue(index, 0);
    }

    public float GetValue(int index)
    {
        return values[index];
    }

    public void SetValue(int index, float value)
    {
        values[index] = value;
    }

    public void AddValue(int index, float value)
    {
        values[index] += value;
    }
}
