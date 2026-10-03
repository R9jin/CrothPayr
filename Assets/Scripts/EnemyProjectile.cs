using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 28f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float hitRadius = 0.28f;

    private string attackerName = "Enemy AI";
    private GameObject shooterObj;
    private bool hasHit = false;
    private Rigidbody rb;

    public void Init(string shooterName, GameObject shooter, Color bulletColor, float bulletDamage = 15f, float bulletSpeed = 28f)
    {
        attackerName = shooterName;
        shooterObj = shooter;
        damage = bulletDamage;
        speed = bulletSpeed;

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null) col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = hitRadius;

        // Apply color & emission to mesh
        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        if (mr != null)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mr.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", bulletColor);
            mpb.SetColor("_EmissionColor", bulletColor * 6f);
            mr.SetPropertyBlock(mpb);
        }

        Light l = GetComponentInChildren<Light>();
        if (l == null)
        {
            GameObject lightObj = new GameObject("BulletGlow");
            lightObj.transform.SetParent(transform, false);
            l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 3.5f;
            l.intensity = 2.5f;
        }
        l.color = bulletColor;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (hasHit) return;

        float moveDist = speed * Time.deltaTime;
        Vector3 moveStep = transform.forward * moveDist;
        Vector3 startPos = transform.position;
        Vector3 nextPos = startPos + moveStep;

        // 1. Precise Raycast pass along trajectory (pinpoint accuracy, ignores spawn-overlap artifacts)
        RaycastHit[] rayHits = Physics.RaycastAll(startPos, transform.forward, moveDist);
        if (rayHits != null && rayHits.Length > 0)
        {
            System.Array.Sort(rayHits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in rayHits)
            {
                if (ShouldIgnore(hit.collider, hit.distance)) continue;

                ProcessHit(hit.collider);
                return;
            }
        }

        // 2. Continuous SphereCast sweep for generous bullet volume
        RaycastHit[] sphereHits = Physics.SphereCastAll(startPos, hitRadius, transform.forward, moveDist);
        if (sphereHits != null && sphereHits.Length > 0)
        {
            System.Array.Sort(sphereHits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in sphereHits)
            {
                if (ShouldIgnore(hit.collider, hit.distance)) continue;

                ProcessHit(hit.collider);
                return;
            }
        }

        transform.position = nextPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit || ShouldIgnore(other, 1f)) return;
        ProcessHit(other);
    }

    public bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag("Player")) return true;
        if (other.GetComponentInParent<PlayerHealth>() != null || other.GetComponent<PlayerHealth>() != null) return true;
        if (other.GetComponentInParent<PlayerMovement>() != null || other.GetComponent<PlayerMovement>() != null) return true;
        if (other.name.IndexOf("Player", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (PlayerHealth.Instance != null && (other.transform == PlayerHealth.Instance.transform || other.transform.IsChildOf(PlayerHealth.Instance.transform))) return true;
        return false;
    }

    private bool ShouldIgnore(Collider other, float hitDistance)
    {
        if (other == null || other.gameObject == gameObject) return true;

        // If it's the player, NEVER ignore!
        if (IsPlayer(other)) return false;

        // Ignore shooter or child of shooter
        if (shooterObj != null && (other.gameObject == shooterObj || other.transform.IsChildOf(shooterObj.transform)))
        {
            return true;
        }

        // Ignore pickups or other enemy bullets
        if (other.GetComponent<AmmoPickup>() != null || other.GetComponent<EnemyProjectile>() != null || other.GetComponent<Bullet>() != null)
        {
            return true;
        }

        // Ignore other enemy AI (no friendly-fire between AI)
        if (other.GetComponentInParent<TrainingDummy>() != null || other.GetComponentInParent<BaseEnemyAI>() != null)
        {
            return true;
        }

        // CRITICAL: Ignore initial spawn overlaps (< 0.08m) with environment to prevent bullet self-destruction at muzzle
        if (hitDistance < 0.08f)
        {
            return true;
        }

        // Ignore generic triggers
        if (other.isTrigger)
        {
            return true;
        }

        return false;
    }

    private void ProcessHit(Collider other)
    {
        if (hasHit) return;
        hasHit = true;

        if (IsPlayer(other))
        {
            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>() 
                ?? other.GetComponent<PlayerHealth>() 
                ?? PlayerHealth.Instance 
                ?? Object.FindFirstObjectByType<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, attackerName);
            }
            NoiseSystem.Emit(transform.position, 8f, gameObject);
        }
        else
        {
            // Impact with environment - emits impact noise
            NoiseSystem.Emit(transform.position, 12f, gameObject);
        }

        Destroy(gameObject);
    }
}
