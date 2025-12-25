using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public Color fadeColor = Color.black;

    [Header("Default Door Sounds")]
    public AudioClip defaultOpenSound;
    public AudioClip defaultCloseSound;
    [Range(0f, 1f)]
    public float defaultSoundVolume = 0.7f;

    // HARDCODED - Last main scene we were in
    private string lastMainScene = "MainGame";

    private Canvas fadeCanvas;
    private Image fadeImage;
    private AudioSource audioSource;
    private bool isTransitioning = false;

    // Script version - if this doesn't match, old instance gets destroyed
    private const int SCRIPT_VERSION = 2;
    private int instanceVersion = SCRIPT_VERSION;

    void Awake()
    {
        // Destroy old versions
        if (Instance != null)
        {
            if (Instance.instanceVersion < SCRIPT_VERSION)
            {
                Debug.Log("[SceneTransition] Destroying old version");
                Destroy(Instance.gameObject);
                Instance = null;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateFadeCanvas();
        CreateAudioSource();

        // If starting in a main scene, save it
        string current = SceneManager.GetActiveScene().name;
        if (IsMainScene(current))
        {
            lastMainScene = current;
        }

        Debug.Log("[SceneTransition] v" + SCRIPT_VERSION + " Ready. LastMain=" + lastMainScene);
    }

    /// <summary>
    /// HARDCODED check - is this a main scene?
    /// </summary>
    public bool IsMainScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;

        string lower = sceneName.ToLower();

        // HARDCODED MAIN SCENES - add more here if needed
        if (lower == "maingame") return true;
        if (lower == "crimeea") return true;
        if (lower == "constanta") return true;

        return false;
    }

    /// <summary>
    /// Get the last main scene (never returns interior scenes)
    /// </summary>
    public string GetLastMainScene()
    {
        if (string.IsNullOrEmpty(lastMainScene))
            return "MainGame"; // HARDCODED fallback
        return lastMainScene;
    }

    void CreateFadeCanvas()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999;

        canvasObj.AddComponent<CanvasScaler>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        fadeImage.raycastTarget = false;

        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    void CreateAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    /// <summary>
    /// Go to a scene
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;

        // SAVE current scene ONLY if it's a main scene
        string current = SceneManager.GetActiveScene().name;
        if (IsMainScene(current))
        {
            lastMainScene = current;
            Debug.Log("[SceneTransition] SAVED main scene: " + lastMainScene);
        }

        Debug.Log("[SceneTransition] " + current + " --> " + sceneName + " (LastMain=" + lastMainScene + ")");

        StartCoroutine(DoTransition(sceneName, defaultOpenSound, defaultCloseSound, defaultSoundVolume));
    }

    /// <summary>
    /// Go to a scene with custom sounds
    /// </summary>
    public void TransitionToSceneWithSounds(string sceneName, AudioClip enterSound, AudioClip exitSound, float volume)
    {
        if (isTransitioning) return;

        // SAVE current scene ONLY if it's a main scene
        string current = SceneManager.GetActiveScene().name;
        if (IsMainScene(current))
        {
            lastMainScene = current;
            Debug.Log("[SceneTransition] SAVED main scene: " + lastMainScene);
        }

        Debug.Log("[SceneTransition] " + current + " --> " + sceneName + " (LastMain=" + lastMainScene + ")");

        StartCoroutine(DoTransition(sceneName, enterSound, exitSound, volume));
    }

    // Keep for compatibility
    public void TransitionToSceneWithSounds(string sceneName, AudioClip enterSound, AudioClip exitSound, float volume, bool ignored)
    {
        TransitionToSceneWithSounds(sceneName, enterSound, exitSound, volume);
    }

    // Old property for compatibility
    public string PreviousScene { get { return GetLastMainScene(); } }

    IEnumerator DoTransition(string sceneName, AudioClip enterSound, AudioClip exitSound, float volume)
    {
        isTransitioning = true;

        if (enterSound != null)
            audioSource.PlayOneShot(enterSound, volume);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(Fade(0f, 1f));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        yield return new WaitForSeconds(0.1f);

        if (exitSound != null)
            audioSource.PlayOneShot(exitSound, volume);

        yield return StartCoroutine(Fade(1f, 0f));

        isTransitioning = false;
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, endAlpha);
    }

    public void FadeToBlack(System.Action onComplete = null)
    {
        StartCoroutine(FadeToBlackCoroutine(onComplete));
    }

    IEnumerator FadeToBlackCoroutine(System.Action onComplete)
    {
        yield return StartCoroutine(Fade(0f, 1f));
        if (onComplete != null) onComplete();
    }

    public void FadeFromBlack(System.Action onComplete = null)
    {
        StartCoroutine(FadeFromBlackCoroutine(onComplete));
    }

    IEnumerator FadeFromBlackCoroutine(System.Action onComplete)
    {
        yield return StartCoroutine(Fade(1f, 0f));
        if (onComplete != null) onComplete();
    }

    public bool IsTransitioning() { return isTransitioning; }

    public void SetFadeDuration(float duration)
    {
        fadeDuration = duration;
    }

    public void StartBlack()
    {
        if (fadeImage != null)
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
    }

    public void FadeOutAndBack(System.Action onComplete = null)
    {
        if (isTransitioning) return;
        StartCoroutine(DoFadeOutAndBack(onComplete));
    }

    IEnumerator DoFadeOutAndBack(System.Action onComplete)
    {
        isTransitioning = true;
        yield return StartCoroutine(Fade(0f, 1f));
        yield return new WaitForSeconds(0.5f);
        if (onComplete != null) onComplete();
        yield return StartCoroutine(Fade(1f, 0f));
        isTransitioning = false;
    }
}