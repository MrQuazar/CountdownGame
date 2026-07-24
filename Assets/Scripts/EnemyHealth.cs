using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;

    [Header("Knockback")]
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.15f;

    private int currentHealth;
    private Rigidbody2D rb;
    private bool isKnockedBack = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        currentHealth -= amount;
        ApplyKnockback(sourcePosition);

        if (currentHealth <= 0)
            Die();
    }

    void ApplyKnockback(Vector2 sourcePosition)
    {
        Vector2 direction = ((Vector2)transform.position - sourcePosition).normalized;
        isKnockedBack = true;
        rb.linearVelocity = new Vector2(direction.x * knockbackForce, knockbackForce * 0.3f);
        CancelInvoke(nameof(EndKnockback)); // in case hit again mid-knockback
        Invoke(nameof(EndKnockback), knockbackDuration);
    }

    void EndKnockback()
    {
        isKnockedBack = false;
    }

    public bool IsKnockedBack => isKnockedBack;

    void Die()
    {
        Destroy(gameObject);
    }
}