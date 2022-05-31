using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DefaultAgent : NeuralNetworkAgent
{
    public float maxMovementSpeed;
    public float maxRotationSpeed;
    public float visionRange = 80;

    public override string InputName(int index)
    {
        return index switch
        {
            0 => "Always On",
            1 => "Nearest Food Angle",
            2 => "Alignment Angle",
            3 => "Cohesion Angle",
            4 => "Separation Angle",
            5 => "Nearby Agents Density",
            6 => "Nearest Agent Angle",
            _ => "",
        };
    }
    public override float Input(int index)
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

    public override string OutputName(int index)
    {
        if (index == 0)
            return "Move Forward";

        if (index == 1)
            return "Rotate";

        return null;
    }

    public override void Output(int index, float value)
    {
        if (index == 0)
            MoveForward(value);
     
        else if (index == 1)
            Rotate(value);
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

    public void MoveForward(float amount)
    {
        amount = Mathf.Clamp(amount, 0, maxMovementSpeed);
        transform.position += transform.forward * amount * Time.deltaTime;
    }

    public void MoveBack(float amount)
    {
        amount = Mathf.Clamp(amount, 0, maxMovementSpeed);
        transform.position -= transform.forward * amount * Time.deltaTime;
    }

    public void MoveRight(float amount)
    {
        amount = Mathf.Clamp(amount, 0, maxMovementSpeed);
        transform.position += transform.right * amount * Time.deltaTime;
    }

    public void MoveLeft(float amount)
    {
        amount = Mathf.Clamp(amount, 0, maxMovementSpeed);
        transform.position -= transform.right * amount * Time.deltaTime;
    }

    public void Rotate(float amount)
    {
        amount *= 180;
        amount = Mathf.Clamp(amount, -maxRotationSpeed, maxRotationSpeed);
        transform.Rotate(Vector3.up, amount * Time.deltaTime);
    }
}
