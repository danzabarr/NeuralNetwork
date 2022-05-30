using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct NeuralNetwork
{
    public int inputs, outputs, width, depth;
    public float[] weights;

    public delegate float Mapping(float value);

    public readonly static Mapping Identity = (float value) => value;
    public readonly static Mapping OneToOneClamped = (float value) => Mathf.Clamp(value, 0, 1);
    public readonly static Mapping HeavisideStep = (float value) => value > 0 ? 1 : 0;
    public readonly static Mapping SmoothStep = (float value) => Mathf.SmoothStep(0, 1, value);
    public readonly static Mapping ReLu = (float value) => Mathf.Max(0, value);
    public readonly static Mapping TanH = (float value) => (float)System.Math.Tanh(value);
    public readonly static Mapping Square = (float value) => value * value;
    public readonly static Mapping Cube = (float value) => value * value * value;

    public readonly static Mapping Activation = TanH;

    public NeuralNetwork(int inputs, int outputs, int width, int depth)
    {
        inputs = Mathf.Clamp(inputs, 0, 1000);
        outputs = Mathf.Clamp(outputs, 0, 1000);
        width = Mathf.Clamp(width, 0, 1000);
        depth = Mathf.Clamp(depth, 0, 1000);

        this.inputs = inputs;
        this.outputs = outputs;
        this.width = width;
        this.depth = depth;

        int secondToLastLayerWidth = width;
        int secondLayerWidth = width;
        if (depth == 0)
        {
            secondLayerWidth = 0;
            secondToLastLayerWidth = inputs;
        }

        int inputWeights = inputs * secondLayerWidth;
        int hiddenWeights = width * width * Mathf.Max(depth - 1, 0);
        int outputWeights = outputs * secondToLastLayerWidth;

        weights = new float[inputWeights + hiddenWeights + outputWeights];
    }

    public int CountWeights
    {
        get
        {
            int secondToLastLayerWidth = width;
            int secondLayerWidth = width;
            if (depth == 0)
            {
                secondLayerWidth = 0;
                secondToLastLayerWidth = inputs;
            }

            int inputWeights = inputs * secondLayerWidth;
            int hiddenWeights = width * width * Mathf.Max(depth - 1, 0);
            int outputWeights = outputs * secondToLastLayerWidth;

            return inputWeights + hiddenWeights + outputWeights;
        }
    }

    public void InitArrays()
    {
        weights = new float[CountWeights];
    }

    public void Randomise(float min, float max, Mapping map)
    {
        weights = Randomised(weights.Length, min, max, map);
    }

    public int Mutate(float chance, float mMin, float mMax, float min, float max)
    {
        int mutations = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            if (Random.value < chance)
            {
                mutations++;
                weights[i] = Mathf.Clamp(weights[i] + Random.Range(mMin, mMax), min, max);
            }
        }

        return mutations;
    }

    public static float[] Randomised(int length, float min, float max, Mapping map)
    {
        if (length <= 0)
            throw new System.Exception("Array length must be greater than 0");

        float[] weights = new float[length];
        for (int i = 0; i < weights.Length; i++)
            weights[i] = Mathf.Lerp(min, max, map(Random.value));

        return weights;
    }

    public static float[] Copy(float[] weights, float mutationChance, float mutationMin, float mutationMax, float weightMin, float weightMax)
    {
        ValidateWeights(weights);

        float[] result = new float[weights.Length];

        for (int i = 0; i < weights.Length; i++)
        {
            float w = weights[i];
            if (Random.value < mutationChance)
                w += Random.Range(mutationMin, mutationMax);
            result[i] = Mathf.Clamp(w, weightMin, weightMax);
        }
        return result;
    }
    
    public static (float[], float[]) Crossover(float[] w1, float[] w2)
    {
        ValidateWeights(w1, w2);

        int p = Random.Range(0, w1.Length);

        float[] r1 = new float[w1.Length];
        float[] r2 = new float[w1.Length];

        for (int i = 0; i < w1.Length; i++)
        {
            if (i < p)
            {
                r1[i] = w1[i];
                r2[i] = w2[i];
            }
            else
            {
                r1[i] = w2[i];
                r2[i] = w1[i];
            }
        }

        return (r1, r2);
    }

    public static float[] Blend(float[] w1, float[] w2)
    {
        return Blend(w1, w2, .5f);
    }

    public static float[] Blend(float[] w1, float[] w2, float t)
    {
        ValidateWeights(w1, w2);

        float[] result = new float[w1.Length];

        for (int i = 0; i < w1.Length; i++)
            result[i] = Mathf.Lerp(w1[i], w2[i], t);

        return result;
    }

    public static float[] Blend(float[] w1, float[] w2, float t1, float t2)
    {
        float t = t1 / (t1 + t2);

        return Blend(w1, w2, t);

    }

    public static float[] RandomBlend(float[] w1, float[] w2)
    {
        ValidateWeights(w1, w2);

        float[] result = new float[w1.Length];

        for (int i = 0; i < w1.Length; i++)
            result[i] = Mathf.Lerp(w1[i], w2[i], Random.value);

        return result;
    }

    private static void ValidateWeights(float[] w1)
    {
        if (w1 == null)
            throw new System.Exception("Weights array was null.");
    }

    private static void ValidateWeights(float[] w1, float[] w2)
    {
        if (w1 == null || w2 == null)
            throw new System.Exception("Weights array was null.");

        if (w1.Length != w2.Length)
            throw new System.Exception("Weights arrays are different lengths.");
    }

    public void RecalculateValues(float[] values) => RecalculateValues(values, Activation);
    public void RecalculateValues(float[] values, Mapping activation)
    {
        if (activation == null)
            throw new System.Exception("Activation function is null.");

        if (values == null)
        {
            Debug.LogWarning("Values array was null.");
            return;
        }

        int w = 0;
        
        int d, r0, r1;
        
        int n0 = 0;
        int n1 = inputs;

        int secondToLastLayerWidth = width;
        int secondLayerWidth = width;

        if (depth == 0)
        {
            secondLayerWidth = 0;
            secondToLastLayerWidth = inputs;
        }

        for (r1 = 0; r1 < secondLayerWidth; r1++)
        {
            float sum = 0;
            for (r0 = 0; r0 < inputs; r0++)
            {
                sum += weights[w] * values[n0];
                w++;
                n0++;
            }
            values[n1] = activation(sum);
            n1++;
            n0 -= inputs;
        }

        if (depth > 0)
        {
            n0 += inputs;
        }

        for (d = 1; d < depth; d++)
        {
            for (r1 = 0; r1 < width; r1++)
            {
                float sum = 0;
                for (r0 = 0; r0 < width; r0++)
                {
                    sum += weights[w] * values[n0];
                    w++;
                    n0++;
                }
                values[n1] = activation(sum);
                n1++;
                n0 -= width;
            }
            n0 += width;
        }

        d = depth + 1;

        for (r1 = 0; r1 < outputs; r1++)
        {
            float sum = 0;
            for (r0 = 0; r0 < secondToLastLayerWidth; r0++)
            {
                sum += weights[w] * values[n0];
                w++;
                n0++;
            }
            values[n1] = activation(sum);
            n1++;
            n0 -= secondToLastLayerWidth;
        }

        /*
        int layer = 0;
        int from = 0;
        int to = 0;

        int secondLayerWidth = width;
        int secondToLastLayerWidth = width;
        if (depth <= 0) {
            secondLayerWidth = outputs;
            secondToLastLayerWidth = inputs;
        }

        for (int i = inputs; i < values.Length; i++)
            values[i] = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            if (i < secondLayerWidth * inputs)
            {
                from = i % inputs;
                to = i / inputs + inputs;
            }
            else if (i >= weights.Length - outputs * secondToLastLayerWidth)
            {
                int j = i - secondLayerWidth * inputs;
                from = j / (width * width) * width + inputs + j % width;
            }
            else
            {
                int j = i - width * inputs;

                layer = j / width + 1;

                from = j / (width * width) * width + inputs + j % width;
                to = j / width + inputs + width;
            }

            Debug.Log(i + " (" + layer + ") " + from + ">" + to);
            values[to] += Activation(weights[i] * values[from]);
        }
        */
    }
}
