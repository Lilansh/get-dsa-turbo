using UnityEngine;

[CreateAssetMenu(fileName = "New Card Data", menuName = "Card Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card Visual")]
    public Sprite frontSprite;
    public Sprite backSprite;

    [Header("Card Properties")]
    public int cardID;
    public string cardName;
    public Color cardColor = Color.white;

    [Header("Gameplay")]
    public bool isMatched = false;
    public bool isFlipped = false;
}