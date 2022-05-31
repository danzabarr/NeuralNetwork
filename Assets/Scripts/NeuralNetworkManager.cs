using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class NeuralNetworkManager : MonoBehaviour
{
    public Text stats;
    public bool randomise;
    public int generation = 1;
    public float timeScale;
    public float epochDuration;
    public int population;
    public float spawnRadius;
    
    public NeuralNetworkAgent prefab;
    public FoodSpawner[] spawners;

    private float counter;

    public void Start()
    {
        SceneManager.agents = Instantiate(population);
        foreach (FoodSpawner spawner in spawners)
            if (spawner) spawner.Spawn();
        UpdateStats();
    }

    private NeuralNetworkAgent InstantiateAgent(float[] weights)
    {
        Vector3 position = Random.insideUnitCircle * spawnRadius;
        position = new Vector3(position.x, 0, position.y);
        Quaternion rotation = Quaternion.AngleAxis(Random.value * 360, Vector3.up);
        NeuralNetworkAgent agent = Instantiate(prefab, position, rotation, transform);
        agent.network.weights = weights;
        return agent;
    }

    public void UpdateStats()
    {
        stats.text = "Generation " + generation;
    }

    public void EndEpoch()
    {
        generation++;
        Debug.Log("-----G" + generation + "-----");

        foreach (FoodSpawner spawner in spawners)
            if (spawner) spawner.Clear();

        List<int> matingPool = new List<int>();

        int survivors = 0;
        float highestRep = 0;

        //foreach (NeuralNetworkAgent agent in SceneManager.agents)
        for (int i = 0; i < SceneManager.agents.Length; i++)
        {
            float s = Mathf.Lerp(0, 1, SceneManager.agents[i].score / 1000) * 100;
            if (s > 0) survivors++;
            highestRep = Mathf.Max(highestRep, s);
            for (int j = 0; j < s; j++)
            {
                matingPool.Add(i);
            }
        }

        if (matingPool.Count == 0)
        {
            for (int i = 0; i < SceneManager.agents.Length; i++)
                matingPool.Add(i);
        }

        Debug.Log(matingPool.Count + " mating pool size");
        Debug.Log(highestRep + " highest representation");

        NeuralNetworkAgent[] nextGeneration = new NeuralNetworkAgent[population];

        Debug.Log(survivors + " survivors");

        int mutations = 0;

        for (int i = 0; i < population; i += 2)
        {
            int r1 = Random.Range(0, matingPool.Count);
            int r2 = Random.Range(0, matingPool.Count);

            NeuralNetworkAgent p1 = SceneManager.agents[matingPool[r1]];
            NeuralNetworkAgent p2 = SceneManager.agents[matingPool[r2]];

            (float[] w1, float[] w2)  = NeuralNetwork.Crossover(p1.network.weights, p2.network.weights);

            NeuralNetworkAgent c1 = InstantiateAgent(w1);
            NeuralNetworkAgent c2 = InstantiateAgent(w2);

            mutations += c1.Mutate();
            mutations += c2.Mutate();

            c1.InitValues();
            c2.InitValues();

            nextGeneration[i] = c1;
            nextGeneration[i + 1] = c2;
        }

        /*
        List<NeuralNetworkAgent> survivors = new List<NeuralNetworkAgent>();

        foreach (NeuralNetworkAgent agent in agents)
            if (agent.transform.position.x > 0)
                survivors.Add(agent);

        Debug.Log(survivors.Count + " survived");

        int i = 0;
        int n = population;

        NeuralNetworkAgent[] nextGeneration = new NeuralNetworkAgent[population];

        int mutations = 0;

        while(n > 0)
        {
            NeuralNetworkAgent agent = agents[i];

            Vector3 position = Random.insideUnitCircle * spawnRadius;
            position = new Vector3(position.x, 0, position.y);
            Quaternion rotation = Quaternion.AngleAxis(Random.value * 360, Vector3.up);

            agent = Instantiate(agent, position, rotation, transform);

            mutations += agent.network.Mutate(mutationChance, mutationMin, mutationMax, weightMin, weightMax);
            agent.InitValues();

            nextGeneration[population - n] = agent;

            i = (i + 1) % survivors.Count;
            n--;
        }

        */
        Debug.Log(mutations + " mutations");
        DestroyAgents();
        SceneManager.agents = nextGeneration;

        foreach (FoodSpawner spawner in spawners)
            if (spawner) spawner.Spawn();

        //SceneManager.CacheArrays();
        UpdateStats();
    }

    private void DestroyAgents()
    {
        for (int i = SceneManager.agents.Length - 1; i >= 0; i--)
            Destroy(SceneManager.agents[i]?.gameObject);
    }

    public void Update()
    {
        Time.timeScale = timeScale;

        counter += Time.deltaTime;
        if (counter >= epochDuration)
        {
            counter = 0;
            EndEpoch();
        }
    }

    public NeuralNetworkAgent[] Instantiate(int n)
    {
        NeuralNetworkAgent[] agents = new NeuralNetworkAgent[n];

        for (int i = 0; i < n; i++)
        {
            Vector3 position = Random.insideUnitCircle * spawnRadius;
            position = new Vector3(position.x, 0, position.y);
            Quaternion rotation = Quaternion.AngleAxis(Random.value * 360, Vector3.up);
            agents[i] = Instantiate(prefab, position, rotation, transform);
            if (randomise)
                agents[i].Randomise();
        }
        return agents;
    }
}
