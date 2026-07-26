using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public float smoothTime = 0.15f;
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("Bounds")]
    public bool useBounds = false;
    public float minX, maxX, minY, maxY;

    private Vector3 velocity = Vector3.zero;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position, desiredPosition, ref velocity, smoothTime
        );

        if (useBounds && cam != null && cam.orthographic)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            float loX = Mathf.Min(minX, maxX);
            float hiX = Mathf.Max(minX, maxX);
            float loY = Mathf.Min(minY, maxY);
            float hiY = Mathf.Max(minY, maxY);

            float clampMinX = loX + halfWidth;
            float clampMaxX = hiX - halfWidth;
            float clampMinY = loY + halfHeight;
            float clampMaxY = hiY - halfHeight;

            smoothedPosition.x = clampMinX <= clampMaxX
                ? Mathf.Clamp(smoothedPosition.x, clampMinX, clampMaxX)
                : (loX + hiX) * 0.5f;

            smoothedPosition.y = clampMinY <= clampMaxY
                ? Mathf.Clamp(smoothedPosition.y, clampMinY, clampMaxY)
                : (loY + hiY) * 0.5f;
        }

        transform.position = smoothedPosition;
    }

    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        float loX = Mathf.Min(minX, maxX);
        float hiX = Mathf.Max(minX, maxX);
        float loY = Mathf.Min(minY, maxY);
        float hiY = Mathf.Max(minY, maxY);

        Gizmos.color = Color.cyan;
        DrawRect(loX, hiX, loY, hiY);

        if (cam != null && cam.orthographic)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            Gizmos.color = Color.magenta;
            DrawRect(loX + halfWidth, hiX - halfWidth, loY + halfHeight, hiY - halfHeight);
        }
    }

    void DrawRect(float xMin, float xMax, float yMin, float yMax)
    {
        Vector3 topLeft = new Vector3(xMin, yMax, 0f);
        Vector3 topRight = new Vector3(xMax, yMax, 0f);
        Vector3 bottomLeft = new Vector3(xMin, yMin, 0f);
        Vector3 bottomRight = new Vector3(xMax, yMin, 0f);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}