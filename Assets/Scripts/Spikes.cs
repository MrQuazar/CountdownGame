using UnityEngine;

public class Spikes : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 1;

    [Header("Teleport")]
    public Transform teleportTarget;

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TryDamage(collision.gameObject);
    }

    void TryDamage(GameObject obj)
    {
        PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage, transform.position);

            if (teleportTarget != null)
            {
                PlayerController2D controller = obj.GetComponent<PlayerController2D>();
                if (controller != null)
                {
                    controller.TeleportTo(teleportTarget.position);
                }
            }
        }
    }
}