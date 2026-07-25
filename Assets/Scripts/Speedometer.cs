using UnityEngine;

/// <summary>
/// Rotates a needle to indicate slow/normal/quick based on the player's current scale stage.
/// Small = slow, Normal = normal, Large = quick.
/// </summary>
public class Speedometer : MonoBehaviour
{
    [Header("References")]
    public PlayerScale playerScale;
    public RectTransform needle;

    [Header("Needle Angles (local Z, degrees)")]
    public float slowAngle = 45f;
    public float normalAngle = 0f;
    public float quickAngle = -45f;

    [Header("Smoothing")]
    public float rotateSpeed = 220f;

    void Update()
    {
        if (playerScale == null || needle == null) return;

        float targetAngle;
        switch (playerScale.CurrentScaleStage)
        {
            case PlayerScale.ScaleStage.Small:
                targetAngle = slowAngle;
                break;
            case PlayerScale.ScaleStage.Large:
                targetAngle = quickAngle;
                break;
            default:
                targetAngle = normalAngle;
                break;
        }

        float currentAngle = needle.localEulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotateSpeed * Time.deltaTime);
        needle.localRotation = Quaternion.Euler(0f, 0f, newAngle);
    }
}