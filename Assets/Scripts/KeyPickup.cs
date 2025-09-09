using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.HasKey = true;
                Debug.Log("Player picked up the key!");
                Destroy(gameObject);

                AudioManager.Instance.PlayStarPickup();
            }
        }
    }
}
