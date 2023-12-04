using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    public GameManager manager;
    public GeneticAlgorithm algorithm;
    public Player bird;

    private int inputLayerSize = 6;
    public int hiddenLayerSize = 6;

    public float input_dx;
    public float input_dy;
    public float input_bottom_pipe_dx;
    public float input_bottom_pipe_dy;
    public float input_top_pipe_dx;
    public float input_top_pipe_dy;
    public float[,] weights1;
    public float[] weights2;
    public float[] bias1;
    public float[] hiddenNode;
    public float output;

    private void Awake()
    {
        manager = FindObjectOfType<GameManager>();
        algorithm = FindObjectOfType<GeneticAlgorithm>();
        bird = GetComponent<Player>();

        weights1 = new float[inputLayerSize, hiddenLayerSize];
        weights2 = new float[hiddenLayerSize];
        bias1 = new float[hiddenLayerSize];

        hiddenNode = new float[hiddenLayerSize];

        SetRandomParameters();
    }

    public void Think()
    {
        if (bird & bird.alive)
        {
            GameObject scoringZone = FindNextScoringZone();
            GameObject closestBottomPipe = FindClosestBottomPipe();
            GameObject closestTopPipe = FindClosestTopPipe();

            if (scoringZone != null)
            {
                input_dx = (scoringZone.transform.position.x - transform.position.x) / 3;
                input_top_pipe_dx = (closestTopPipe.transform.position.x - transform.position.x) / 3;
                input_bottom_pipe_dx = (closestBottomPipe.transform.position.x - transform.position.x) / 3;

                input_dy = transform.position.y - scoringZone.transform.position.y;
                input_top_pipe_dy = -(transform.position.y - closestTopPipe.transform.position.y) * 1.05f;
                input_bottom_pipe_dy = transform.position.y - (closestBottomPipe.transform.position.y * 1.05f);
            }
            else
            {
                input_dx = (3 - transform.position.x) / 3;
                input_bottom_pipe_dx = (3 - transform.position.x) / 3;
                input_top_pipe_dx = (3 - transform.position.x) / 3;

                input_dy = transform.position.y;
                input_bottom_pipe_dy = 0;
                input_top_pipe_dy = 0;
            }

            output = Eval();

            if (output > 0.5)
            {
                bird.Flap();
            }
        }
    }

    public GameObject FindClosestTopPipe()
    {
        GameObject[] pipes = GameObject.FindGameObjectsWithTag("TopPipe");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject pipe in pipes)
        {
            float currentDistance = pipe.transform.position.x - position.x;
            if (currentDistance < distance & currentDistance > -0.25f)
            {
                closest = pipe;
                distance = currentDistance;
            }
        }

        return closest;
    }

    public GameObject FindClosestBottomPipe()
    {
        GameObject[] pipes = GameObject.FindGameObjectsWithTag("BottomPipe");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject pipe in pipes)
        {
            float currentDistance = pipe.transform.position.x - position.x;
            if (currentDistance < distance & currentDistance > -0.25f)
            {
                closest = pipe;
                distance = currentDistance;
            }
        }

        return closest;
    }

    public GameObject FindNextScoringZone()
    {
        GameObject[] scoringZones = GameObject.FindGameObjectsWithTag("Scoring");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject zone in scoringZones)
        {
            float currentDistance = zone.transform.position.x - position.x;
            if (currentDistance < distance & currentDistance > -0.25f)
            {
                closest = zone;
                distance = currentDistance;
            }
        }

        return closest;
    }

    void SetRandomParameters()
    {
        for (int i = 0; i < inputLayerSize; i++)
        {
            for (int j = 0; j < hiddenLayerSize; j++)
            {
                weights1[i, j] = RandomGene();
            }
        }

        for (int i = 0; i < hiddenLayerSize; i++)
        {
            weights2[i] = RandomGene();
            bias1[i] = RandomGene();
        }
    }

    float Eval()
    {
        float outputLayer = 0;
        for (int j = 0; j < hiddenLayerSize; j++)
        {
            float sum = (weights1[0, j] * input_dx + weights1[1, j] * input_bottom_pipe_dx + weights1[2, j]) / 3 + weights1[3, j] * input_dy + weights1[4, j] * input_bottom_pipe_dy + weights1[5, j] * input_top_pipe_dy;
            float hiddenLayerOut1 = stepFunction(sum, bias1[j]);
            hiddenNode[j] = hiddenLayerOut1;

            outputLayer += weights2[j] * hiddenLayerOut1;
        }

        return outputLayer;
    }

    float stepFunction(float input, float tresh)
    {
        if (input > tresh)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void Mutate()
    {
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            weights2[i] = MutateGene(weights2[i]);
            bias1[i] = MutateGene(bias1[i]);
        }
        for (int i = 0; i < inputLayerSize; i++)
        {
            for (int j = 0; j < hiddenLayerSize; j++)
            {
                weights1[i, j] = MutateGene(weights1[i, j]);
            }
        }

        for (int i = 0; i < hiddenLayerSize; i++)
        {
            weights2[i] = MutateGene(weights2[i]);
            bias1[i] = MutateGene(bias1[i]);
        }
    }

    public float RandomGene()
    {
        float t = algorithm.geneInitializationValue;
        return Random.Range(-t, t);
    }

    public float MutateGene(float gene)
    {
        float r = Random.Range(0.0f, 1.0f);
        if (r < algorithm.mutation_rate)
        {
            float mutateFactor = 1 + (Random.Range(-algorithm.mutationStrength, algorithm.mutationStrength));
            gene = mutateFactor * gene;
        }
        return gene;
    }

    public void Clone(NeuralNetwork sourceNn, Player sourceBird)
    {

        bird = sourceBird;
        inputLayerSize = sourceNn.inputLayerSize;
        hiddenLayerSize = sourceNn.hiddenLayerSize;

        for (int i = 0; i < inputLayerSize; i++)
        {
            for (int j = 0; j < hiddenLayerSize; j++)
            {
                weights1[i, j] = sourceNn.weights1[i, j];
            }
        }

        for (int i = 0; i < hiddenLayerSize; i++)
        {
            weights2[i] = sourceNn.weights2[i];
            bias1[i] = sourceNn.bias1[i];
        }
    }

    public void Breed(NeuralNetwork motherNn, NeuralNetwork fatherNn, Player hostBird)
    {

        bird = hostBird;
        inputLayerSize = motherNn.inputLayerSize;
        hiddenLayerSize = motherNn.hiddenLayerSize;


        for (int i = 0; i < inputLayerSize; i++)
        {
            for (int j = 0; j < hiddenLayerSize; j++)
            {
                weights1[i, j] = (Random.Range(0, 2) == 0) ? motherNn.weights1[i, j] : fatherNn.weights1[i, j];
            }
        }

        for (int i = 0; i < hiddenLayerSize; i++)
        {
            weights2[i] = (Random.Range(0, 2) == 0) ? motherNn.weights2[i] : fatherNn.weights2[i];

            bias1[i] = (Random.Range(0, 2) == 0) ? motherNn.bias1[i] : fatherNn.bias1[i];
        }
    }

    public float RegularisationTerm()
    {
        float r = 0;
        for (int i = 0; i < inputLayerSize; i++)
        {
            for (int j = 0; j < hiddenLayerSize; j++)
            {
                r += Mathf.Abs(weights1[i, j]);
            }
        }

        for (int i = 0; i < hiddenLayerSize; i++)
        {
            r += Mathf.Abs(weights2[i]);
            r += Mathf.Abs(bias1[i]);
        }

        return algorithm.regularizationWeight * r;
    }

}
