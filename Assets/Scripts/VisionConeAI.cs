using UnityEngine;

public class VisionConeAI : BaseEnemyAI
{
    [Header("Vision Cone Settings")]
    [SerializeField] private float visionRange = 18f;
    [SerializeField] private float visionAngle = 70f; // total angle in degrees
    [SerializeField] private float meleeRange = 1.9f;
    [SerializeField] private float fastRoamSpeed = 6.2f;
    [SerializeField] private float chaseSpeed = 8.0f; // Faster than Player (6f) and other AIs (4.5f)

    private LineRenderer coneRenderer;
    private bool isChasing = false;
    private Vector3 lastKnownPlayerPos;
    private float searchTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        aiType = EnemyAIType.VisionCone;
        moveSpeed = fastRoamSpeed;
        turnSpeed = 11f;
        attackCooldown = 0.85f;
        attackDamage = 22f;

        CreateVisionConeVisual();
    }

    protected override void Start()
    {
        base.Start();
        SetTitle("<color=#FFAA00>[VISION] RUSHER</color>", new Color(1f, 0.7f, 0.1f));
        SetFillColor(new Color(1f, 0.65f, 0.1f));
        ApplyDistinctVisuals();
    }

    private void CreateVisionConeVisual()
    {
        GameObject coneObj = new GameObject("VisionConeVisual");
        coneObj.transform.SetParent(transform, false);
        coneRenderer = coneObj.AddComponent<LineRenderer>();
        coneRenderer.useWorldSpace = false;
        coneRenderer.startWidth = 0.05f;
        coneRenderer.endWidth = 0.05f;
        coneRenderer.material = new Material(Shader.Find("Sprites/Default"));
        coneRenderer.startColor = new Color(1f, 0.8f, 0.1f, 0.45f);
        coneRenderer.endColor = new Color(1f, 0.5f, 0f, 0.15f);

        // Draw cone lines in local space
        int segments = 16;
        coneRenderer.positionCount = segments + 2;
        float halfAngle = visionAngle * 0.5f;

        Vector3[] points = new Vector3[segments + 2];
        points[0] = new Vector3(0f, 0.2f, 0f); // Origin at feet

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -halfAngle + (visionAngle / segments) * i;
            float rad = currentAngle * Mathf.Deg2Rad;
            points[i + 1] = new Vector3(Mathf.Sin(rad) * visionRange * 0.7f, 0.2f, Mathf.Cos(rad) * visionRange * 0.7f);
        }

        coneRenderer.SetPositions(points);
    }

    private void ApplyDistinctVisuals()
    {
        // Tint renderers with amber / orange rusher armor
        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                if (r != null && r.material != null)
                {
                    r.material.color = new Color(0.95f, 0.65f, 0.2f);
                }
            }
        }
    }

    protected override void UpdateAILogic()
    {
        bool seesPlayer = CheckVisionCone();

        if (seesPlayer)
        {
            isChasing = true;
            searchTimer = 3.0f;
            lastKnownPlayerPos = playerTransform.position;
            ChaseAndAttackPlayer();
        }
        else if (isChasing && searchTimer > 0f)
        {
            searchTimer -= Time.deltaTime;
            InvestigateLastKnownPosition();
        }
        else
        {
            isChasing = false;
            moveSpeed = fastRoamSpeed;
            RoamAround();
        }
    }

    /// <summary>
    /// Checks if player is within vision angle, distance, and not blocked by obstacles.
    /// </summary>
    private bool CheckVisionCone()
    {
        if (playerTransform == null) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 playerPos = playerTransform.position + Vector3.up * 1.0f;
        Vector3 dirToPlayer = playerPos - eyePos;
        float distToPlayer = dirToPlayer.magnitude;

        if (distToPlayer > visionRange) return false;

        // Angle check: must be inside field of view
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer.normalized);
        if (angleToPlayer > visionAngle * 0.5f)
        {
            return false; // Player is outside vision cone (e.g. behind or to side of AI)
        }

        // Raycast check: ensure line of sight is not occluded by walls
        return HasClearLineOfSight(playerPos);
    }

    private void ChaseAndAttackPlayer()
    {
        moveSpeed = chaseSpeed; // Moves Faster than the Player (8.0f vs 6.0f)
        Vector3 playerPos = playerTransform.position;
        float dist = Vector3.Distance(transform.position, playerPos);

        RotateTowards(playerPos);

        if (dist > meleeRange)
        {
            // Sprint directly at the player
            Vector3 moveDir = (playerPos - transform.position);
            moveDir.y = 0f;
            moveDir.Normalize();

            Vector3 newPos = transform.position + moveDir * (moveSpeed * Time.deltaTime);
            newPos.x = Mathf.Clamp(newPos.x, roomBoundsMin.x * 0.85f, roomBoundsMax.x * 0.85f);
            newPos.z = Mathf.Clamp(newPos.z, roomBoundsMin.y * 0.85f, roomBoundsMax.y * 0.85f);
            float targetY = GetGroundedY(newPos);
            newPos.y = Mathf.MoveTowards(transform.position.y, targetY, 12f * Time.deltaTime);
            transform.position = newPos;
        }
        else
        {
            // In Melee Range: execute melee attack
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                ExecuteMeleeAttack();
            }
        }
    }

    private void ExecuteMeleeAttack()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage, "Vision Cone Rusher AI");
        }

        // Visual flash on melee hit
        StartCoroutine(MeleeStrikeEffect());
    }

    private System.Collections.IEnumerator MeleeStrikeEffect()
    {
        // Brief scale punch to animate the strike
        Vector3 origScale = transform.localScale;
        transform.localScale = origScale * 1.15f;
        yield return new WaitForSeconds(0.12f);
        transform.localScale = origScale;
    }

    private void InvestigateLastKnownPosition()
    {
        moveSpeed = chaseSpeed * 0.8f;
        RotateTowards(lastKnownPlayerPos);

        Vector3 moveDir = (lastKnownPlayerPos - transform.position);
        moveDir.y = 0f;
        float dist = moveDir.magnitude;

        if (dist > 1.0f)
        {
            moveDir.Normalize();
            Vector3 newPos = transform.position + moveDir * (moveSpeed * Time.deltaTime);
            newPos.x = Mathf.Clamp(newPos.x, roomBoundsMin.x * 0.85f, roomBoundsMax.x * 0.85f);
            newPos.z = Mathf.Clamp(newPos.z, roomBoundsMin.y * 0.85f, roomBoundsMax.y * 0.85f);
            float targetY = GetGroundedY(newPos);
            newPos.y = Mathf.MoveTowards(transform.position.y, targetY, 10f * Time.deltaTime);
            transform.position = newPos;
        }
    }
}
