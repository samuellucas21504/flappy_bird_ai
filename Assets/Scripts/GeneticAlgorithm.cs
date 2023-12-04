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
                runningBirds[i].GetComponent<Player>().alive = true;
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
        Debug.Log($"maxCount {maxCount}");
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Handles.color = Color.black;

        Vector2 dx = new Vector2(0.8f, 0);
        Vector2 dy = new Vector2(0, -0.3f);

        Vector2 pos = new Vector2(0.1f, 0.7f);
        Vector2 pos11 = pos + 1.5f * dy;
        Vector2 pos12 = pos + 2.5f * dy;
        Vector2 pos13 = pos + 3.5f * dy;
        Vector2 pos14 = pos + 4.5f * dy;
        Vector2 pos15 = pos + 5.5f * dy;
        Vector2 pos16 = pos + 6.5f * dy;
        Vector2 pos31 = pos + 2.5f * dy + 2 * dx;


        NeuralNetwork nn = runningBirds[0].GetComponent<NeuralNetwork>();

        for (int i = 0; i < nn.hiddenLayerSize; i++)
        {
            Vector2 pos2i = pos + dx + i * dy;

            DrawConnection(nn.weights1[0, i], pos11, pos2i, -dx / 3 - dy / 5);
            DrawConnection(nn.weights1[1, i], pos12, pos2i, -dy / 5);
            DrawConnection(nn.weights1[2, i], pos13, pos2i, -dx / 3 - dy / 5);
            DrawConnection(nn.weights1[3, i], pos14, pos2i, -dy / 5);
            DrawConnection(nn.weights1[0, i], pos15, pos2i, -dx / 3 - dy / 5);
            DrawConnection(nn.weights1[1, i], pos16, pos2i, -dy / 5);
            DrawConnection(nn.weights2[i], pos2i, pos31, -dy / 5 - dx / 5);
            
            DrawNode(nn.hiddenNode[i], pos2i);
        }
        DrawNode(nn.input_dx, pos11);
        DrawNode(nn.input_dy, pos12);
        DrawNode(nn.input_top_pipe_dx, pos13);
        DrawNode(nn.input_top_pipe_dy, pos14);
        DrawNode(nn.input_bottom_pipe_dx, pos15);
        DrawNode(nn.input_bottom_pipe_dy, pos16);
        DrawNode(nn.output, pos31);
    }

    void DrawConnection(float weight, Vector2 pos1, Vector2 pos2, Vector2 offset)
    {
        Gizmos.color = (weight > 0) ? Color.green : Color.red;
        Gizmos.DrawLine(pos1, pos2);
        Handles.Label((pos1 + pos2) / 2 + offset, weight.ToString("F2"));
    }

    void DrawNode(float value, Vector2 pos)
    {
        Gizmos.color = (value > 0) ? Color.green : Color.red;
        Gizmos.DrawSphere(pos, 0.1f);
        Handles.Label(pos + new Vector2(0, -0.1f), value.ToString("F2"));
    }
}
