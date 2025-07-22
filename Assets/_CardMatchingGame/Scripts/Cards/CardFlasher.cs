using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardFlasher : MonoBehaviour
{
    public Image cardImage; // Assign your card's Image component here
    public float flashDuration = 3f;     // Total duration of the flashing
    public float flashInterval = 0.5f;   // How fast the card toggles

    private void Start()
    {
        if (cardImage == null)
            cardImage = GetComponent<Image>();

        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        bool isVisible = true;

        while (elapsed < flashDuration)
        {
            cardImage.enabled = isVisible;
            isVisible = !isVisible;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // Ensure card remains visible after flashing
        cardImage.enabled = true;
    }
}
