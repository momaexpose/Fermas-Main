using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueSystem : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public Color speakerColor = Color.white;
        [TextArea(2, 5)]
        public string text;
        public float autoAdvanceTime = 0f; // 0 = wait for input
    }

    [Header("Dialogue Content")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();

    [Header("UI Settings")]
    public Color defaultSpeakerColor = Color.yellow;
    public Color dialogueBoxColor = new Color(0, 0, 0, 0.85f);
    public int fontSize = 24;
    public int speakerFontSize = 28;

    [Header("Typewriter Effect")]
    public bool useTypewriter = true;
    public float typewriterSpeed = 0.03f;

    [Header("Audio")]
    public AudioClip typingSound;
    public AudioClip advanceSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;

    private GameObject dialoguePanel;
    private Text speakerText;
    private Text dialogueText;
    private Text continueText;
    private AudioSource audioSource;

    private int currentLine = 0;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private Coroutine typewriterCoroutine;

    void Start()
    {
        Debug.Log("[DialogueSystem] Initializing...");
        CreateUI();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        // Hide at start
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            Debug.Log("[DialogueSystem] Panel hidden at start");
        }

        Debug.Log("[DialogueSystem] Ready with " + dialogueLines.Count + " lines");
    }

    void CreateUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DialogueCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Main panel at bottom of screen
        dialoguePanel = new GameObject("DialoguePanel");
        dialoguePanel.transform.SetParent(canvas.transform, false);

        Image panelBg = dialoguePanel.AddComponent<Image>();
        panelBg.color = dialogueBoxColor;

        RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 20);
        panelRect.sizeDelta = new Vector2(-40, 200);

        // Speaker name
        GameObject speakerObj = new GameObject("SpeakerName");
        speakerObj.transform.SetParent(dialoguePanel.transform, false);
        speakerText = speakerObj.AddComponent<Text>();
        speakerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (speakerText.font == null) speakerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        speakerText.fontSize = speakerFontSize;
        speakerText.fontStyle = FontStyle.Bold;
        speakerText.color = defaultSpeakerColor;
        speakerText.alignment = TextAnchor.UpperLeft;

        RectTransform speakerRect = speakerObj.GetComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0, 1);
        speakerRect.anchorMax = new Vector2(1, 1);
        speakerRect.pivot = new Vector2(0, 1);
        speakerRect.anchoredPosition = new Vector2(30, -15);
        speakerRect.sizeDelta = new Vector2(-60, 40);

        // Dialogue text
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(dialoguePanel.transform, false);
        dialogueText = textObj.AddComponent<Text>();
        dialogueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (dialogueText.font == null) dialogueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dialogueText.fontSize = fontSize;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAnchor.UpperLeft;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0, -15);
        textRect.offsetMin = new Vector2(30, 40);
        textRect.offsetMax = new Vector2(-30, -55);

        // Continue prompt
        GameObject continueObj = new GameObject("ContinueText");
        continueObj.transform.SetParent(dialoguePanel.transform, false);
        continueText = continueObj.AddComponent<Text>();
        continueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (continueText.font == null) continueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        continueText.fontSize = 16;
        continueText.color = new Color(0.7f, 0.7f, 0.7f);
        continueText.alignment = TextAnchor.LowerRight;
        continueText.text = "Press ENTER or SPACE to continue...";

        RectTransform continueRect = continueObj.GetComponent<RectTransform>();
        continueRect.anchorMin = new Vector2(1, 0);
        continueRect.anchorMax = new Vector2(1, 0);
        continueRect.pivot = new Vector2(1, 0);
        continueRect.anchoredPosition = new Vector2(-20, 10);
        continueRect.sizeDelta = new Vector2(400, 25);
    }

    void Update()
    {
        if (!isDialogueActive) return;

        // Advance dialogue with Enter or Space
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (isTyping)
            {
                // Skip typewriter effect
                CompleteCurrentLine();
            }
            else
            {
                // Advance to next line
                AdvanceDialogue();
            }
        }
    }

    public void StartDialogue()
    {
        Debug.Log("[DialogueSystem] StartDialogue called with " + dialogueLines.Count + " lines");

        if (dialogueLines.Count == 0)
        {
            Debug.LogWarning("[DialogueSystem] No dialogue lines to show!");
            return;
        }

        isDialogueActive = true;
        currentLine = 0;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            Debug.Log("[DialogueSystem] Panel activated");
        }

        ShowCurrentLine();

        // Hide cursor during dialogue
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ShowCurrentLine()
    {
        if (currentLine >= dialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueLines[currentLine];

        // Set speaker name
        if (speakerText != null)
        {
            speakerText.text = line.speakerName;
            speakerText.color = line.speakerColor != Color.clear ? line.speakerColor : defaultSpeakerColor;
        }

        // Show text with typewriter effect or instantly
        if (useTypewriter)
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);

            typewriterCoroutine = StartCoroutine(TypewriterEffect(line.text));
        }
        else
        {
            if (dialogueText != null)
                dialogueText.text = line.text;
        }

        // Hide continue prompt while typing
        if (continueText != null)
            continueText.gameObject.SetActive(!useTypewriter);

        // Auto advance if set
        if (line.autoAdvanceTime > 0)
        {
            StartCoroutine(AutoAdvance(line.autoAdvanceTime));
        }
    }

    IEnumerator TypewriterEffect(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;

            if (typingSound != null && c != ' ')
                audioSource.PlayOneShot(typingSound, soundVolume * 0.3f);

            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;

        if (continueText != null)
            continueText.gameObject.SetActive(true);
    }

    void CompleteCurrentLine()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        isTyping = false;

        if (currentLine < dialogueLines.Count && dialogueText != null)
            dialogueText.text = dialogueLines[currentLine].text;

        if (continueText != null)
            continueText.gameObject.SetActive(true);
    }

    void AdvanceDialogue()
    {
        if (advanceSound != null)
            audioSource.PlayOneShot(advanceSound, soundVolume);

        currentLine++;
        ShowCurrentLine();
    }

    IEnumerator AutoAdvance(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (isDialogueActive && !isTyping)
            AdvanceDialogue();
    }

    void EndDialogue()
    {
        isDialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Debug.Log("[DialogueSystem] Dialogue complete!");

        // Try OrasIntroManager first
        OrasIntroManager orasIntro = FindFirstObjectByType<OrasIntroManager>();
        if (orasIntro != null)
        {
            orasIntro.OnDialogueEnd();
            return;
        }

        // Try CrimeeaIntroManager
        CrimeeaIntroManager crimeeaIntro = FindFirstObjectByType<CrimeeaIntroManager>();
        if (crimeeaIntro != null)
        {
            crimeeaIntro.OnDialogueEnd();
            return;
        }

        // Fall back to IntroManager
        if (IntroManager.Instance != null)
        {
            IntroManager.Instance.EndIntro();
            return;
        }

        Debug.Log("[DialogueSystem] No intro manager found to handle dialogue end.");
    }

    /// <summary>
    /// Add a line of dialogue at runtime
    /// </summary>
    public void AddLine(string speaker, string text, Color speakerColor)
    {
        dialogueLines.Add(new DialogueLine
        {
            speakerName = speaker,
            text = text,
            speakerColor = speakerColor
        });
    }

    /// <summary>
    /// Clear all dialogue lines
    /// </summary>
    public void ClearDialogue()
    {
        dialogueLines.Clear();
    }

    public bool IsDialogueActive() { return isDialogueActive; }
}