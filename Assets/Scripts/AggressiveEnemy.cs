using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AggressiveEnemy : MonoBehaviour
{
    [Header("Patrol")]
    public float moveSpeed = 2f;
    public Transform pointA;
    public Transform pointB;

    [Header("Attack")]
    public int damage = 1;
    public float attackRate = 1.5f;
    public float attackRange = 1f;
    public Transform attackPoint;

    [Header("Attack Visual")]
    public SpriteRenderer attackVisual;
    public float attackVisualDuration = 0.15f;

    [Header("Aggro")]
    public float aggroRange = 4f;
    public LayerMask playerLayer;
    public float deAggroDelay = 2f;

    private float loseAggroTimer = 0f;
    private bool waitingToDeAggro = false;

    [Header("Contact")]
    public int contactDamage = 1;

    private Rigidbody2D rb;
    private Animator animator;
    private EnemyHealth health;
    private Transform currentPatrolTarget;
    private Transform player;
    private float facing = 1f;
    private float attackCooldownTimer = 0f;
    private bool isAggro = false;
    private Coroutine attackVisualRoutine;
    private AudioSource moveLoopSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        currentPatrolTarget = pointB;

        if (attackVisual != null)
            attackVisual.enabled = false;

        moveLoopSource = gameObject.AddComponent<AudioSource>();
        moveLoopSource.playOnAwake = false;
        moveLoopSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (health != null && health.IsDead) return;

        attackCooldownTimer -= Time.deltaTime;
        CheckAggro();

        if (animator != null)
        {
            animator.SetBool("IsChasing", isAggro && !waitingToDeAggro);
            animator.SetBool("IsWaiting", waitingToDeAggro);
        }
    }

    void FixedUpdate()
    {
        if (health != null && (health.IsKnockedBack || health.IsDead))
        {
            AudioManager.Instance?.StopLoopSFX(moveLoopSource);
            return;
        }

        if (waitingToDeAggro)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            AudioManager.Instance?.StopLoopSFX(moveLoopSource);
            return;
        }

        if (isAggro && player != null)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        if (Mathf.Abs(rb.linearVelocity.x) > 0.05f)
            AudioManager.Instance?.StartLoopSFX(moveLoopSource, isAggro ? SFXType.ChasingEnemyChaseMove : SFXType.ChasingEnemyPatrolMove);
        else
            AudioManager.Instance?.StopLoopSFX(moveLoopSource);
    }

    void CheckAggro()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, aggroRange, playerLayer);

        if (hit != null)
        {
            isAggro = true;
            player = hit.transform;
            waitingToDeAggro = false;
            loseAggroTimer = 0f;
        }
        else if (isAggro && !waitingToDeAggro)
        {
            waitingToDeAggro = true;
            loseAggroTimer = deAggroDelay;
        }
        else if (waitingToDeAggro)
        {
            loseAggroTimer -= Time.deltaTime;
            if (loseAggroTimer <= 0f)
            {
                isAggro = false;
                waitingToDeAggro = false;
                player = null;
                TeleportToPatrolPath();
            }
        }
    }

    void TeleportToPatrolPath()
    {

        float distToA = Vector2.Distance(transform.position, pointA.position);
        float distToB = Vector2.Distance(transform.position, pointB.position);

        if (distToA < distToB)
        {
            transform.position = pointA.position;
            currentPatrolTarget = pointB;
        }
        else
        {
            transform.position = pointB.position;
            currentPatrolTarget = pointA;
        }

        rb.linearVelocity = Vector2.zero;
    }

    void Patrol()
    {
        Vector2 direction = (currentPatrolTarget.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        UpdateFacing(direction.x);
        if (Vector2.Distance(transform.position, currentPatrolTarget.position) < 0.5f)
        {
            currentPatrolTarget = currentPatrolTarget == pointA ? pointB : pointA;
        }
    }

    void ChasePlayer()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 direction = (player.position - transform.position).normalized;

        UpdateFacing(direction.x);

        if (distToPlayer > attackRange)
        {
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (attackCooldownTimer <= 0f)
            {
                Attack();
            }
        }
    }

    void UpdateFacing(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.01f) return;
        facing = dirX >= 0 ? 1f : -1f;
        transform.localScale = new Vector3(facing, 1f, 1f);
    }

    void Attack()
    {
        attackCooldownTimer = attackRate;
        if (animator != null) animator.SetTrigger("Attack");
        AudioManager.Instance?.PlaySFX(SFXType.ChasingEnemyAttack, transform.position);

        ShowAttackVisual();

        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, 0.5f, playerLayer);
        if (hit != null)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage, transform.position);
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (health != null && health.IsDead) return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage, transform.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, 0.5f);
        }

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}