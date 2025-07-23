using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Card Data")]
    public CardData[] availableCards;

    private List<Card> allCards = new List<Card>();
    private List<Card> flippedCards = new List<Card>();
    private int score = 0;
    private float gameTime = 0;
    private bool gameActive = false;

    void Start()
    {
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

        // Freeze UI/input
        gameActive = false;
        score = 0;
        gameTime = 0;

        // Flash all card faces
        StartCoroutine(StartFlipPreview());

        AudioManager.Instance?.PlaySound("GameStart");
    }

    private IEnumerator StartFlipPreview()
    {
        yield return new WaitForSeconds(0.5f); // Slight delay before flipping begins

        List<Coroutine> flipCoroutines = new List<Coroutine>();

        foreach (Card card in allCards)
        {
            Coroutine flash = StartCoroutine(card.FlashCardFace(revealTime));
            flipCoroutines.Add(flash);
        }

        // Wait for all cards to flip back before activating game
        yield return new WaitForSeconds(revealTime + 0.4f); // total flash duration = one reveal cycle

        gameActive = true;
    }



    void ConfigureGrid()
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridWidth;

        // Calculate cell size based on screen size
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

        // Create pairs
        for (int i = 0; i < uniqueCards; i++)
        {
            CardData cardData = availableCards[i % availableCards.Length];
            cardPool.Add(cardData);
            cardPool.Add(cardData);
        }

        // Create card GameObjects
        for (int i = 0; i < totalCards; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardParent);
            Card card = cardObj.GetComponent<Card>();

            // Create unique CardData instance for this card
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

        // Rearrange in hierarchy
        for (int i = 0; i < allCards.Count; i++)
        {
            allCards[i].transform.SetSiblingIndex(i);
        }
    }

    public void OnCardClicked(Card clickedCard)
    {
        if (!gameActive || flippedCards.Count >= 2) return;

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
            score += 100;
            AudioManager.Instance?.PlaySound("Match");

            CheckWinCondition();
        }
        else
        {
            // No match
            flippedCards[0].FlipToBack();
            flippedCards[1].FlipToBack();
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
        }

        SaveBestScore();
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = "Score: " + score;
        if (timeText) timeText.text = "Time: " + gameTime.ToString("F1") + "s";
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
        string id = GameSession.Instance.selectedLevel.levelID;

        if (score > PlayerPrefs.GetInt("BestScore_" + id, 0))
            PlayerPrefs.SetInt("BestScore_" + id, score);

        if (gameTime < PlayerPrefs.GetFloat("BestTime_" + id, float.MaxValue))
            PlayerPrefs.SetFloat("BestTime_" + id, gameTime);

        PlayerPrefs.Save();
    }


}