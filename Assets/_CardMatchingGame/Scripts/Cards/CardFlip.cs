using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardFlip : MonoBehaviour
{
    [SerializeField] private Image cardImage;     // Card's Image component
    [SerializeField] private Sprite backSprite;   // Sprite for the back of the card
    [SerializeField] private Sprite frontSprite;  // Sprite for the front (face) of the card

    private bool isFlipped = false;

    // Routine to flip between card back and face
    public IEnumerator FlipCard()
    {
        float duration = 0.3f; // Half-flip duration
        float elapsed = 0f;

        // Shrink X to zero ("close" the card)
        while (elapsed < duration)
        {
            float scaleX = Mathf.Lerp(1f, 0f, elapsed / duration);
            cardImage.transform.localScale = new Vector3(scaleX, 1f, 1f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Swap sprite: show face or back
        cardImage.sprite = isFlipped ? backSprite : frontSprite;
        isFlipped = !isFlipped;

        elapsed = 0f;

        // Expand X size back to normal ("open" the card)
        while (elapsed < duration)
        {
            float scaleX = Mathf.Lerp(0f, 1f, elapsed / duration);
            cardImage.transform.localScale = new Vector3(scaleX, 1f, 1f);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // Routine to flash the card face with flip animation
    public IEnumerator FlashCardFace(float flashTime)
    {
        if (isFlipped) // Do nothing if already showing front
            yield break;

        // Flip to show the face
        yield return StartCoroutine(FlipCard());

        // Wait for the desired flash time
        yield return new WaitForSeconds(flashTime);

        // Flip back to hidden
        yield return StartCoroutine(FlipCard());
    }
}
