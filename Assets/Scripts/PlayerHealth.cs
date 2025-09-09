using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    [Header("UI")]
    public Image healthImage;      // Drag the UI Image here
    public Sprite[] healthSprites; // Array of 4 sprites representing health levels

   [Header("Animation")]
    public Animator animator;  // Drag the Animator component here

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }


    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Play damage animation
        if (animator != null)
        {
            animator.SetTrigger("TakeDamageTrigger");
        }

        UpdateHealthUI();

        AudioManager.Instance.PlayPlayerDamage();


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
{
    currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    UpdateHealthUI(); // Update your health bar or sprites
}


   void UpdateHealthUI()
    {
        if (healthImage != null && healthSprites.Length > 0)
        {
            int index = Mathf.Clamp(currentHealth - 1, 0, healthSprites.Length - 1);
            healthImage.sprite = healthSprites[index];
        }
    }

    void Die()
    {
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("DieTrigger");
        }

        // Disable player controls here if needed
        Debug.Log("Player died!");

        LoseScreen loseScreen = FindObjectOfType<LoseScreen>();
        if (loseScreen != null)
        {
            loseScreen.ShowLoseScreen();
        }

    }
}
