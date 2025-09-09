using UnityEngine;

public class Chest : MonoBehaviour
{
    private bool isOpened = false;
    private WinScreen winScreen;

    private void Start()
    {
        // Find WinScreen in the scene
        winScreen = FindObjectOfType<WinScreen>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOpened) return;

        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory != null && inventory.HasKey)
            {
                isOpened = true;
                Debug.Log("Chest opened! You win!");

                if (winScreen != null)
                {
                    winScreen.ShowWinScreen();
                }

                // Optionally keep chest visible instead of destroying it
                // Destroy(gameObject);
            }
            else
            {
                Debug.Log("Chest is locked. Find the star first!");
            }
        }
    }
}
