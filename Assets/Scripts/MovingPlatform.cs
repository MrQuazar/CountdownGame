using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    public enum MotionType { Linear, Circular, Rotate }
    public enum RotationDirection { Clockwise, CounterClockwise }
    public enum LoopMode { Infinite, FixedCount }

    [Header("Motion Type")]
    public MotionType motionType = MotionType.Linear;

    [Header("Linear Settings")]
    public Transform pointA;
    public Transform pointB;
    public float travelDuration = 2f;
    public float waitDuration = 1f;

    [Header("Circular Settings")]
    public Transform circleCenter;
    public float circleRadius = 3f;
    public float circleSpeed = 90f;
    public RotationDirection direction = RotationDirection.Clockwise;

    [Header("Self-Rotate Settings")]
    public float rotateSpeed = 90f; // degrees per second
    public RotationDirection rotateDirection = RotationDirection.Clockwise;

    [Header("Oscillation / Looping")]
    public LoopMode loopMode = LoopMode.Infinite;
    public int loopCount = 3;
    // Linear: 1 loop = A→B→A. Circular: 1 loop = one 360° orbit. Rotate: 1 loop = one 360° spin.

    private Rigidbody2D rb;
    private int completedLoops = 0;
    private bool motionFinished = false;

    private Vector2 startPoint;
    private Vector2 endPoint;
    private bool movingToB = true;
    private float moveTimer = 0f;
    private bool waiting = false;
    private float waitTimer = 0f;

    private float currentAngle = 0f;      // used by Circular
    private float currentRotation = 0f;   // used by Rotate
    public Vector2 Velocity { get; private set; }
    private Vector2 previousPosition;

    void FixedUpdate()
    {

        if (motionFinished)
        {
            Velocity = Vector2.zero;
            return;
        }

        switch (motionType)
        {
            case MotionType.Linear: UpdateLinear(); break;
            case MotionType.Circular: UpdateCircular(); break;
            case MotionType.Rotate: UpdateRotate(); break;
        }

        Velocity = (rb.position - previousPosition) / Time.fixedDeltaTime;
        previousPosition = rb.position;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Start()
    {
        if (motionType == MotionType.Linear && pointA != null && pointB != null)
        {
            startPoint = pointA.position;
            endPoint = pointB.position;
            rb.position = startPoint;
        }
        else if (motionType == MotionType.Circular && circleCenter != null)
        {
            Vector2 offset = (Vector2)transform.position - (Vector2)circleCenter.position;
            currentAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            circleRadius = offset.magnitude;
        }
        else if (motionType == MotionType.Rotate)
        {
            currentRotation = rb.rotation;
        }
    }


    void UpdateLinear()
    {
        if (waiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0f)
                waiting = false;
            return;
        }

        moveTimer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(moveTimer / travelDuration);

        Vector2 from = movingToB ? startPoint : endPoint;
        Vector2 to = movingToB ? endPoint : startPoint;
        Vector2 newPos = Vector2.Lerp(from, to, t);

        rb.MovePosition(newPos);

        if (t >= 1f)
        {
            moveTimer = 0f;
            waiting = true;
            waitTimer = waitDuration;
            movingToB = !movingToB;

            if (movingToB)
            {
                completedLoops++;
                CheckLoopLimit();
            }
        }
    }

    void UpdateCircular()
    {
        float dir = direction == RotationDirection.Clockwise ? -1f : 1f;
        currentAngle += dir * circleSpeed * Time.fixedDeltaTime;

        if (currentAngle >= 360f || currentAngle <= -360f)
        {
            currentAngle %= 360f;
            completedLoops++;
            CheckLoopLimit();
        }

        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * circleRadius;
        Vector2 newPos = (Vector2)circleCenter.position + offset;

        rb.MovePosition(newPos);
    }

    void UpdateRotate()
    {
        float dir = rotateDirection == RotationDirection.Clockwise ? -1f : 1f;
        float delta = dir * rotateSpeed * Time.fixedDeltaTime;
        currentRotation += delta;

        if (currentRotation >= 360f || currentRotation <= -360f)
        {
            currentRotation %= 360f;
            completedLoops++;
            CheckLoopLimit();
        }

        rb.MoveRotation(rb.rotation + delta);
    }

    void CheckLoopLimit()
    {
        if (loopMode == LoopMode.FixedCount && completedLoops >= loopCount)
        {
            motionFinished = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (motionType == MotionType.Linear && pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.2f);
            Gizmos.DrawWireSphere(pointB.position, 0.2f);
        }
        else if (motionType == MotionType.Circular && circleCenter != null)
        {
            Gizmos.color = Color.cyan;
            float radius = Application.isPlaying ? circleRadius : Vector2.Distance(transform.position, circleCenter.position);
            DrawGizmoCircle(circleCenter.position, radius);
        }
        else if (motionType == MotionType.Rotate)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.3f); // marks the self-rotation pivot (its own center)
        }
    }

    void DrawGizmoCircle(Vector3 center, float radius)
    {
        int segments = 32;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}