using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NeuralNetworkEditorWindow : EditorWindow
{
    private NeuralNetworkAgent agent;
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

        bool needsRepaint = false;

        if (this.agent != agent)
        {
            this.agent = agent;
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
        DrawNode(value, n, d, r, default, null, null);
    }

    private void DrawNode(float value, int n, int d, int r, Rect labelPosition, string label, GUIStyle labelStyle)
    {
        var oldColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.Lerp(oldColor, Color.green, value);

        float x = NodeX(d);
        float y = NodeY(d, r);
        string buttonText = (Mathf.Round(value * 100) * 0.01).ToString();

        if (label != null)
        {
            labelPosition = new Rect(x + labelPosition.x, y + labelPosition.y, labelPosition.width, labelPosition.height);
            GUI.Label(labelPosition, label, labelStyle);
        }

        if (GUI.Button(new Rect(x - 15, y - 15, ButtonSize, ButtonSize), buttonText))
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
        int n = 0;

        GUIStyle inputLabelStyle = new GUIStyle(GUI.skin.label);
        inputLabelStyle.alignment = TextAnchor.MiddleRight;

        GUIStyle outputLabelStyle = GUI.skin.label;

        Rect inputLabelPosition = new Rect(-220, -10, 200, 30);
        Rect outputLabelPosition = new Rect(30, -10, 100, 30);

        for (int r = 0; r < inputs; r++)
        {
            string inputName = agent.InputName(r);
            
            DrawNode(agent.GetValue(r), n++, 0, r, inputLabelPosition, inputName, inputLabelStyle);
        }

        for (int d = 0; d < depth; d++)
            for (int r = 0; r < width; r++)
                DrawNode(agent.GetValue(inputs + d * width + r), n++, d + 1, r);

        for (int r = 0; r < outputs; r++)
        {
            string outputName = agent.OutputName(r);
            DrawNode(agent.GetValue(inputs + width * depth + r), n++, depth + 1, r, 
                outputLabelPosition, outputName, outputLabelStyle);
        }
    }

    public void DrawWeights()
    {
        int width = agent.network.width;
        int depth = agent.network.depth;
        int inputs = agent.network.inputs;
        int outputs = agent.network.outputs;

        float[] weights = agent.network.weights;

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
