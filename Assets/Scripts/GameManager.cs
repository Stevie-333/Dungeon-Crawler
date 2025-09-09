using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int EnemiesKilled = 0;
    private float startTime;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        startTime = Time.time;
    }

    public float GetElapsedTime()
    {
        return Time.time - startTime;
    }

    public void AddKill()
    {
        EnemiesKilled++;
    }

    public int CalculateScore()
{
    int kills = EnemiesKilled;
    float time = GetElapsedTime();

    // More kills = better
    int killPoints = kills * 100;

    // Less time = better
    int timeBonus = Mathf.Max(0, 1000 - Mathf.FloorToInt(time));

    return killPoints + timeBonus;
}

}
