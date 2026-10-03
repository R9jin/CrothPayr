using UnityEngine;

public class LineOfSightAI : BaseEnemyAI
{
    [Header("Line of Sight Settings")]
    [SerializeField] private float maxSightRange = 35f;
    [SerializeField] private float preferredCombatDistance = 9f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;

    private LineRenderer laserLine;
    private bool seesPlayer = false;

    protected override void Awake()
    {
        base.Awake();
        aiType = EnemyAIType.LineOfSight;
        moveSpeed = 4.5f; // Normal speed
        turnSpeed = 8f;
        attackCooldown = 1.3f;
        attackDamage = 16f;

        if (projectilePrefab == null)
            projectilePrefab = Resources.Load<GameObject>("EnemyProjectile");

        CreateLaserSight();
    }

    protected override void Start()
    {
        base.Start();
        SetTitle("<color=#FF4444>[LoS] SNIPER</color>", new Color(1f, 0.3f, 0.3f));
        SetFillColor(new Color(0.95f, 0.2f, 0.2f));
        ApplyDistinctVisuals();
    }

    private void CreateLaserSight()
    {
        GameObject laserObj = new GameObject("LoSLaserSight");
        laserObj.transform.SetParent(transform, false);
        laserLine = laserObj.AddComponent<LineRenderer>();
        laserLine.startWidth = 0.04f;
        laserLine.endWidth = 0.04f;
        laserLine.material = new Material(Shader.Find("Sprites/Default"));
        laserLine.startColor = new Color(1f, 0f, 0f, 0.7f);
        laserLine.endColor = new Color(1f, 0.2f, 0.2f, 0.1f);
        laserLine.enabled = false;
    }

    private void ApplyDistinctVisuals()
    {
        // Tint model renderers with red accent
        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.color = new Color(0.85f, 0.35f, 0.35f);
                }
            }
        }
    }

    protected override void UpdateAILogic()
    {
        if (playerTransform == null)
        {
            seesPlayer = false;
            if (laserLine != null) laserLine.enabled = false;
            RoamAround();
            return;
        }

        Vector3 playerPos = GetPlayerAimPosition();
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // "Automatically sees the player when there is a clear view between them."
        bool clearView = distToPlayer <= maxSightRange && HasClearLineOfSight(playerPos);

        if (clearView)
        {
            seesPlayer = true;
            EngagePlayer(playerPos, distToPlayer);
        }
        else
        {
            seesPlayer = false;
            if (laserLine != null) laserLine.enabled = false;
            RoamAround();
        }
    }

    private void EngagePlayer(Vector3 playerTargetPos, float distToPlayer)
    {
        RotateTowards(playerTargetPos);

        // Movement with Normal Speed
        if (distToPlayer > preferredCombatDistance)
        {
            // Close in at normal speed
            Vector3 moveDir = (playerTargetPos - transform.position);
            moveDir.y = 0f;
            moveDir.Normalize();

            Vector3 newPos = transform.position + moveDir * (moveSpeed * Time.deltaTime);
            newPos.x = Mathf.Clamp(newPos.x, roomBoundsMin.x * 0.85f, roomBoundsMax.x * 0.85f);
            newPos.z = Mathf.Clamp(newPos.z, roomBoundsMin.y * 0.85f, roomBoundsMax.y * 0.85f);
            float targetY = GetGroundedY(newPos);
            if (targetY <= transform.position.y + 0.5f)
            {
                float vSpeed = (targetY < transform.position.y) ? 14f : 6f;
                newPos.y = Mathf.MoveTowards(transform.position.y, targetY, vSpeed * Time.deltaTime);
                transform.position = newPos;
            }
        }

        // Draw laser sight to player
        Vector3 origin = shootPoint != null ? shootPoint.position : transform.position + Vector3.up * 0.81f;
        if (laserLine != null)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, origin);
            laserLine.SetPosition(1, playerTargetPos);
        }

        // Attack on sight with ranged shots
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            FireRangedShot(playerTargetPos);
        }
    }

    private void FireRangedShot(Vector3 targetPos)
    {
        Vector3 baseOrigin = shootPoint != null ? shootPoint.position : transform.position + Vector3.up * 0.81f;
        Vector3 shootDir = (targetPos - baseOrigin).normalized;
        Vector3 origin = baseOrigin + shootDir * 0.65f;

        GameObject projObj;
        if (projectilePrefab != null)
        {
            projObj = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(shootDir));
        }
        else
        {
            // Create projectile procedurally
            projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projObj.name = "LoS_Bullet";
            projObj.transform.position = origin;
            projObj.transform.rotation = Quaternion.LookRotation(shootDir);
            projObj.transform.localScale = new Vector3(0.28f, 0.28f, 0.45f);

            // Add EnemyProjectile component
            EnemyProjectile ep = projObj.AddComponent<EnemyProjectile>();
            ep.Init("Line of Sight AI", gameObject, Color.red, attackDamage, 28f);
            return;
        }

        EnemyProjectile proj = projObj.GetComponent<EnemyProjectile>();
        if (proj != null)
        {
            proj.Init("Line of Sight AI", gameObject, Color.red, attackDamage, 28f);
        }
    }

    protected override void Die()
    {
        if (laserLine != null) laserLine.enabled = false;
        base.Die();
    }
}
