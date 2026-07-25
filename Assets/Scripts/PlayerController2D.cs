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
    [Tooltip("Index 0 = Small, 1 = Normal, 2 = Large.")]
    public float[] jumpForces = { 10f, 14f, 18f };
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    private int MaxJumps => playerScale.CurrentScaleStage == PlayerScale.ScaleStage.Large ? 2 : 1;
    private float JumpForce => jumpForces[Mathf.Clamp((int)playerScale.CurrentScaleStage, 0, jumpForces.Length - 1)];

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.6f;
    private bool airDashUsed = false;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 0.6f;
    public float attackCooldown = 0.35f;
    [Tooltip("Index 0 = Small, 1 = Normal, 2 = Large.")]
    public int[] attackDamages = { 1, 1, 2 };
    public LayerMask enemyLayer;
    private int AttackDamage => attackDamages[Mathf.Clamp((int)playerScale.CurrentScaleStage, 0, attackDamages.Length - 1)];

    [Header("Attack Visual")]
    public SpriteRenderer attackVisual;
    public float attackVisualDuration = 0.15f;

    [Header("Damage Taken")]
    [Tooltip("Index 0 = Small, 1 = Normal, 2 = Large. Multiplies incoming damage — read by PlayerHealth.")]
    public float[] damageTakenMultipliers = { 1.5f, 1f, 0.6f };
    public float DamageTakenMultiplier => damageTakenMultipliers[Mathf.Clamp((int)playerScale.CurrentScaleStage, 0, damageTakenMultipliers.Length - 1)];

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Launch (Jump Pads)")]
    public float launchControlLockDuration = 0.25f;
    private bool isLaunching = false;
    private float launchTimer = 0f;

    [Header("Moving Platform")]
    private MovingPlatform currentPlatform = null;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;
    private Animator animator;
    private float facing = 1f;

    private int jumpsUsed = 0;
    private bool justJumped = false;
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
        if (isLaunching)
        {
            launchTimer -= Time.deltaTime;
            if (launchTimer <= 0f)
                isLaunching = false;
        }
        moveInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && rb.linearVelocity.y <= 0f)
        {
            jumpsUsed = 0;
            airDashUsed = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing)
        {
            bool canJump = false;

            if (jumpsUsed == 0)
            {
                canJump = isGrounded;
            }
            else
            {
                canJump = jumpsUsed < MaxJumps;
            }

            if (canJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
                jumpsUsed++;
                justJumped = true;
            }
        }
        dashCooldownTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && !isDashing
            && playerScale.CurrentScaleStage == PlayerScale.ScaleStage.Small
            && (isGrounded || !airDashUsed))
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

        Vector2 platformVelocity = (currentPlatform != null && isGrounded) ? currentPlatform.Velocity : Vector2.zero;

        if (isDashing)
        {
            rb.linearVelocity = new Vector2(facing * dashSpeed, 0f);
            return;
        }

        float horizontalVelocity = moveInput * moveSpeed + platformVelocity.x;

        if (platformVelocity.y != 0f && !justJumped)
        {
            rb.linearVelocity = new Vector2(horizontalVelocity, platformVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontalVelocity, rb.linearVelocity.y);
        }

        justJumped = false;

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

        if (!isGrounded)
            airDashUsed = true;
    }

    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 1f;
    }

    public void ResetJumps()
    {
        jumpsUsed = 0;
    }
    public void Launch(Vector2 velocity, float lockDuration = -1f)
    {
        rb.linearVelocity = velocity;
        isLaunching = true;
        launchTimer = lockDuration > 0f ? lockDuration : launchControlLockDuration;
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
                enemyHealth.TakeDamage(AttackDamage, attackPoint.position);
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

    void OnCollisionEnter2D(Collision2D collision) => CheckPlatformContact(collision);
    void OnCollisionStay2D(Collision2D collision) => CheckPlatformContact(collision);

    void OnCollisionExit2D(Collision2D collision)
    {
        MovingPlatform platform = collision.gameObject.GetComponent<MovingPlatform>();
        if (platform != null && platform == currentPlatform)
        {
            currentPlatform = null;
        }
    }

    void CheckPlatformContact(Collision2D collision)
    {
        MovingPlatform platform = collision.gameObject.GetComponent<MovingPlatform>();
        if (platform == null) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y == 1)
            {
                currentPlatform = platform;
                return;
            }
        }
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