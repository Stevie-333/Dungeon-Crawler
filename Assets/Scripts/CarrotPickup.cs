using UnityEngine;

public class CarrotPickup : MonoBehaviour
{
    public int healAmount = 1;  // Amount of health restored

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Heal(healAmount);  
        }

            AudioManager.Instance.PlayCarrotPickup();

            Destroy(gameObject); // Remove the carrot
        }
    }
}
