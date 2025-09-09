using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Vector2 movement;

    [Header("Attack")]
    public float attackRange = 1f;
    public int attackDamage = 1;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayer;
    public Transform attackPoint;

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;
    public Image healthImage;         
    public Sprite[] healthSprites;    

    [Header("Components")]
    public Animator animator;
    private SpriteRenderer spriteRenderer;

    private float lastAttackTime;
    private bool isDead = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        if (isDead) return;

        // Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Flip sprite
        if (movement.x > 0)
            spriteRenderer.flipX = false;
        else if (movement.x < 0)
            spriteRenderer.flipX = true;

        // Attack input
        if (Input.GetButtonDown("Fire1") && Time.time >= lastAttackTime + attackCooldown)
        {
            animator.SetTrigger("Attack");
            lastAttackTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Simple movement (no tilemap restriction)
        transform.position += (Vector3)movement.normalized * moveSpeed * Time.fixedDeltaTime;
    }

    // Called by Animation Event at the correct frame
    public void PerformAttack()
    {
        if (isDead) return;

        // Play attack sound
        AudioManager.Instance?.PlayPlayerAttack();

        // Spawn attack VFX
        if (attackPoint != null && AttackEffectPool.Instance != null)
        {
            GameObject fx = AttackEffectPool.Instance.Get();
            fx.transform.position = attackPoint.position;
            fx.transform.rotation = Quaternion.identity;

            Vector3 s = fx.transform.localScale;
            s.x = spriteRenderer.flipX ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
            fx.transform.localScale = s;
        }

        // Damage enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            BaseEnemy be = enemy.GetComponent<BaseEnemy>();
            if (be != null)
                be.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        animator.SetTrigger("TakeDamageTrigger");
        AudioManager.Instance?.PlayPlayerDamage();
        UpdateHealthUI();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthImage != null && healthSprites.Length > 0)
        {
            int index = Mathf.Clamp(currentHealth - 1, 0, healthSprites.Length - 1);
            healthImage.sprite = healthSprites[index];
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("DieTrigger");
        movement = Vector2.zero;

        LoseScreen loseScreen = FindObjectOfType<LoseScreen>();
        if (loseScreen != null)
            loseScreen.ShowLoseScreen();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
