using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;

    [Header("Knockback")]
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.15f;

    [Header("Death")]
    public float deathAnimDuration = 0.5f; // time to let death animation play before destroying

    private int currentHealth;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isKnockedBack = false;
    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (isDead) return;

        currentHealth -= amount;
        ApplyKnockback(sourcePosition);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null) animator.SetTrigger("Hurt");
        }
    }

    void ApplyKnockback(Vector2 sourcePosition)
    {
        Vector2 direction = ((Vector2)transform.position - sourcePosition).normalized;
        isKnockedBack = true;
        rb.linearVelocity = new Vector2(direction.x * knockbackForce, knockbackForce * 0.3f);
        CancelInvoke(nameof(EndKnockback));
        Invoke(nameof(EndKnockback), knockbackDuration);
    }

    void EndKnockback()
    {
        isKnockedBack = false;
    }

    public bool IsKnockedBack => isKnockedBack;
    public bool IsDead => isDead;

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // stop physics from pushing the corpse around

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false; // no more contact damage / hits once dead

        if (animator != null) animator.SetTrigger("Death");

        Destroy(gameObject, deathAnimDuration);
    }
}