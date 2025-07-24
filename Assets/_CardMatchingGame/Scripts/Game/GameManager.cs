using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int gridWidth = 4;
    public int gridHeight = 4;
    public float revealTime = 1.0f;

    [Header("References")]
    public Transform cardParent;
    public GameObject cardPrefab;
    public GridLayoutGroup gridLayout;
    public Text scoreText;
    public Text timeText;
    public Text comboText;  // Optional: assign in Inspector to show combo info

    [Header("Card Data")]
    public CardData[] availableCards;

    private List<Card> allCards = new List<Card>();
    private List<Card> flippedCards = new List<Card>();
    private int score = 0;
    private float gameTime = 0;
    private bool gameActive = false;

    [Header("GameOver UI")]
    public GameObject GameOverUI;
    public Text EndScoreText;
    public Text EndTimeText;

    // Combo system variables
    private int currentCombo = 0;
    private int maxCombo = 0;
    private int comboMultiplier = 1;
    private int baseScorePerMatch = 100;

    void Start()
    {
        if (GameOverUI != null)
            GameOverUI.SetActive(false);

        LevelData level = GameSession.Instance.selectedLevel;

        if (level != null)
        {
            gridWidth = level.gridWidth;
            gridHeight = level.gridHeight;
        }

        InitializeGame();
    }

    void Update()
    {
        if (gameActive)
        {
            gameTime += Time.deltaTime;
            UpdateUI();
        }
    }

    public void InitializeGame()
    {
        ClearExistingCards();
        ConfigureGrid();
        CreateCards();
        ShuffleCards();

        // Reset combo and score variables
        currentCombo = 0;
        maxCombo = 0;
        comboMultiplier = 1;
        score = 0;
        gameTime = 0;

        // Freeze UI/input during flash preview
        gameActive = false;

        // Flash all cards to show faces briefly
        StartCoroutine(StartFlipPreview());

        AudioManager.Instance?.PlaySound("GameStart");

        // Clear Game Over UI if showing
        if (GameOverUI != null)
            GameOverUI.SetActive(false);
    }

    private IEnumerator StartFlipPreview()
    {
        yield return new WaitForSeconds(0.5f); // Slight delay before flipping begins

        foreach (Card card in allCards)
        {
            StartCoroutine(card.FlashCardFace(revealTime));
            // Optional: stagger each card's flip a little for effect
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(revealTime + 0.3f); // Wait till all cards reverted

        gameActive = true;
    }

    void ConfigureGrid()
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridWidth;

        RectTransform parentRect = cardParent.GetComponent<RectTransform>();

        float cellWidth = (parentRect.rect.width - gridLayout.spacing.x * (gridWidth - 1)) / gridWidth - gridLayout.padding.horizontal;
        float cellHeight = (parentRect.rect.height - gridLayout.spacing.y * (gridHeight - 1)) / gridHeight - gridLayout.padding.vertical;

        float cellSize = Mathf.Min(cellWidth, cellHeight);
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
    }

    void CreateCards()
    {
        int totalCards = gridWidth * gridHeight;
        int uniqueCards = totalCards / 2;

        List<CardData> cardPool = new List<CardData>();

        // Create pairs of cards
        for (int i = 0; i < uniqueCards; i++)
        {
            CardData cardData = availableCards[i % availableCards.Length];
            cardPool.Add(cardData);
            cardPool.Add(cardData);
        }

        for (int i = 0; i < totalCards; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardParent);
            Card card = cardObj.GetComponent<Card>();

            CardData uniqueCardData = ScriptableObject.CreateInstance<CardData>();
            uniqueCardData.frontSprite = cardPool[i].frontSprite;
            uniqueCardData.backSprite = cardPool[i].backSprite;
            uniqueCardData.cardID = cardPool[i].cardID;
            uniqueCardData.cardName = cardPool[i].cardName;
            uniqueCardData.cardColor = cardPool[i].cardColor;

            card.cardData = uniqueCardData;
            card.OnCardClicked += OnCardClicked;
            card.SetupCard();

            allCards.Add(card);
        }
    }

    void ShuffleCards()
    {
        FisherYatesShuffle.Shuffle(allCards);

        for (int i = 0; i < allCards.Count; i++)
        {
            allCards[i].transform.SetSiblingIndex(i);
        }
    }

    public void OnCardClicked(Card clickedCard)
    {
        if (!gameActive || flippedCards.Count >= 2)
            return;

        // Prevent clicking the same card twice in a row
        if (flippedCards.Contains(clickedCard))
            return;

        clickedCard.FlipCard();
        flippedCards.Add(clickedCard);

        AudioManager.Instance?.PlaySound("CardFlip");

        if (flippedCards.Count == 2)
        {
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {
        gameActive = false;

        yield return new WaitForSeconds(revealTime);

        if (flippedCards[0].cardData.cardID == flippedCards[1].cardData.cardID)
        {
            // Match found
            flippedCards[0].SetMatched();
            flippedCards[1].SetMatched();

            // Combo system logic
            currentCombo++;
            maxCombo = Mathf.Max(maxCombo, currentCombo);
            comboMultiplier = 1 + (currentCombo / 3); // Increase multiplier every 3 combos (adjust logic as you wish)
            int comboScore = baseScorePerMatch * comboMultiplier;
            score += comboScore;

            AudioManager.Instance?.PlaySound("Match");

            CheckWinCondition();
        }
        else
        {
            // No match
            flippedCards[0].FlipToBack();
            flippedCards[1].FlipToBack();

            // Reset combo
            currentCombo = 0;
            comboMultiplier = 1;

            AudioManager.Instance?.PlaySound("Mismatch");
        }

        flippedCards.Clear();
        gameActive = true;
    }

    void CheckWinCondition()
    {
        bool allMatched = true;
        foreach (Card card in allCards)
        {
            if (!card.cardData.isMatched)
            {
                allMatched = false;
                break;
            }
        }

        if (allMatched)
        {
            gameActive = false;

            AudioManager.Instance?.PlaySound("GameOver");

            SaveLoadManager.Instance?.SaveScore(score, gameTime);

            // Show win UI
            if (GameOverUI != null)
                GameOverUI.SetActive(true);

            if (EndScoreText != null)
                EndScoreText.text = "Final Score: " + score;

            if (EndTimeText != null)
                EndTimeText.text = "Time Taken: " + gameTime.ToString("F1") + "s";

            SaveBestScore();
        }
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = "Score: " + score;
        if (timeText) timeText.text = "Time: " + gameTime.ToString("F1") + "s";

        if (comboText != null)
            comboText.text = currentCombo > 1 ? $"Combo x{comboMultiplier}" : "";
    }

    void ClearExistingCards()
    {
        foreach (Transform child in cardParent)
        {
            DestroyImmediate(child.gameObject);
        }
        allCards.Clear();
        flippedCards.Clear();
    }

    private void SaveBestScore()
    {
        if (GameSession.Instance == null || GameSession.Instance.selectedLevel == null)
            return;

        string id = GameSession.Instance.selectedLevel.levelID;

        if (score > PlayerPrefs.GetInt("BestScore_" + id, 0))
            PlayerPrefs.SetInt("BestScore_" + id, score);

        if (gameTime < PlayerPrefs.GetFloat("BestTime_" + id, float.MaxValue))
            PlayerPrefs.SetFloat("BestTime_" + id, gameTime);

        PlayerPrefs.Save();
    }

    public void ReturntoMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        if (GameSession.Instance != null)
            GameSession.Instance.selectedLevel = null;
    }
}
