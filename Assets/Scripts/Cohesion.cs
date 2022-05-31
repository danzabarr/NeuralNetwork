using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cohesion : NeuralNetworkInput
{
    public override float Input()
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

                if (sqDist <= Agent.visionRange * Agent.visionRange)
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
