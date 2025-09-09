using UnityEngine;

public class ChaserEnemy : BaseEnemy
{
    public float detectionRange = 5f;
    public float wallCheckDistance = 0.5f; // how far ahead to check for walls
    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

public override void Move()
{
    if (isDying || target == null) return;

    Vector2 targetPos = target.position;
    float distance = Vector2.Distance(rb.position, targetPos);

    if (distance <= detectionRange)
    {
        Vector2 direction = (targetPos - rb.position).normalized;
        Vector2 newPos = rb.position + direction * moveSpeed * Time.fixedDeltaTime;

        // Use MovePosition to avoid applying force to the player
        rb.MovePosition(newPos);
    }
    else
    {
        rb.velocity = Vector2.zero;
    }
}

}
