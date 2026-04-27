using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int batteriesNeeded = 3;
    public float timeToSurvive = 60f;

    private int currentBatteries = 0;
    private bool gameEnded = false;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (gameEnded) return;

        timeToSurvive -= Time.deltaTime;

        if (timeToSurvive <= 0f)
        {
            Debug.Log("TIME SURVIVED! Go to Safe Zone!");
            timeToSurvive = 0f;
        }
    }

    public void AddBattery(int amount)
    {
        currentBatteries += amount;
        Debug.Log("Batteries: " + currentBatteries + "/" + batteriesNeeded);
    }

    public void TryWinGame()
    {
        if (gameEnded) return;

        if (currentBatteries >= batteriesNeeded)
        {
            gameEnded = true;
            Debug.Log("YOU WIN!");
        }
        else
        {
            Debug.Log("Collect all batteries first!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.LoseGame();
        }
    }
    public void LoseGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        Debug.Log("YOU LOSE!");
    }
}