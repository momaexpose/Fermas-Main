using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject newGamePanel;
    public GameObject continuePanel;
    public GameObject optionsPanel;

    void Start()
    {
        OpenMain();
    }

    public void OpenMain()
    {
        mainPanel.SetActive(true);
        newGamePanel.SetActive(false);
        continuePanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void OpenNewGame()
    {
        mainPanel.SetActive(false);
        newGamePanel.SetActive(true);
    }

    public void OpenContinue()
    {
        mainPanel.SetActive(false);
        continuePanel.SetActive(true);
    }

    public void OpenOptions()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
