using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles displaying the win screen UI, score, and post-victory options.
/// </summary>
public class WinScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winPanel;               // The panel to show when the player wins
    public TextMeshProUGUI scoreText;         // UI element to display the score

    private void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false); // Ensure hidden at start
    }

    /// <summary>
    /// Shows the win screen, pauses gameplay, and updates the score text.
    /// </summary>
    public void ShowWinScreen()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        // Pause game time
        Time.timeScale = 0f;

        // Display final score
        if (scoreText != null)
        {
            int score = GameManager.Instance.CalculateScore();
            scoreText.text = "Score: " + score;
        }
    }

    /// <summary>
    /// Restart the game by reloading the current scene.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume time

        // Hide lose screen if it exists (prevents overlap if both were active)
        LoseScreen loseScreen = FindObjectOfType<LoseScreen>();
        if (loseScreen != null && loseScreen.panel != null)
            loseScreen.panel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Quit the game.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit Game called");
        Application.Quit();
    }
}
