using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float damage = 35f;
    private float speed = 50f;
    private bool hasHit = false;

    public void Init(Color color, float bulletSpeed)
    {
        speed = bulletSpeed;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }

        // Apply orange emissive color to the bullet mesh via MaterialPropertyBlock
        // (does not mutate the shared material asset).
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", color);
            // HDR emission: multiply color brightness well above 1 so Bloom picks it up.
            mpb.SetColor("_EmissionColor", color * 8f);
            meshRenderer.SetPropertyBlock(mpb);
        }

        // Destroy after 4 seconds if no collision
        Destroy(gameObject, 4f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        ProcessHit(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcessHit(other.gameObject);
    }

    private void ProcessHit(GameObject hitObj)
    {
        if (hasHit || hitObj == null) return;

        // Ignore hits on Player or any player child/parent
        if (hitObj.CompareTag("Player") || hitObj.name.Contains("Player") || (hitObj.transform.root != null && hitObj.transform.root.name.Contains("Player")))
        {
            return;
        }

        // Ignore pickups (ammo pickup, etc.)
        if (hitObj.GetComponent<AmmoPickup>() != null || hitObj.GetComponentInParent<AmmoPickup>() != null)
        {
            return;
        }

        hasHit = true;
        NoiseSystem.Emit(transform.position, 12f, gameObject);

        TrainingDummy dummy = hitObj.GetComponentInParent<TrainingDummy>();
        if (dummy == null)
        {
            dummy = hitObj.GetComponent<TrainingDummy>();
        }

        if (dummy != null)
        {
            dummy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}