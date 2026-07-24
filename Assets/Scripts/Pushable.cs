using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Pushable : MonoBehaviour
{
    [Header("Push Settings")]
    public float pushSpeed = 2f;
    public float stopDrag = 8f;

    [Header("Player Animation (optional)")]
    public string pushAnimBool = "IsPushing";

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Rigidbody2D playerRb = collision.rigidbody;
        if (playerRb == null || collision.gameObject.GetComponent<PlayerController2D>() == null) return;

        // Which side is the player pushing from, and are they moving into us?
        float sideSign = Mathf.Sign(transform.position.x - playerRb.position.x);
        float playerMoveDir = Mathf.Sign(playerRb.linearVelocity.x);
        bool isPushingIntoUs = Mathf.Abs(playerRb.linearVelocity.x) > 0.05f && playerMoveDir == sideSign;

        if (isPushingIntoUs)
        {
            rb.linearVelocity = new Vector2(playerMoveDir * pushSpeed, rb.linearVelocity.y);
            SetPushAnim(collision.gameObject, true);
        }
        else
        {
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, 0f, stopDrag * Time.fixedDeltaTime),
                rb.linearVelocity.y);
            SetPushAnim(collision.gameObject, false);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController2D>() != null)
            SetPushAnim(collision.gameObject, false);
    }

    void SetPushAnim(GameObject player, bool state)
    {
        if (string.IsNullOrEmpty(pushAnimBool)) return;
        Animator anim = player.GetComponent<Animator>();
        if (anim != null) anim.SetBool(pushAnimBool, state);
    }
}