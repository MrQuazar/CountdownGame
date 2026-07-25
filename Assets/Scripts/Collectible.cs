using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [Header("Optional Feedback")]
    public GameObject collectVFX;
    public AudioClip collectSFX;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController2D>() == null) return;

        if (collectVFX != null)
            Instantiate(collectVFX, transform.position, Quaternion.identity);

        AudioManager.Instance?.PlaySFX(SFXType.Collectible, transform.position, collectSFX);

        if (GameManager.Instance != null)
            GameManager.Instance.CollectItem();

        Destroy(gameObject);
    }
}