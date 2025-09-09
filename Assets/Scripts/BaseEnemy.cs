using System;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 2f;
    public int damage = 1;
    public int health = 3;

    [Header("Drops")]
    public GameObject carrotPrefab;
    [Range(0f, 1f)] public float carrotDropChance = 0.3f;

    protected Transform target;
    protected Animator animator;
    protected Rigidbody2D rb;

    protected bool isDying = false;

    public event Action<BaseEnemy> OnEnemyDied;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic; // ensure dynamic
        rb.freezeRotation = true; // prevent rotation

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;

        animator = GetComponent<Animator>();
    }

private void OnTriggerEnter2D(Collider2D other)
{
    if (isDying) return;

    if (other.CompareTag("Player"))
    {
        // Deal Damage to player
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);  
        }

        // Play attack animation
        if (animator != null && animator.HasParameter("Attack"))
        {
            animator.SetTrigger("Attack");
        }
    }
}

public void TakeDamage(int damageAmount)
    {
        if (isDying) return;

        health -= damageAmount;
        if (health <= 0)
            Die();
    }

protected virtual void Die()
{
    if (isDying) return;
    isDying = true;

    // Stop player movement caused by physics when enemy dies
    Rigidbody2D playerRb = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Rigidbody2D>();
    if (playerRb != null)
    {
        playerRb.velocity = Vector2.zero;
        playerRb.angularVelocity = 0f;
    }

    if (animator != null && animator.HasParameter("Death"))
    {
        animator.SetTrigger("Death");
    }
    else
    {
        DropCarrot();
        NotifyDeath();
        Destroy(gameObject);
    }

    AudioManager.Instance.PlayEnemyDeath();
}



public void OnDeathAnimationComplete()
{
    // Update kill count
    GameManager.Instance.AddKill();

    // Drop loot
    DropCarrot();

    // Notify any listeners (RoomController, etc.)
    NotifyDeath();

    // Destroy enemy object
    Destroy(gameObject);
}

    private void DropCarrot()
    {
        if (carrotPrefab != null && UnityEngine.Random.value < carrotDropChance)
            Instantiate(carrotPrefab, transform.position, Quaternion.identity);
    }

    private void NotifyDeath()
    {
        OnEnemyDied?.Invoke(this);
    }

    private void FixedUpdate()
    {
        if (!isDying)
            Move(); // physics-safe movement
    }

    public abstract void Move();
}

// Helper extension
public static class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, string paramName)
    {
        foreach (var param in animator.parameters)
            if (param.name == paramName) return true;
        return false;
    }
}
