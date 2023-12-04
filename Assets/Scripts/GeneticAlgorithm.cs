using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    public GameObject birdAi;
    public int playerNumber = 20;
    public int winnersNumber = 2;
    public float mutation_rate = 0.2f;
    public float mutationStrength = 2;
    public float bestScore = 0;
    public float regularizationWeight = 0;
    public int generation = 0;
    public float geneInitializationValue = 1;

    public GameObject[] runningBirds;
    private GameObject[] oldRunningBirds;
    private List<GameObject> birdsToKill;
    private GameObject[] winningBirds;
    public float[] currentBirdScores;

    public GameManager gameManager;

    void Start()
    {
        runningBirds = new GameObject[playerNumber];
        oldRunningBirds = new GameObject[playerNumber];
        birdsToKill = new List<GameObject>();
        winningBirds = new GameObject[playerNumber];
        currentBirdScores = new float[playerNumber];

        GeneratePlayers();
    }

    void FixedUpdate()
    {

        if (IsAllPlayerDead())
        {
            gameManager.GameOver();
            SelectWinners();

            generation += 1;
            Debug.Log("Generation " + generation);
            GenerateNewGenerationPlayer();
            KillBirds();
            
            gameManager.ContinueGame();
        }

        else
        {
            for (int i = 0; i < playerNumber; i++)
            {
                runningBirds[i].GetComponent<NeuralNetwork>().Think();
            }
        }

        runningBirds = runningBirds.OrderByDescending(go => go.GetComponent<Player>().score).ToArray();

        for (int i = 0; i < playerNumber; i++)
        {
            currentBirdScores[i] = runningBirds[i].GetComponent<Player>().score;
            if (currentBirdScores[i] > bestScore)
            {
                bestScore = currentBirdScores[i];
            }
        }

        UpdateCounter();
    }

    private void Update()
    {
        runningBirds = runningBirds.OrderByDescending(go => go.GetComponent<Player>().score).ToArray();
    }

    void GeneratePlayers()
    {
        for (int i = 0; i < playerNumber; i++)
        {
            runningBirds[i] = Instantiate(birdAi, new Vector3(-1, 0, 0), Quaternion.identity);
            runningBirds[i].name = ("bird " + generation + " " + i).ToString();
        }
    }

    void GenerateNewGenerationPlayer()
    {
        for (int i = 0; i < playerNumber; i++)
        {

            if (i < winnersNumber)
            {
                runningBirds[i].GetComponent<Player>().Reset();
            }
            else if (i >= winnersNumber & i < 2 * winnersNumber)
            {
                birdsToKill.Add(runningBirds[i]);

                runningBirds[i] = CloneBird(runningBirds[i - winnersNumber]);
                runningBirds[i].GetComponent<NeuralNetwork>().Mutate();
                runningBirds[i].GetComponent<Player>().alive = true;
                runningBirds[i].name = ("bird " + generation + " " + i).ToString();
            }
            else
            {
                oldRunningBirds[i] = runningBirds[i];
                birdsToKill.Add(runningBirds[i]);

                int j = Random.Range(0, winnersNumber);
                int k = Random.Range(0, winnersNumber);

                if (j < k)
                {
                    int t = j;
                    j = k;
                    k = t;
                }

                runningBirds[i] = BreedBird(winningBirds[j], winningBirds[k]);
                runningBirds[i].name = ("bird " + generation + " " + j + "_" + k).ToString();
                runningBirds[i].GetComponent<NeuralNetwork>().Mutate();
            }
        }
    }

    bool IsAllPlayerDead()
    {
        return !runningBirds.Any(x => x.GetComponent<Player>().alive);
    }

    void UpdateCounter()
    {
        var maxCount = runningBirds.Max(x => x.GetComponent<Player>().publicScore);
        gameManager.UpdateCounter(maxCount.ToString());
    }

    void SelectWinners()
    {
        runningBirds = runningBirds.OrderByDescending(go => go.GetComponent<Player>().score).ToArray();

        for (int i = 0; i < winnersNumber; i++)
        {
            winningBirds[i] = runningBirds[i];
        }

    }

    public GameObject CloneBird(GameObject motherBird)
    {
        GameObject babyBird = Instantiate(birdAi, new Vector3(-1, 0, 0), Quaternion.identity);
        babyBird.GetComponent<NeuralNetwork>().Clone(motherBird.GetComponent<NeuralNetwork>(), babyBird.GetComponent<Player>());
        return babyBird;
    }

    public GameObject BreedBird(GameObject motherBird, GameObject fatherBird)
    {
        GameObject babyBird = Instantiate(birdAi, new Vector3(-1, 0, 0), Quaternion.identity);
        babyBird.GetComponent<NeuralNetwork>().Breed(motherBird.GetComponent<NeuralNetwork>(), fatherBird.GetComponent<NeuralNetwork>(), babyBird.GetComponent<Player>());
        return babyBird;
    }

    void KillBirds()
    {
        foreach (GameObject go in birdsToKill)
        {
            Destroy(go);
        }
    }
}
