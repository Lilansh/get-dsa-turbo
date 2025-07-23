using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Level Configs")]
    public LevelData[] availableLevels;

    [Header("UI")]
    public Transform levelButtonsParent; // vertical or grid layout group
    public GameObject levelButtonPrefab;
    public Text scoreText;
    public Button startButton;

    private LevelData selectedLevel;

    void Start()
    {
        PopulateLevelButtons();
        startButton.onClick.AddListener(StartGame);
    }

    void PopulateLevelButtons()
    {
        foreach (var level in availableLevels)
        {
            GameObject btnObj = Instantiate(levelButtonPrefab, levelButtonsParent);
            Text btnText = btnObj.GetComponentInChildren<Text>();
            Button button = btnObj.GetComponent<Button>();

            btnText.text = $"{level.gridWidth} x {level.gridHeight}";
            LevelData currentLevel = level; // copy for closure
            button.onClick.AddListener(() => OnLevelSelected(currentLevel));
        }

        OnLevelSelected(availableLevels[0]); // default
    }

    void OnLevelSelected(LevelData level)
    {
        selectedLevel = level;

        // Show best score & time
        int bestScore = PlayerPrefs.GetInt("BestScore_" + level.levelID, 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime_" + level.levelID, 0);

        scoreText.text = $"Best Score: {bestScore}\nBest Time: {bestTime:F2}s";
    }

    void StartGame()
    {
        if (selectedLevel != null)
        {
            GameSession.Instance.selectedLevel = selectedLevel;
            SceneManager.LoadScene("MainGame"); // your gameplay scene
        }
    }
}
