using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject instructionsPanel;

    public void StartGame()
    {
        SceneManager.LoadScene("Game"); // replace with your actual scene
    }

    public void ShowInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    public void HideInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
