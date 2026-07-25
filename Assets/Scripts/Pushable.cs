using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Pushable : MonoBehaviour
{
    [Header("Push Difficulty Per Player Stage")]
    [Tooltip("Index 0 = Small (hardest to push), 1 = Normal, 2 = Large (easiest to push). 1 = moves exactly with player, 0.5 = moves at half the player's speed, 0 = immovable.")]
    public float[] pushDifficultyMultipliers = { 0.4f, 1f, 1f };

    [Header("Player Animation (optional)")]
    public string pushAnimBool = "IsPushing";

    private Rigidbody2D rb;
    private Vector2 lastPlayerPos;
    private bool hasLastPos = false;
    private AudioSource pushLoopSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        pushLoopSource = gameObject.AddComponent<AudioSource>();
        pushLoopSource.playOnAwake = false;
        pushLoopSource.spatialBlend = 0f;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Rigidbody2D playerRb = collision.rigidbody;
        PlayerController2D playerController = collision.gameObject.GetComponent<PlayerController2D>();
        if (playerRb == null || playerController == null) return;

        if (!IsSideContact(collision))
        {
            hasLastPos = false;
            SetPushAnim(collision.gameObject, false);
            AudioManager.Instance?.StopLoopSFX(pushLoopSource);
            return;
        }

        if (!hasLastPos)
        {
            lastPlayerPos = playerRb.position;
            hasLastPos = true;
            return;
        }

        PlayerScale playerScale = collision.gameObject.GetComponent<PlayerScale>();
        float multiplier = 1f;
        if (playerScale != null && pushDifficultyMultipliers.Length > 0)
        {
            int stage = Mathf.Clamp((int)playerScale.CurrentScaleStage, 0, pushDifficultyMultipliers.Length - 1);
            multiplier = pushDifficultyMultipliers[stage];
        }

        float sideSign = Mathf.Sign(transform.position.x - playerRb.position.x);
        float playerMoveDir = Mathf.Sign(playerRb.linearVelocity.x);
        bool isPushingIntoUs = Mathf.Abs(playerRb.linearVelocity.x) > 0.05f && playerMoveDir == sideSign;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (isPushingIntoUs && multiplier > 0f)
        {
            float deltaX = (playerRb.position.x - lastPlayerPos.x) * multiplier;
            rb.MovePosition(new Vector2(rb.position.x + deltaX, rb.position.y));
            SetPushAnim(collision.gameObject, true);
            AudioManager.Instance?.StartLoopSFX(pushLoopSource, SFXType.BoxPush);
        }
        else
        {
            SetPushAnim(collision.gameObject, false);
            AudioManager.Instance?.StopLoopSFX(pushLoopSource);
        }

        lastPlayerPos = playerRb.position;
    }

    bool IsSideContact(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.7f && Mathf.Abs(contact.normal.y) < 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController2D>() != null)
        {
            hasLastPos = false;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            SetPushAnim(collision.gameObject, false);
            AudioManager.Instance?.StopLoopSFX(pushLoopSource);
        }
    }

    void SetPushAnim(GameObject player, bool state)
    {
        if (string.IsNullOrEmpty(pushAnimBool)) return;
        Animator anim = player.GetComponent<Animator>();
        if (anim != null) anim.SetBool(pushAnimBool, state);
    }
}