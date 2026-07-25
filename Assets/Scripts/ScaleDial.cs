using UnityEngine;

public class ScaleDial : MonoBehaviour
{
    [Header("References")]
    public PlayerScale playerScale;
    public Transform player;
    public Transform reel; // holds the 3 form sprites, rolls up/down

    [Header("Form Sprites (children of Reel)")]
    public SpriteRenderer smallSprite;
    public SpriteRenderer normalSprite;
    public SpriteRenderer largeSprite;

    [Header("Static Direction Arrows (not children of Reel — do not move)")]
    public SpriteRenderer upperArrow;
    public SpriteRenderer lowerArrow;

    [Header("Follow (keeps this beside the player, not a child of it)")]
    public Vector2 followOffset = new Vector2(-2.5f, 0f);

    [Header("Reel")]
    public float[] slotSpacing = {0f,1f,1.2f};
    public float rollSpeed = 6f; // units/sec

    [Header("Opacity")]
    [Range(0f, 1f)] public float activeAlpha = 1f;
    [Range(0f, 1f)] public float inactiveAlpha = 0.4f;

    [Header("Visibility")]
    [Tooltip("How long the dial stays visible after a scale change.")]
    public float displayDuration = 2f;

    private int lastStage = -1;
    private float hideTimer = 0f;
    private bool visible = false;

    void Start()
    {
        if (playerScale != null) lastStage = (int)playerScale.CurrentScaleStage;
        SetVisible(false);
    }

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = new Vector3(
                player.position.x + followOffset.x,
                player.position.y + followOffset.y,
                transform.position.z);
        }

        if (playerScale == null || reel == null) return;

        int stage = (int)playerScale.CurrentScaleStage; // 0 = Small, 1 = Normal, 2 = Large

        if (stage != lastStage)
        {
            lastStage = stage;
            hideTimer = displayDuration;
            SetVisible(true);
        }

        if (visible)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f) SetVisible(false);
        }

        float targetY = (stage - 1) * slotSpacing[stage];
        Vector3 pos = reel.localPosition;
        pos.y = Mathf.MoveTowards(pos.y, targetY, rollSpeed * Time.deltaTime);
        reel.localPosition = pos;

        UpdateAlphas(stage);
    }

    void UpdateAlphas(int stage)
    {
        SetAlpha(smallSprite, stage == 0 ? activeAlpha : inactiveAlpha);
        SetAlpha(normalSprite, stage == 1 ? activeAlpha : inactiveAlpha);
        SetAlpha(largeSprite, stage == 2 ? activeAlpha : inactiveAlpha);
    }

    void SetAlpha(SpriteRenderer sr, float a)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }

    void SetVisible(bool state)
    {
        visible = state;
        if (smallSprite != null) smallSprite.enabled = state;
        if (normalSprite != null) normalSprite.enabled = state;
        if (largeSprite != null) largeSprite.enabled = state;
        if (upperArrow != null) upperArrow.enabled = state;
        if (lowerArrow != null) lowerArrow.enabled = state;
    }
}