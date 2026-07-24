using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class VerticalFan : MonoBehaviour
{
    [Header("Wind Behavior")]
    public float smallFlySpeed = 8f;
    public float topMargin = 0.3f;
    public float largeFallAssist = 0f;

    [Header("Setup")]
    public bool showGizmo = true;

    private BoxCollider2D zone;

    void Awake()
    {
        zone = GetComponent<BoxCollider2D>();
    }

    void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerScale scale = other.GetComponent<PlayerScale>();
        Rigidbody2D rb = other.attachedRigidbody;
        if (scale == null || rb == null) return;

        float topY = zone.bounds.max.y - topMargin;

        switch (scale.CurrentScaleStage)
        {
            case PlayerScale.ScaleStage.Small:
                if (rb.position.y < topY)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, smallFlySpeed);
                else
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                break;

            case PlayerScale.ScaleStage.Normal:
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                break;

            case PlayerScale.ScaleStage.Large:
                if (largeFallAssist > 0f)
                    rb.linearVelocity += Vector2.down * largeFallAssist * Time.fixedDeltaTime;
                break;
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmo) return;
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.35f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.offset, box.size);
    }
}