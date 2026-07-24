using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("Launch Settings")]
    public float launchForce = 20f;
    public bool launchAlongTransformUp = false; // false = always straight up, true = perpendicular to pad's rotation

    [Header("Detection")]
    public LayerMask playerLayer;

    [Header("Feedback (optional)")]
    public Animator animator;
    public string triggerName = "Launch";

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryLaunch(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryLaunch(other.gameObject);
    }

    void TryLaunch(GameObject obj)
    {
        if (((1 << obj.layer) & playerLayer) == 0) return;

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector2 direction = launchAlongTransformUp ? (Vector2)transform.up : Vector2.up;
        Vector2 launchVelocity = direction * launchForce;

        PlayerController2D playerController = obj.GetComponent<PlayerController2D>();
        if (playerController != null)
        {
            playerController.Launch(launchVelocity);
            playerController.ResetJumps();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(launchVelocity, ForceMode2D.Impulse);
        }

        if (animator != null)
            animator.SetTrigger(triggerName);
    }

    void OnDrawGizmosSelected()
    {
        Vector2 dir = launchAlongTransformUp ? (Vector2)transform.up : Vector2.up;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)dir * 1.5f);
    }
}