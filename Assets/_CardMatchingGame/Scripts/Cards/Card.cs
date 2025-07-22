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
            cardData.isFlipped = false;
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

        // Shrink X to zero over flipDuration/2
        while (elapsedTime < flipDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = flipCurve.Evaluate(elapsedTime / (flipDuration / 2f));
            transform.localScale = new Vector3(originalScale.x * (1 - progress), originalScale.y, originalScale.z);
            yield return null;
        }

        // Change sprite halfway
        cardImage.sprite = targetSprite;

        // Expand back out
        elapsedTime = 0;
        while (elapsedTime < flipDuration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = flipCurve.Evaluate(elapsedTime / (flipDuration / 2f));
            transform.localScale = new Vector3(originalScale.x * progress, originalScale.y, originalScale.z);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    /// <summary>
    /// Call this from GameManager to flash face at start.
    /// </summary>
    public IEnumerator FlashCardFace(float delay)
    {
        isClickable = false;

        // Flip to front
        yield return StartCoroutine(FlipToSprite(cardData.frontSprite));
        cardData.isFlipped = true;

        // Wait (card stays revealed for a moment)
        yield return new WaitForSeconds(delay);

        // Flip back
        yield return StartCoroutine(FlipToSprite(cardData.backSprite));
        cardData.isFlipped = false;

        isClickable = true;
    }

    public void SetMatched()
    {
        cardData.isMatched = true;
        isClickable = false;
        cardImage.color = Color.gray;
    }
}
