using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public float fadeDuration = 2.0f; // Duration in seconds

    private Material material;
    private bool isFading = false;

    void OnEnable()
    {
        // Attempt to get the material from the Renderer component
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
        }
        FadeOut();
    }

    public void FadeIn()
    {
        // Start fading in only if not already fading
        if (!isFading && material != null)
        {
            StartCoroutine(FadeMaterial(0f, 1f, 2)); // From alpha = 0 to alpha = 1
        }
    }

    public void FadeOut()
    {
        // Start fading out only if not already fading
        if (!isFading && material != null)
        {
            StartCoroutine(FadeMaterial(1f, 0f, 3)); // From alpha = 1 to alpha = 0
        }
    }

    private IEnumerator FadeMaterial(float startAlpha, float endAlpha, float fadingTime)
    {
        isFading = true;
        float elapsedTime = 0f;

        Color currentColor = material.color;
        Color startColor = new Color(currentColor.r, currentColor.g, currentColor.b, startAlpha);
        Color endColor = new Color(currentColor.r, currentColor.g, currentColor.b, endAlpha);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            material.color = Color.Lerp(startColor, endColor, elapsedTime / fadingTime);
            yield return null;
        }

        // Ensure final alpha is set correctly after loop
        material.color = endColor;
        isFading = false;
    }
}
