using UnityEngine;

public class PatrolEnemy : BaseEnemy
{
    public float patrolDistance = 3f;
    public bool patrolHorizontally = true;

    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 targetPoint;
    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start();

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (patrolHorizontally)
        {
            pointA = transform.position - new Vector3(patrolDistance, 0, 0);
            pointB = transform.position + new Vector3(patrolDistance, 0, 0);
        }
        else
        {
            pointA = transform.position - new Vector3(0, patrolDistance, 0);
            pointB = transform.position + new Vector3(0, patrolDistance, 0);
        }

        targetPoint = pointB;
    }

    public override void Move()
    {
        // Move towards target
        Vector3 oldPosition = transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);

        // Flip sprite based on movement
            if (transform.position.x > oldPosition.x)
                spriteRenderer.flipX = true;  // moving right
            else if (transform.position.x < oldPosition.x)
                spriteRenderer.flipX = false; // moving left

        // Check if reached target point
        if (Vector3.Distance(transform.position, targetPoint) < 0.1f)
            targetPoint = (targetPoint == pointA) ? pointB : pointA;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If enemy hits a wall, reverse direction
        if (collision.gameObject.CompareTag("Wall"))
        {
            targetPoint = (targetPoint == pointA) ? pointB : pointA;
        }
    }
}

