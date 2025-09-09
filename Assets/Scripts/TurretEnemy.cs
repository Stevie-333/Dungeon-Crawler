using UnityEngine;

public class TurretEnemy : BaseEnemy
{
    [Header("Detection & Shooting")]
    public float detectionRange = 8f;
    public float fireRate = 1.5f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 5f;
    public int projectileDamage = 1;

    private float fireCooldown = 0f;
    private bool isAttacking = false;

    private SpriteRenderer sr;

    protected override void Start()
    {
        base.Start();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            sr.enabled = true; // Ensure it's never toggled off
    }




    public override void Move()
    {
        // Stationary turret does not move
    }

    protected void Update()
    {

        if (target == null || isDying) return;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= detectionRange)
        {
            fireCooldown -= Time.deltaTime;

            // Only trigger attack animation if cooldown finished and not already attacking
            if (fireCooldown <= 0f && !isAttacking)
            {
                if (animator != null && animator.HasParameter("Attack"))
                    animator.SetTrigger("Attack");

                isAttacking = true;
                fireCooldown = fireRate; // reset cooldown
            }
        }
    }


    // This function is called from an Animation Event at the moment the turret should shoot
    public void SpawnProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        Vector2 direction = (target.position - firePoint.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = direction * projectileSpeed;

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null) projectile.damage = projectileDamage;
    }

    // This function is called from an Animation Event at the END of the attack animation
    public void OnAttackAnimationComplete()
    {
        isAttacking = false;
    }
}
