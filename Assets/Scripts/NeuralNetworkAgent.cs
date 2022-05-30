using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class NeuralNetworkAgent : MonoBehaviour
{
    public float score = 0.0f;
    public float visionRange = 80;

    public NeuralNetwork network;
    public float[] values;
    public UnityEvent<float>[] actions;

    public virtual void Output(int index, float value) 
    {
        actions[index]?.Invoke(value);
    }

    public virtual float Input(int index)
    {
        return index switch
        {
            0 => AlwaysOn,
            1 => NearestFoodAngle,
            2 => AlignmentAngle,
            3 => CohesionAngle,
            4 => SeparationAngle,
            5 => NearbyAgentsDensity,
            6 => NearestAgentAngle,
            _ => 0,
        };
    }

    public void OnValidate()
    {
        if (network.weights == null || network.weights.Length != network.CountWeights)
            network.InitArrays();
        InitValues();
        RecalculateValues();
    }

    public void OnDrawGizmos()
    {
        float c = CohesionAngle * 180;

        Vector3 angle = Quaternion.Euler(0, c, 0) * transform.forward;

        Gizmos.DrawLine(transform.position, transform.position + angle);

    }

    public void OnDrawGizmosSelected()
    {
        float c = CohesionAngle;
        float a = AlignmentAngle;

        MoreGizmos.DrawCircle(transform.position, visionRange, (int)(visionRange * .5f));
        Vector3 alignment = Vector2.zero;
        Vector3 cohesion = Vector2.zero;
        int n = 0;
        if (SceneManager.agents != null)
            foreach (NeuralNetworkAgent agent in SceneManager.agents)
            {
                if (agent == null)
                    continue;

                if (agent == this)
                    continue;

                float sqDist = Vector3.SqrMagnitude(agent.transform.position - transform.position);

                if (sqDist <= visionRange * visionRange)

                {
                    cohesion += agent.transform.position;
                    alignment += agent.transform.forward;
                    n++;
                }
            }

        if (n > 0)
            cohesion /= n;
        alignment = alignment.normalized;

        float alignmentAngle = Vector3.SignedAngle(transform.forward, alignment, Vector3.up) / 180;
        float cohesionAngle = Vector3.SignedAngle(transform.forward, cohesion - transform.position, Vector3.up) / 180;

        Gizmos.DrawLine(transform.position, transform.position + alignment);
        Gizmos.DrawLine(transform.position, cohesion);
    }
    public float CohesionAngle
    {
        get
        {
            Vector3 cohesion = Vector2.zero;
            int n = 0;
            if (SceneManager.agents != null)
                foreach (NeuralNetworkAgent agent in SceneManager.agents)
                {
                    if (agent == null)
                        continue;

                    if (agent == this)
                        continue;

                    float sqDist = Vector3.SqrMagnitude(agent.transform.position - transform.position);

                    if (sqDist <= visionRange * visionRange)
                    {
                        cohesion += (agent.transform.position - transform.position);
                        n++;
                    }
                }

            if (n <= 0)
                return 0;

            cohesion /= n;
            //cohesion -= transform.position;
            //cohesion *= -1;

            return Vector3.SignedAngle(transform.forward, cohesion, Vector3.up) / 180;
        }
    }
    public float SeparationAngle
    {
        get
        {
            Vector3 separation = Vector2.zero;
            int n = 0;
            if (SceneManager.agents != null)
                foreach (NeuralNetworkAgent agent in SceneManager.agents)
                {
                    if (agent == null)
                        continue;

                    if (agent == this)
                        continue;

                    float sqDist = Vector3.SqrMagnitude(agent.transform.position - transform.position);

                    if (sqDist <= visionRange * visionRange)
                    {
                        separation += (transform.position - agent.transform.position) / (1 + sqDist);
                        n++;
                    }
                }

            if (n <= 0)
                return 0;

            separation /= n;
            //cohesion -= transform.position;
            //separation *= -1;

            return Vector3.SignedAngle(transform.forward, separation, Vector3.up) / 180;
        }
    }

    public float AlignmentAngle
    {
        get
        {
            Vector3 alignment = Vector2.zero;
            int n = 0;
            if (SceneManager.agents != null)
                foreach (NeuralNetworkAgent agent in SceneManager.agents)
                {
                    if (agent == null)
                        continue;

                    if (agent == this)
                        continue;
                        
                    float sqDist = Vector3.SqrMagnitude(agent.transform.position - transform.position);

                    if (sqDist <= visionRange * visionRange)

                    {
                        alignment += agent.transform.forward / (1 + sqDist);
                        n++;
                    }
                }

            if (n <= 0)
                return 0;

            alignment /= n;

            return Vector3.SignedAngle(transform.forward, alignment, Vector3.up) / 180;
        }
    }

    public void Update()
    {
        TriggerSensors();
        RecalculateValues();
        TriggerActions();
    }

    public void TriggerSensors()
    {
        for (int i = 0; i < network.inputs; i++)
            values[i] = Input(i);
    }

    public void TriggerActions()
    {
        if (actions == null || actions.Length != network.outputs)
            actions = new UnityEvent<float>[network.outputs];

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

    public void ClearValue(int index) => SetValue(index, 0);

    public void SetValue(int index, float value)
    {
        values[index] = value;
    }

    public void AddValue(int index, float value)
    {
        values[index] += value;
    }

    public static float RandomZeroToOne => Random.value;
    public static float RandomPlusMinusOne => Random.value * 2 - 1;
    public static float AlwaysOn => 1;
    public static float Zero => 0;
    public static float MinusOne => -1;

    public float PositionX
        => transform.position.x;
    public float PositionY
        => transform.position.x;
    public float PositionZ
            => transform.position.z;
    public float RotationY
        => transform.eulerAngles.y;

    public float NearbyAgentsCount
    {
        get
        {
            float squareRange = 10;
            int count = 0;

            foreach (NeuralNetworkAgent a in SceneManager.agents)
            {
                if (a == null)
                    continue;

                if (a == this)
                    continue;
                
                float squareDistance = Vector3.SqrMagnitude(a.transform.position - transform.position);
                if (squareDistance < squareRange)
                    count++;
            }


            return count;
        }
    }

    public float NearbyAgentsDensity
    {
        get
        {
            float density = 0;
            foreach (NeuralNetworkAgent a in SceneManager.agents)
            {
                if (a == null)
                    continue;

                if (a == this)
                    continue;

                float squareDistance = Vector3.SqrMagnitude(a.transform.position - transform.position);

                if (squareDistance == 0)
                    continue;

                density += 1f / squareDistance;
            }

            return density;
        }
    }

    public float NearestAgentAngle
    {
        get
        {
            Vector3 origin = transform.position;
            Vector3 forward = transform.forward;
            float d = visionRange * visionRange;
            NeuralNetworkAgent nearest = SceneManager.Nearest(origin, SceneManager.agents, ref d);
            if (nearest == null) return 0;
            Vector3 target = nearest.transform.position;
            return Vector3.SignedAngle(forward, target - origin, Vector3.up) / 180;
        }
    }

    public float NearestFoodInverseSquareDistance
    {
        get
        {
            Vector3 origin = transform.position;
            float d = visionRange * visionRange;
            Item nearest = Item.Nearest(origin, ref d);
            if (nearest == null) return 0;
            return Mathf.Lerp(0, 1, 1 / d);
        }
    }

    public float NearestFoodAngle
    {
        get
        {
            Vector3 origin = transform.position;
            Vector3 forward = transform.forward;
            float d = visionRange * visionRange;
            Item nearest = Item.Nearest(origin, ref d);
            if (nearest == null) return 0;
            Vector3 target = nearest.transform.position;
            return Vector3.SignedAngle(forward, target - origin, Vector3.up) / 180;
        }
    }

}
