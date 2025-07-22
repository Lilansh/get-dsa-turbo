using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "CardMatch/CardDatabase")]
public class CardDatabase : ScriptableObject
{
    // List to hold all card data ScriptableObjects
    [SerializeField]
    private List<CardData> allCards = new List<CardData>();

    // Return the list of all cards
    public List<CardData> GetAllCards()
    {
        return allCards;
    }

    // Retrieve a CardData by index
    public CardData GetCard(int index)
    {
        if (index >= 0 && index < allCards.Count)
        {
            return allCards[index];
        }
        else
        {
            Debug.LogWarning($"CardDatabase: Requested index {index} is out of range.");
            return null;
        }
    }

    // Retrieve a CardData by card ID (assuming CardData has unique ID)
    public CardData GetCardByID(int cardID)
    {
        return allCards.Find(card => card.cardID == cardID);
    }

    // Method to add a CardData (useful during development)
    public void AddCard(CardData card)
    {
        if (!allCards.Contains(card))
        {
            allCards.Add(card);
        }
    }
}
