using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Place this script in your OrasIntro scene.
/// It sets up the dialogue for the gangster intro.
/// Edit the SetupDialogue() method to add your lines.
/// </summary>
public class OrasIntroManager : MonoBehaviour
{
    [Header("Speaker Names")]
    public string gangster1Name = "Soldat";
    public string gangster2Name = "Don' Cristi";
    public string playerName = "Tu";

    [Header("Speaker Colors")]
    public Color gangster1Color = new Color(1f, 0.4f, 0.4f); // Red
    public Color gangster2Color = new Color(1f, 0.8f, 0.2f); // Gold/Yellow
    public Color playerColor = new Color(0.4f, 0.8f, 1f); // Cyan/Blue

    [Header("Timing")]
    public float delayBeforeFadeIn = 0.5f;
    public float fadeInDuration = 1.5f;
    public float delayBeforeDialogue = 1f;

    [Header("Next Scene")]
    public string nextSceneName = "Crimeea";

    private DialogueSystem dialogueSystem;

    void Start()
    {
        Debug.Log("[OrasIntro] Starting intro sequence...");

        // Ensure SceneTransitionManager exists
        if (SceneTransitionManager.Instance == null)
        {
            GameObject transitionObj = new GameObject("SceneTransitionManager");
            transitionObj.AddComponent<SceneTransitionManager>();
            Debug.Log("[OrasIntro] Created SceneTransitionManager");
        }

        // Start with screen black
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.StartBlack();
            Debug.Log("[OrasIntro] Screen set to black");
        }

        // Find or create DialogueSystem
        dialogueSystem = FindFirstObjectByType<DialogueSystem>();
        if (dialogueSystem == null)
        {
            GameObject dialogueObj = new GameObject("DialogueSystem");
            dialogueSystem = dialogueObj.AddComponent<DialogueSystem>();
            Debug.Log("[OrasIntro] Created DialogueSystem");
        }

        // Clear default dialogue and add our own
        dialogueSystem.ClearDialogue();
        SetupDialogue();

        // Start the intro sequence
        StartCoroutine(RunIntroSequence());
    }

    IEnumerator RunIntroSequence()
    {
        Debug.Log("[OrasIntro] Waiting before fade in...");
        yield return new WaitForSeconds(delayBeforeFadeIn);

        // Fade from black slowly
        Debug.Log("[OrasIntro] Fading from black...");
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.SetFadeDuration(fadeInDuration);
            SceneTransitionManager.Instance.FadeFromBlack();
            yield return new WaitForSeconds(fadeInDuration);
            SceneTransitionManager.Instance.SetFadeDuration(0.5f); // Reset to default
        }

        Debug.Log("[OrasIntro] Waiting before dialogue...");
        yield return new WaitForSeconds(delayBeforeDialogue);

        // Start dialogue
        Debug.Log("[OrasIntro] Starting dialogue...");
        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue();
        }
    }

    /// <summary>
    /// Called by DialogueSystem when dialogue ends
    /// </summary>
    public void OnDialogueEnd()
    {
        Debug.Log("[OrasIntro] Dialogue ended, transitioning to " + nextSceneName);

        if (SceneTransitionManager.Instance != null)
        {
            // Fade to black -> load Crimeea -> fade from black
            SceneTransitionManager.Instance.TransitionToScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void SetupDialogue()
    {
        // Hardcoded names to avoid Inspector override issues
        string soldat = "Soldier";
        string don = "Don' Cristi";
        string tu = "You";

        Color soldatColor = gangster1Color;
        Color donColor = gangster2Color;
        Color tuColor = playerColor;

        AddLine(soldat, "Look boss, I brought the prowler.", soldatColor);
        AddLine(don, "Good... now let's see what we'll do with him.", donColor);
        AddLine(don, "Bean-counter, hand over the money, or you won't see tomorrow.", donColor);
        AddLine(tu, "But I don't have any money, a gypsy named Vlad stole it all from me.", tuColor);
        AddLine(don, "In that case, I can send you on an expedition where you can earn my money back.", donColor);
        AddLine(don, "You go to my brother, the Boss of the Gypsies, and he'll give you a ship you can take out to sea.", donColor);
        AddLine(tu, "Okay, and what do I have to do after that?", tuColor);
        AddLine(don, "You sail the sea, grow drugs on the ship, then sell them to pirate thugs who come to you.", donColor);
        AddLine(don, "It's a dangerous expedition, because the pirates won't be happy if they don't get their drugs. You'll need to get some weapons or train yourself.", donColor);
        AddLine(don, "The boat is decent, something from the Second World War...", donColor);
        AddLine(soldat, "Boss, we’re out of time, we have to leave—the police will be here any moment.", soldatColor);
        AddLine(don, "Fine, prepare the helicopter.", donColor);
        AddLine(don, "And as for you, I recommend you run now, then go straight to the Boss of the Gypsies.", donColor);

    }

    void AddLine(string speaker, string text, Color color)
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.AddLine(speaker, text, color);
        }
    }
}