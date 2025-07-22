using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public Image cardImage;
    public CardData cardData;

    [Header("Animation Settings")]
    public float flipDuration = 0.3f;
    public AnimationCurve flipCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isFlipping = false;
    private bool isClickable = true;

    public System.Action<Card> OnCardClicked;

    void Start()
    {
        SetupCard();
    }

    public void SetupCard()
    {
        if (cardData != null)
        {
            cardImage.sprite = cardData.backSprite;
            cardImage.color = cardData.cardColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isClickable && !isFlipping && !cardData.isMatched)
        {
            OnCardClicked?.Invoke(this);
        }
    }

    public void FlipCard()
    {
        if (!isFlipping)
        {
            StartCoroutine(FlipCardCoroutine());
        }
    }

    private IEnumerator FlipCardCoroutine()
    {
        isFlipping = true;
        isClickable = false;

        // Flip to front
        yield return StartCoroutine(FlipToSprite(cardData.frontSprite));

        cardData.isFlipped = true;

        // Re-enable clicking after flip
        isClickable = true;
        isFlipping = false;
    }

    public void FlipToBack()
    {
        if (!cardData.isMatched)
        {
            StartCoroutine(FlipToBackCoroutine());
        }
    }

    private IEnumerator FlipToBackCoroutine()
    {
        isClickable = false;
        yield return StartCoroutine(FlipToSprite(cardData.backSprite));
        cardData.isFlipped = false;
        isClickable = true;
    }

    private IEnumerator FlipToSprite(Sprite targetSprite)
    {
        float elapsedTime = 0;
        Vector3 originalScale = transform.localScale;

        // Scale down (flip effect)
        while (elapsedTime < flipDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float progress = flipCurve.Evaluate(elapsedTime / (flipDuration / 2));
            transform.localScale = new Vector3(originalScale.x * (1 - progress), originalScale.y, originalScale.z);
            yield return null;
        }

        // Change sprite at halfway point
        cardImage.sprite = targetSprite;

        // Scale back up
        elapsedTime = 0;
        while (elapsedTime < flipDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float progress = flipCurve.Evaluate(elapsedTime / (flipDuration / 2));
            transform.localScale = new Vector3(originalScale.x * progress, originalScale.y, originalScale.z);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    public void SetMatched()
    {
        cardData.isMatched = true;
        isClickable = false;
        // Add visual feedback for matched state
        cardImage.color = Color.gray;
    }
}