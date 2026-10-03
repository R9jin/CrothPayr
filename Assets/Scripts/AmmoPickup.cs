using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AmmoPickup : MonoBehaviour
{
    [Header("Ammo Settings")]
    [SerializeField] private int ammoAmount = 10;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;

    private Vector3 startPosition;
    private bool isCollected = false;

    private void Start()
    {
        startPosition = transform.position;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null || other.GetComponentInParent<PlayerMovement>() != null)
        {
            isCollected = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectAmmo(ammoAmount);
            }

            // Play the reload sound from the player's own AudioSource (diegetic)
            PlayerShooting shooting = other.GetComponent<PlayerShooting>();
            if (shooting == null) shooting = other.GetComponentInParent<PlayerShooting>();
            if (shooting != null) shooting.PlayReloadSound();

            Destroy(gameObject);
        }
    }
}
