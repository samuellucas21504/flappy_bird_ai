
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text counter;
    public GameObject player;
    public Vector2 initialPlayerPosition;
    public Spawner spawner;

    public void UpdateCounter(string counter)
    {
        this.counter.text = counter;
    }

    public void ContinueGame()
    {
        spawner.gameObject.SetActive(true);
    }

    public void GameOver()
    {
        Pipes[] pipes = FindObjectsOfType<Pipes>();

        for (int i = 0; i < pipes.Length; i++)
        {
            Destroy(pipes[i].gameObject);
        }
        spawner.gameObject.SetActive(false);
    }

}
