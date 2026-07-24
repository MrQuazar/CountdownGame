using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;
    public int MaxHealth => maxHealth;

    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.2f;
    public float invincibilityDuration = 0.8f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isInvincible = false;
    private bool isKnockedBack = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (isInvincible) return;

        currentHealth -= amount;
        ApplyKnockback(sourcePosition);
        StartCoroutine(InvincibilityFlash());

        if (currentHealth <= 0)
            Die();
    }

    void ApplyKnockback(Vector2 sourcePosition)
    {
        Vector2 direction = ((Vector2)transform.position - sourcePosition).normalized;
        isKnockedBack = true;
        rb.linearVelocity = new Vector2(direction.x * knockbackForce, knockbackForce * 0.5f);
        Invoke(nameof(EndKnockback), knockbackDuration);
    }

    void EndKnockback()
    {
        isKnockedBack = false;
    }

    public bool IsKnockedBack => isKnockedBack;

    System.Collections.IEnumerator InvincibilityFlash()
    {
        isInvincible = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float flashTimer = 0f;
        bool visible = true;

        while (flashTimer < invincibilityDuration)
        {
            visible = !visible;
            if (sr != null) sr.color = visible ? Color.white : new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSeconds(0.1f);
            flashTimer += 0.1f;
        }

        if (sr != null) sr.color = Color.white;
        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Player died!");
    }

    public int CurrentHealth => currentHealth;
}