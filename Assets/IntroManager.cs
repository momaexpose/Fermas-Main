using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    public static IntroManager Instance { get; private set; }

    [Header("Scene Names")]
    public string introSceneName = "OrasIntro";
    public string afterIntroSceneName = "Crimeea";

    [Header("Intro Settings")]
    public float delayBeforeFadeIn = 0.5f;
    public float fadeInDuration = 1.5f;
    public float delayBeforeDialogue = 1f;

    private bool introComplete = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == introSceneName && !introComplete)
        {
            StartCoroutine(StartIntroSequence());
        }
    }

    /// <summary>
    /// Call this from Main Menu "Start New Game" button
    /// </summary>
    public void StartNewGame()
    {
        introComplete = false;
        StartCoroutine(DoStartNewGame());
    }

    IEnumerator DoStartNewGame()
    {
        // Fade to black first
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.FadeToBlack();
            yield return new WaitForSeconds(0.6f);
        }

        // Load intro scene (screen stays black)
        SceneManager.LoadScene(introSceneName);
    }

    IEnumerator StartIntroSequence()
    {
        // Wait a moment
        yield return new WaitForSeconds(delayBeforeFadeIn);

        // Fade from black slowly
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.SetFadeDuration(fadeInDuration);
            SceneTransitionManager.Instance.FadeFromBlack();
            yield return new WaitForSeconds(fadeInDuration);
            SceneTransitionManager.Instance.SetFadeDuration(0.5f); // Reset to default
        }

        yield return new WaitForSeconds(delayBeforeDialogue);

        // Start dialogue
        DialogueSystem dialogue = FindFirstObjectByType<DialogueSystem>();
        if (dialogue != null)
        {
            dialogue.StartDialogue();
        }
        else
        {
            Debug.LogWarning("[IntroManager] No DialogueSystem found in scene!");
            EndIntro();
        }
    }

    /// <summary>
    /// Called when dialogue ends
    /// </summary>
    public void EndIntro()
    {
        introComplete = true;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(afterIntroSceneName);
        }
        else
        {
            SceneManager.LoadScene(afterIntroSceneName);
        }
    }
}