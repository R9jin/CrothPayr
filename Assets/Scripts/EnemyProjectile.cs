using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 26f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 4f;

    private string attackerName = "Enemy AI";
    private GameObject shooterObj;
    private bool hasHit = false;

    public void Init(string shooterName, GameObject shooter, Color bulletColor, float bulletDamage = 15f, float bulletSpeed = 26f)
    {
        attackerName = shooterName;
        shooterObj = shooter;
        damage = bulletDamage;
        speed = bulletSpeed;

        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = 0.3f;
        }

        // Apply color & emission to mesh
        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        if (mr != null)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mr.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", bulletColor);
            mpb.SetColor("_EmissionColor", bulletColor * 5f);
            mr.SetPropertyBlock(mpb);
        }

        Light l = GetComponentInChildren<Light>();
        if (l != null)
        {
            l.color = bulletColor;
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Ignore shooter or child of shooter
        if (shooterObj != null && (other.gameObject == shooterObj || other.transform.IsChildOf(shooterObj.transform)))
        {
            return;
        }

        // Ignore pickups or other enemy bullets
        if (other.GetComponent<AmmoPickup>() != null || other.GetComponent<EnemyProjectile>() != null || other.GetComponent<Bullet>() != null)
        {
            return;
        }

        // Ignore enemies (don't friendly-fire other AI)
        if (other.GetComponentInParent<TrainingDummy>() != null)
        {
            return;
        }

        hasHit = true;

        // Check if player
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage, attackerName);
        }
        else
        {
            // Impact with environment - emits impact noise
            NoiseSystem.Emit(transform.position, 12f, gameObject);
        }

        Destroy(gameObject);
    }
}
