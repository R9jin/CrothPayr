using System.Collections;
using UnityEngine;

public class ProximityAI : BaseEnemyAI
{
    [Header("Proximity Sound Settings")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;

    private LineRenderer proximityCircleRenderer;
    private bool isAlertedBySound = false;
    private Vector3 soundTargetPosition;
    private GameObject soundSourceObject;
    private float soundAlertTimer = 0f;
    private int burstShotsRemaining = 0;

    protected override void Awake()
    {
        base.Awake();
        aiType = EnemyAIType.Proximity;
        moveSpeed = 4.5f; // Normal speed
        turnSpeed = 10f;
        attackCooldown = 0.9f;
        attackDamage = 18f;

        if (projectilePrefab == null)
            projectilePrefab = Resources.Load<GameObject>("EnemyProjectile");

        CreateProximityRingVisual();
    }

    protected override void Start()
    {
        base.Start();
        SetTitle("<color=#BB66FF>[PROXIMITY] ACOUSTIC</color>", new Color(0.75f, 0.4f, 1f));
        SetFillColor(new Color(0.7f, 0.35f, 0.95f));
        ApplyDistinctVisuals();

        NoiseSystem.OnNoiseEmitted += HandleNoiseHeard;
    }

    private void OnDestroy()
    {
        NoiseSystem.OnNoiseEmitted -= HandleNoiseHeard;
    }

    private void CreateProximityRingVisual()
    {
        GameObject ringObj = new GameObject("ProximitySensorRing");
        ringObj.transform.SetParent(transform, false);
        proximityCircleRenderer = ringObj.AddComponent<LineRenderer>();
        proximityCircleRenderer.useWorldSpace = false;
        proximityCircleRenderer.loop = true;
        proximityCircleRenderer.startWidth = 0.05f;
        proximityCircleRenderer.endWidth = 0.05f;
        proximityCircleRenderer.material = new Material(Shader.Find("Sprites/Default"));
        proximityCircleRenderer.startColor = new Color(0.7f, 0.3f, 1f, 0.35f);
        proximityCircleRenderer.endColor = new Color(0.7f, 0.3f, 1f, 0.35f);

        int segments = 32;
        proximityCircleRenderer.positionCount = segments;
        Vector3[] points = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float rad = (i * 360f / segments) * Mathf.Deg2Rad;
            points[i] = new Vector3(Mathf.Sin(rad) * 4.5f, 0.15f, Mathf.Cos(rad) * 4.5f);
        }
        proximityCircleRenderer.SetPositions(points);
    }

    private void ApplyDistinctVisuals()
    {
        // Tint renderers with purple / cyan acoustic sensor finish
        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.color = new Color(0.6f, 0.3f, 0.85f);
                }
            }
        }
    }

    private void HandleNoiseHeard(NoiseEvent evt)
    {
        if (isDead) return;

        // Ignore sounds created by this AI or its projectiles
        if (evt.source == gameObject || (evt.source != null && evt.source.transform.IsChildOf(transform)))
        {
            return;
        }

        float dist = Vector3.Distance(transform.position, evt.position);

        // Check if sound occurred within proximity/detection area
        if (dist <= (detectionRadius + evt.radius))
        {
            // "Will shot any objects around its proximity/detection area that creates sounds."
            isAlertedBySound = true;
            soundTargetPosition = evt.position;
            soundSourceObject = evt.source;
            soundAlertTimer = 3.5f;
            burstShotsRemaining = 3; // Fires a burst at the sound source
        }
    }

    protected override void UpdateAILogic()
    {
        // "Never sees the player." - Vision detection is completely skipped.

        if (isAlertedBySound && soundAlertTimer > 0f)
        {
            soundAlertTimer -= Time.deltaTime;
            EngageSoundSource();
        }
        else
        {
            isAlertedBySound = false;
            RoamAround();
        }
    }

    private void EngageSoundSource()
    {
        Vector3 targetPos = soundTargetPosition;
        // If the sound source object is still alive, aim at its current position
        if (soundSourceObject != null)
        {
            targetPos = soundSourceObject.transform.position + Vector3.up * 1.0f;
        }

        RotateTowards(targetPos);

        // Fire ranged shots at sound target
        if (burstShotsRemaining > 0 && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            burstShotsRemaining--;
            FireAcousticRangedShot(targetPos);
        }
    }

    private void FireAcousticRangedShot(Vector3 targetPos)
    {
        Vector3 origin = shootPoint != null ? shootPoint.position : transform.position + Vector3.up * 1.4f;
        Vector3 shootDir = (targetPos - origin).normalized;

        GameObject projObj;
        if (projectilePrefab != null)
        {
            projObj = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(shootDir));
        }
        else
        {
            projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projObj.name = "Proximity_Bullet";
            projObj.transform.position = origin;
            projObj.transform.rotation = Quaternion.LookRotation(shootDir);
            projObj.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

            EnemyProjectile ep = projObj.AddComponent<EnemyProjectile>();
            ep.Init("Proximity Acoustic AI", gameObject, new Color(0.75f, 0.2f, 1f), attackDamage, 24f);
            return;
        }

        EnemyProjectile proj = projObj.GetComponent<EnemyProjectile>();
        if (proj != null)
        {
            proj.Init("Proximity Acoustic AI", gameObject, new Color(0.75f, 0.2f, 1f), attackDamage, 24f);
        }
    }

    protected override void Die()
    {
        NoiseSystem.OnNoiseEmitted -= HandleNoiseHeard;
        base.Die();
    }
}
