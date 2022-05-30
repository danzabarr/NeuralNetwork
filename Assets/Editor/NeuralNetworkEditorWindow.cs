using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NeuralNetworkEditorWindow : EditorWindow
{
    private NeuralNetworkAgent agent;
    private NeuralNetworkSave save;
    private  const float ButtonSize = 40;

    [MenuItem("Window/Neural Network Editor")]
    public static void ShowMyEditor()
    {
        NeuralNetworkEditorWindow editor = EditorWindow.GetWindow<NeuralNetworkEditorWindow>();
        editor.Init();
    }

    public void Init()
    {

    }
    private void Update()
    {
        if (agent != null)
            Repaint();
    }

    private void Revalidate()
    {
        NeuralNetworkAgent agent = Selection.activeGameObject?.GetComponent<NeuralNetworkAgent>();
        NeuralNetworkSave save = Selection.activeObject as NeuralNetworkSave;

        bool needsRepaint = false;

        if (this.agent != agent)
        {
            this.agent = agent;
            needsRepaint = true;
        }
        if (this.save != save)
        {
            this.save = save;
            needsRepaint = true;
        }
        if (needsRepaint)
            Repaint();
    }

    public void OnValidate() => Revalidate();
    public void OnSelectionChange() => Revalidate();
    public void OnHierarchyChange() => Revalidate();

    private float NodeX(int d)
    {
        return Mathf.Lerp(ButtonSize / 2, position.width - ButtonSize / 2, (d + 0.5f) / (agent.network.depth + 2));
    }

    private float NodeY(int d, int r)
    {
        float columnHeight = agent.network.width;

        if (d == 0)
            columnHeight = agent.network.inputs;

        if (d >= agent.network.depth + 1)
            columnHeight = agent.network.outputs;

        return Mathf.Lerp(ButtonSize / 2, position.height - ButtonSize / 2, (r + 0.5f) / columnHeight);
    }

    private void DrawNode(float value, int n, int d, int r) 
    {
        var oldColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.Lerp(oldColor, Color.green, value);

        float x = NodeX(d);
        float y = NodeY(d, r);
        string label = (Mathf.Round(value * 100) * 0.01).ToString();

        if (GUI.Button(new Rect(x - 15, y - 15, ButtonSize, ButtonSize), label))
        {
            if (value > 0)
                agent.SetValue(n, 0);
            else
                agent.SetValue(n, 1);

            agent.RecalculateValues();
            agent.TriggerActions();
            Repaint();
        }

        GUI.backgroundColor = oldColor;
    }

    private void DrawWeight(float weight, int d, int r0, int r1)
    {
        float x0 = NodeX(d);
        float y0 = NodeY(d, r0);

        float x1 = NodeX(d + 1);
        float y1 = NodeY(d + 1, r1);

        Vector3 v0 = new Vector3(x0, y0);
        Vector3 v1 = new Vector3(x1, y1);

        if (weight < 0)
            Handles.color = Color.Lerp(new Color(1, 0, 0, 0), Color.red, -weight);
        else
            Handles.color = Color.Lerp(new Color(0, 1, 0, 0), Color.green, weight);

        Handles.DrawLine(v0, v1);
        
        Handles.color = Color.white;

        return;
        string label =(Mathf.Round(weight * 100) * 0.01).ToString();
        Vector3 p = Vector3.Lerp(v0, v1, 4 / 5f) - new Vector3(10, 0);


        Handles.Label(p, label);

    }

    public void OnGUI()
    {
        if (agent == null)
        {
            GUI.Label(new Rect(position.width / 2 - 100, position.height / 2, 200, 20), "No Network is selected.");
            return;
        }

        DrawWeights();
        DrawNodes();
    }

    public void DrawNodes()
    {
        int inputs = agent.network.inputs;
        int depth = agent.network.depth;
        int width = agent.network.width;
        int outputs = agent.network.outputs;
        float[] values = agent.values;
        int n = 0;

        for (int r = 0; r < inputs; r++)
            DrawNode(values[r], n++, 0, r);

        for (int d = 0; d < depth; d++)
            for (int r = 0; r < width; r++)
                DrawNode(values[inputs + d * width + r], n++, d + 1, r);

        for (int r = 0; r < outputs; r++)
            DrawNode(values[inputs + width * depth + r], n++, depth + 1, r);
    }

    public void DrawWeights()
    {
        int width = agent.network.width;
        int depth = agent.network.depth;
        int inputs = agent.network.inputs;
        int outputs = agent.network.outputs;

        float[] values = agent.values;
        float[] weights = agent.network.weights;

        if (values == null)
            return;

        if (weights == null)
            return;


        int w = 0;
        int r0, r1;
        int d = 0;
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
            for (r0 = 0; r0 < inputs; r0++)
            {
                DrawWeight(weights[w], d, r0, r1);
                w++;
                n0++;
            }
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
                for (r0 = 0; r0 < width; r0++)
                {
                    DrawWeight(weights[w], d, r0, r1);
                    w++;
                    n0++;
                }
                n1++;
                n0 -= width;
            }
            n0 += width;
        }

        d = depth;

        for (r1 = 0; r1 < outputs; r1++)
        {
            for (r0 = 0; r0 < secondToLastLayerWidth; r0++)
            {
                DrawWeight(weights[w], d, r0, r1);
                w++;
                n0++;
            }

            n1++;
            n0 -= secondToLastLayerWidth;
        }
    }
}
