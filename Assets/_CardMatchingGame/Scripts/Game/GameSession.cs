using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance;

    public LevelData selectedLevel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
