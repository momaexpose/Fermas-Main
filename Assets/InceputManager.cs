using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InceputManager : MonoBehaviour
{
    public Image fadeImage;   // Black full screen image
    public Image logoImage;   // Logo on top
    public GameObject mainMenu;

    public float fadeDuration = 1.5f;
    public float logoHoldTime = 1.5f;

    void Start()
    {
        // Force correct starting state
        fadeImage.color = new Color(0, 0, 0, 1); // fully black
        logoImage.color = new Color(1, 1, 1, 1); // logo visible
        mainMenu.SetActive(false);

        StartCoroutine(Intro());
    }

    IEnumerator Intro()
    {
        // Hold logo on black screen
        yield return new WaitForSeconds(logoHoldTime);

        // Fade ONLY the logo
        yield return StartCoroutine(Fade(logoImage, 1f, 0f));

        // Enable menu while still black
        mainMenu.SetActive(true);

        // FIRST TIME black screen fades out
        yield return StartCoroutine(Fade(fadeImage, 1f, 0f));
    }

    IEnumerator Fade(Image img, float from, float to)
    {
        float t = 0f;
        Color c = img.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            img.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        img.color = new Color(c.r, c.g, c.b, to);
    }
}
