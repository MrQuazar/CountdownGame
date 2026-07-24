using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BasicPatrolEnemy : MonoBehaviour
{
    [Header("Patrol")]
    public float moveSpeed = 2f;
    public Transform pointA;
    public Transform pointB;

    [Header("Contact Damage")]
    public int contactDamage = 1;

    private Rigidbody2D rb;
    private Transform currentTarget;
    private float facing = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentTarget = pointB;
    }

    void FixedUpdate()
    {
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null && health.IsKnockedBack) return;
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        facing = direction.x >= 0 ? 1f : -1f;
        transform.localScale = new Vector3(facing, 1f, 1f);

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage, transform.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.2f);
            Gizmos.DrawWireSphere(pointB.position, 0.2f);
        }
    }
}