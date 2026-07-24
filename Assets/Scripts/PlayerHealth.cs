using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("UI")]
    public GameObject loseScreen;

    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;
    public int MaxHealth => maxHealth;

    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.2f;
    public float invincibilityDuration = 0.8f;

    [Header("Death")]
    public string deathTriggerName = "Death";

    private Rigidbody2D rb;
    private Animator animator;
    private bool isInvincible = false;
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
        if (isInvincible || isDead) return;

        PlayerController2D controller = GetComponent<PlayerController2D>();
        float multiplier = controller != null ? controller.DamageTakenMultiplier : 1f;
        int scaledAmount = Mathf.Max(1, Mathf.RoundToInt(amount * multiplier));

        currentHealth -= scaledAmount;
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
    public bool IsDead => isDead;

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
        if (isDead) return;
        isDead = true;

        CancelInvoke(nameof(EndKnockback));
        StopAllCoroutines();
        isKnockedBack = false;
        isInvincible = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;

        if (animator != null)
            animator.SetTrigger(deathTriggerName);

        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        DisablePlayerControl();

        Debug.Log("Player died!");
        if (loseScreen != null) loseScreen.SetActive(true);
    }

    void DisablePlayerControl()
    {
        PlayerController2D controller = GetComponent<PlayerController2D>();
        if (controller != null) controller.enabled = false;

        PlayerScale scale = GetComponent<PlayerScale>();
        if (scale != null) scale.enabled = false;
    }

    public int CurrentHealth => currentHealth;
}