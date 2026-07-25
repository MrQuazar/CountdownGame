using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScale : MonoBehaviour
{
    [Header("Scale Stages")]
    // Index 0 = Small, 1 = Normal, 2 = Large
    public float[] scaleStages = { 0.5f, 1f, 2f };
    public int startStageIndex = 1;
    public enum ScaleStage { Small = 0, Normal = 1, Large = 2 }
    public ScaleStage CurrentScaleStage => (ScaleStage)currentStage;
    
    [Header("Collision Check")]
    public LayerMask obstacleLayer;
    public float headCheckPadding = 0.02f;

    [Header("Input")]
    public float inputCooldown = 0.15f;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Vector2 baseColliderSize;
    private Vector2 baseColliderOffset;
    private int currentStage;
    public int CurrentStage => currentStage;
    private float facing = 1f;
    private float cooldownTimer = 0f;

    private int pendingStage = -1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        baseColliderSize = capsuleCollider.size;
        baseColliderOffset = capsuleCollider.offset;

        currentStage = startStageIndex;
        transform.localScale = new Vector3(facing * scaleStages[currentStage], scaleStages[currentStage], 1f);

        AudioManager.Instance?.PlayGameMusicForStage(currentStage);
    }

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        int targetStage = currentStage;

        if (Input.GetKeyDown(KeyCode.W)) targetStage = Mathf.Min(currentStage + 1, scaleStages.Length - 1);
        else if (Input.GetKeyDown(KeyCode.S)) targetStage = Mathf.Max(currentStage - 1, 0);

        if (targetStage == currentStage) return;

        if (CanScaleTo(scaleStages[targetStage]))
        {
            pendingStage = targetStage;
            cooldownTimer = inputCooldown;
        }
        // else: blocked, stay at current stage, no cooldown so player can try again next frame
    }

    void FixedUpdate()
    {
        if (pendingStage < 0) return;
        ApplyScale(pendingStage);
        pendingStage = -1;
    }

    bool CanScaleTo(float testScale)
    {
        float currentScale = scaleStages[currentStage];

        if (testScale <= currentScale) return true;

        float currentHalfHeight = (baseColliderSize.y * currentScale) / 2f;
        float feetY = rb.position.y + (baseColliderOffset.y * currentScale) - currentHalfHeight;

        float currentTopY = feetY + (baseColliderSize.y * currentScale);
        float newTopY = feetY + (baseColliderSize.y * testScale);

        float addedHeight = newTopY - currentTopY;
        if (addedHeight <= 0f) return true;

        float checkWidth = baseColliderSize.x * testScale;
        Vector2 checkSize = new Vector2(checkWidth - headCheckPadding, addedHeight);
        Vector2 checkCenter = new Vector2(rb.position.x, currentTopY + addedHeight / 2f);

        Collider2D hit = Physics2D.OverlapBox(checkCenter, checkSize, 0f, obstacleLayer);
        return hit == null;
    }

    void ApplyScale(int newStageIndex)
    {
        float oldScale = scaleStages[currentStage];
        float newScale = scaleStages[newStageIndex];

        float oldHalfHeight = (baseColliderSize.y * oldScale) / 2f;
        float feetY = rb.position.y + (baseColliderOffset.y * oldScale) - oldHalfHeight;

        currentStage = newStageIndex;
        transform.localScale = new Vector3(facing * newScale, newScale, 1f);
        AudioManager.Instance?.PlayGameMusicForStage(currentStage);
        AudioManager.Instance?.PlaySFX(newScale > oldScale ? SFXType.ScaleUp : SFXType.ScaleDown, transform.position);

        float newHalfHeight = (baseColliderSize.y * newScale) / 2f;
        float newY = feetY - (baseColliderOffset.y * newScale) + newHalfHeight;

        rb.MovePosition(new Vector2(rb.position.x, newY));
    }

    public void SetFacing(float direction)
    {
        facing = Mathf.Sign(direction);
        transform.localScale = new Vector3(facing * scaleStages[currentStage], scaleStages[currentStage], 1f);
    }
}