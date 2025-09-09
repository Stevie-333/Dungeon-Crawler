using UnityEngine;
using TMPro;

/// <summary>
/// Handles displaying the lose screen UI when the player dies.
/// </summary>
public class LoseScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;            // Panel that holds lose screen UI
    public TextMeshProUGUI titleText;   // Text field for "Game Over"

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false); // Ensure hidden at start
    }

    /// <summary>
    /// Shows the lose screen with optional text.
    /// </summary>
    public void ShowLoseScreen()
    {
        if (panel != null)
            panel.SetActive(true);

        if (titleText != null)
            titleText.text = "Game Over";
    }
}
