using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    public PlayerScale playerScale;
    public PlayerHealth playerHealth;

    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    private int MaxJumps => playerScale.CurrentScaleStage == PlayerScale.ScaleStage.Large ? 2 : 1;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.6f;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 0.6f;
    public float attackCooldown = 0.35f;
    public int attackDamage = 1;
    public LayerMask enemyLayer;

    [Header("Attack Visual")]
    public SpriteRenderer attackVisual;
    public float attackVisualDuration = 0.15f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;
    private Animator animator;
    private float facing = 1f;

    private int jumpsRemaining;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    private float attackCooldownTimer = 0f;
    private Coroutine attackVisualRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Make sure the visual starts hidden.
        if (attackVisual != null)
            attackVisual.enabled = false;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && rb.linearVelocity.y <= 0f)
            jumpsRemaining = MaxJumps;

        if (Input.GetKeyDown(KeyCode.Space) && jumpsRemaining > 0 && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsRemaining--;
        }

        dashCooldownTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && !isDashing
            && playerScale.CurrentScaleStage == PlayerScale.ScaleStage.Small)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                EndDash();
        }

        attackCooldownTimer -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && attackCooldownTimer <= 0f)
        {
            Attack();
        }

        if (moveInput != 0 && !isDashing)
        {
            facing = Mathf.Sign(moveInput);
            playerScale.SetFacing(moveInput);
        }

        if (attackVisual != null)
        {
            Vector3 scale = attackVisual.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facing;
            attackVisual.transform.localScale = scale;
        }

        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsGrounded", isGrounded);
    }

    void FixedUpdate()
    {
        if (playerHealth != null && playerHealth.IsKnockedBack) return;
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(facing * dashSpeed, 0f);
            return;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        rb.gravityScale = 0f;
        animator.SetTrigger("Dash");
    }

    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 1f;
    }

    public void ResetJumps()
    {
        jumpsRemaining = MaxJumps;
    }

    void Attack()
    {
        attackCooldownTimer = attackCooldown;
        animator.SetTrigger("Attack");

        ShowAttackVisual();

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hits)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
                enemyHealth.TakeDamage(attackDamage, attackPoint.position);
        }
    }

    void ShowAttackVisual()
    {
        if (attackVisual == null) return;

        if (attackVisualRoutine != null)
            StopCoroutine(attackVisualRoutine);

        attackVisualRoutine = StartCoroutine(AttackVisualCoroutine());
    }

    private IEnumerator AttackVisualCoroutine()
    {
        attackVisual.enabled = true;
        yield return new WaitForSeconds(attackVisualDuration);
        attackVisual.enabled = false;
        attackVisualRoutine = null;
    }

    public void AnimEvent_ShowAttackVisual()
    {
        if (attackVisual != null) attackVisual.enabled = true;
    }

    public void AnimEvent_HideAttackVisual()
    {
        if (attackVisual != null) attackVisual.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}