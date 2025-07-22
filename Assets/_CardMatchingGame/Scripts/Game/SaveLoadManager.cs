using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;

    private const string HIGH_SCORE_KEY = "HighScore";
    private const string BEST_TIME_KEY = "BestTime";
    private const string TOTAL_GAMES_KEY = "TotalGames";
    private const string CURRENT_LEVEL_KEY = "CurrentLevel";

    void Awake()
    {
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

    public void SaveScore(int score, float time)
    {
        int currentHighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        float currentBestTime = PlayerPrefs.GetFloat(BEST_TIME_KEY, float.MaxValue);
        int totalGames = PlayerPrefs.GetInt(TOTAL_GAMES_KEY, 0);

        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, score);
        }

        if (time < currentBestTime)
        {
            PlayerPrefs.SetFloat(BEST_TIME_KEY, time);
        }

        PlayerPrefs.SetInt(TOTAL_GAMES_KEY, totalGames + 1);
        PlayerPrefs.Save();
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    public float GetBestTime()
    {
        return PlayerPrefs.GetFloat(BEST_TIME_KEY, 0f);
    }

    public int GetTotalGames()
    {
        return PlayerPrefs.GetInt(TOTAL_GAMES_KEY, 0);
    }

    public void SaveGameState(int level)
    {
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, level);
        PlayerPrefs.Save();
    }

    public int LoadGameState()
    {
        return PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 1);
    }

    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}