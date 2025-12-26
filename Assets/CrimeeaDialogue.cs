using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Place this script in your Crimeea scene.
/// Dialogue with Bossu Tiganilor.
/// </summary>
public class CrimeeaIntroManager : MonoBehaviour
{
    [Header("Speaker Colors")]
    public Color bossColor = new Color(0.8f, 0.5f, 1f); // Purple
    public Color playerColor = new Color(0.4f, 0.8f, 1f); // Cyan

    [Header("Timing")]
    public float delayBeforeFadeIn = 0.5f;
    public float fadeInDuration = 1.5f;
    public float delayBeforeDialogue = 1f;

    [Header("After Dialogue")]
    public bool goToNextScene = false;
    public string nextSceneName = "MainGame";

    private DialogueSystem dialogueSystem;

    void Start()
    {
        // Create SceneTransitionManager if needed
        if (SceneTransitionManager.Instance == null)
        {
            GameObject go = new GameObject("SceneTransitionManager");
            go.AddComponent<SceneTransitionManager>();
        }

        // Start black
        SceneTransitionManager.Instance.StartBlack();

        // Create DialogueSystem
        dialogueSystem = FindFirstObjectByType<DialogueSystem>();
        if (dialogueSystem == null)
        {
            GameObject dialogueObj = new GameObject("DialogueSystem");
            dialogueSystem = dialogueObj.AddComponent<DialogueSystem>();
        }

        dialogueSystem.ClearDialogue();
        SetupDialogue();

        StartCoroutine(RunIntro());
    }

    IEnumerator RunIntro()
    {
        yield return new WaitForSeconds(delayBeforeFadeIn);

        SceneTransitionManager.Instance.SetFadeDuration(fadeInDuration);
        SceneTransitionManager.Instance.FadeFromBlack();

        yield return new WaitForSeconds(fadeInDuration + delayBeforeDialogue);

        SceneTransitionManager.Instance.SetFadeDuration(0.5f);

        if (dialogueSystem != null)
            dialogueSystem.StartDialogue();
    }

    /// <summary>
    /// Called by DialogueSystem when dialogue ends
    /// </summary>
    public void OnDialogueEnd()
    {
        Debug.Log("[CrimeeaIntro] Dialogue ended");

        if (goToNextScene)
        {
            SceneTransitionManager.Instance.TransitionToScene(nextSceneName);
        }
        // If goToNextScene is false, player just stays in scene and can move around
    }

    void SetupDialogue()
    {
        string boss = "The King of Gypsies";
        string tu = "You";

        // =====================================================
        // ADD YOUR DIALOGUE LINES HERE
        // =====================================================

        AddLine(boss, "Ah, are you the one sent by my brother?", bossColor);
        AddLine(tu, "Yes, he sent me. He told me you have a ship for me.", playerColor);
        AddLine(boss, "A ship? Haha! It's more like a boat... but it works.", bossColor);
        AddLine(boss, "Take good care of it, I don't have another one.", bossColor);
        AddLine(tu, "Thank you. What exactly do I have to do?", playerColor);
        AddLine(boss, "You grow drugs on the boat. Pirates will come to buy.", bossColor);
        AddLine(boss, "If you have nothing to sell them... they won't be happy.", bossColor);
        AddLine(boss, "Take care of yourself, scoundrel. The sea is dangerous.", bossColor);
        AddLine(tu, "I understand.", playerColor);
        AddLine(boss, "Now go. The boat is waiting for you. First, go to your home, where you will press the telephone to talk to me. If i pick up, the ship is ready to be used. You can select your destination from the big map on the right wall.", bossColor);
        AddLine(boss, "Inside, you will find two chests: one for seeds, and one for the products. You get seeds by sending the golden bird, Grasu', to bring you seeds. You can send it by pressing e on it. When you have seeds, and you want to plant them, press e on a pot, and wait until the plant finishes growing. After you have your product, you can sell to pirates that come every few minutes.", bossColor);
        AddLine(boss, "To leave, go to the map and choose where to go, then pull the lever and the ship will start.", bossColor);
        AddLine(boss, "After you get the money you owe(3000 $) you can pay it back from one of the screens inside your boat.", bossColor);


        // =====================================================
        // END OF DIALOGUE
        // =====================================================
    }

    void AddLine(string speaker, string text, Color color)
    {
        if (dialogueSystem != null)
            dialogueSystem.AddLine(speaker, text, color);
    }
}