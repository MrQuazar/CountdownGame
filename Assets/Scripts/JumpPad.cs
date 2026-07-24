using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("Launch Settings")]
    public float launchForce = 20f;
    public bool useCustomDirection = false;
    public Vector2 launchDirection = Vector2.up;
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

        Vector2 direction = useCustomDirection ? launchDirection.normalized : Vector2.up;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(direction * launchForce, ForceMode2D.Impulse);

        PlayerController2D playerController = obj.GetComponent<PlayerController2D>();
        if (playerController != null)
            playerController.ResetJumps();

        if (animator != null)
            animator.SetTrigger(triggerName);
    }
}