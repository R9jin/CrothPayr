using System.Collections;
using UnityEngine;

public enum EnemyAIType
{
    LineOfSight,
    VisionCone,
    Proximity
}

public abstract class BaseEnemyAI : TrainingDummy
{
    [Header("Base AI Settings")]
    [SerializeField] protected EnemyAIType aiType = EnemyAIType.LineOfSight;
    [SerializeField] protected float moveSpeed = 4.5f;
    [SerializeField] protected float turnSpeed = 8f;
    [SerializeField] protected float attackCooldown = 1.4f;
    [SerializeField] protected float attackDamage = 15f;

    [Header("Roaming Bounds")]
    [SerializeField] protected Vector2 roomBoundsMin = new Vector2(-12f, -12f);
    [SerializeField] protected Vector2 roomBoundsMax = new Vector2(12f, 12f);

    protected Transform playerTransform;
    protected PlayerHealth playerHealth;
    protected Vector3 wanderTarget;
    protected bool isWandering = true;
    protected float wanderTimer = 0f;
    protected float nextAttackTime = 0f;
    protected float idleWaitTimer = 0f;

    public EnemyAIType AIType => aiType;

    public void SetBounds(Vector2 min, Vector2 max)
    {
        roomBoundsMin = min;
        roomBoundsMax = max;
    }

    protected override void Awake()
    {
        base.Awake();
        FindPlayer();
        PickNewWanderTarget();
    }

    protected virtual void Start()
    {
        FindPlayer();
    }

    protected void FindPlayer()
    {
        if (playerTransform != null && playerHealth != null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");

        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null) playerHealth = player.GetComponentInParent<PlayerHealth>();
        }
    }

    /// <summary>
    /// Computes the exact center of mass / chest of the player (adapts dynamically to crouching, jumping, and standing).
    /// </summary>
    protected Vector3 GetPlayerAimPosition()
    {
        if (playerTransform == null) return transform.position + transform.forward * 5f;

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null)
        {
            return cc.bounds.center;
        }

        Collider col = playerTransform.GetComponentInChildren<Collider>();
        if (col != null)
        {
            return col.bounds.center;
        }

        return playerTransform.position + Vector3.up * 0.5f;
    }

    protected virtual void Update()
    {
        if (isDead || Time.timeScale == 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.IsLevelOver) return;

        if (playerTransform == null)
        {
            FindPlayer();
        }

        UpdateAILogic();
    }

    protected abstract void UpdateAILogic();

    /// <summary>
    /// Checks if there is an unobstructed direct ray between AI eye position and target position.
    /// </summary>
    protected bool HasClearLineOfSight(Vector3 targetPos)
    {
        Vector3 eyePos = transform.position + Vector3.up * 0.81f;
        Vector3 dir = targetPos - eyePos;
        float dist = dir.magnitude;

        if (dist < 0.1f) return true;

        Ray ray = new Ray(eyePos, dir.normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, dist);

        // Sort hits by distance
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            if (hit.collider.transform.IsChildOf(transform) || hit.collider.transform == transform) continue;

            // If it hits player or player child, line of sight is clear!
            if (hit.collider.CompareTag("Player") || (playerTransform != null && hit.collider.transform.IsChildOf(playerTransform)))
            {
                return true;
            }

            // Hit a wall or obstacle
            return false;
        }

        return true;
    }

    /// <summary>
    /// Computes the exact solid surface beneath the AI and returns the correct center Y position
    /// (floor height + 0.60m to place cylinder feet flat on the ground). Never hits own colliders or other enemies.
    /// </summary>
    protected float GetGroundedY(Vector3 pos)
    {
        float startY = Mathf.Max(pos.y + 2.5f, 6.0f);
        Vector3 rayOrigin = new Vector3(pos.x, startY, pos.z);
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 15f);

        // Sort from highest surface to lowest
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            // CRITICAL: Ignore own collider and all child colliders so AI never hits itself!
            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)) continue;
            // CRITICAL: Ignore other AI enemies so enemies NEVER stack or float on top of each other!
            if (hit.collider.GetComponentInParent<TrainingDummy>() != null || hit.collider.GetComponentInParent<BaseEnemyAI>() != null) continue;
            // CRITICAL: Ignore player
            if (hit.collider.CompareTag("Player") || hit.collider.GetComponentInParent<PlayerHealth>() != null) continue;
            // Ignore projectiles
            if (hit.collider.GetComponentInParent<EnemyProjectile>() != null || hit.collider.name.Contains("Projectile") || hit.collider.name.Contains("Bullet")) continue;

            // Found solid floor, ramp, or platform surface (feet sit flat at hit.point.y)
            return hit.point.y + 0.60f;
        }

        // Default ground floor is at Y = 0.0, so center Y is 0.60f
        return 0.60f;
    }

    /// <summary>
    /// Roams towards current wanderTarget while avoiding walls.
    /// </summary>
    protected void RoamAround()
    {
        if (idleWaitTimer > 0f)
        {
            idleWaitTimer -= Time.deltaTime;
            // Ambient scan: smoothly scan left and right to look down corridors rather than freezing into a wall
            float scanOffset = Mathf.Sin(Time.time * 2f) * 20f * Time.deltaTime;
            transform.Rotate(Vector3.up, scanOffset);
            return;
        }

        wanderTimer += Time.deltaTime;

        Vector3 moveDir = (wanderTarget - transform.position);
        moveDir.y = 0f;
        float distToTarget = moveDir.magnitude;

        // If reached target or wandering too long, pick new target and wait briefly
        if (distToTarget < 1.0f || wanderTimer > 6f)
        {
            idleWaitTimer = Random.Range(1.0f, 2.5f);
            PickNewWanderTarget();
            return;
        }

        moveDir.Normalize();

        // Obstacle avoidance: SphereCast forward at chest level
        Vector3 waistPos = transform.position + Vector3.up * 0.1f;
        if (Physics.SphereCast(waistPos, 0.35f, transform.forward, out RaycastHit hit, 1.2f))
        {
            if (!hit.collider.isTrigger && hit.collider.transform != transform && !hit.collider.transform.IsChildOf(transform)
                && hit.collider.GetComponentInParent<TrainingDummy>() == null)
            {
                // Reflect movement direction off wall normal so the AI NEVER stares at walls!
                Vector3 awayDir = Vector3.Reflect(transform.forward, hit.normal);
                awayDir.y = 0f;
                if (awayDir.sqrMagnitude < 0.01f)
                {
                    awayDir = hit.normal;
                    awayDir.y = 0f;
                }
                awayDir.Normalize();

                // Immediately rotate AI into open space away from the wall
                transform.rotation = Quaternion.LookRotation(awayDir);

                // Set new target along open direction
                wanderTarget = transform.position + awayDir * Random.Range(4f, 8f);
                wanderTarget.x = Mathf.Clamp(wanderTarget.x, roomBoundsMin.x * 0.8f, roomBoundsMax.x * 0.8f);
                wanderTarget.z = Mathf.Clamp(wanderTarget.z, roomBoundsMin.y * 0.8f, roomBoundsMax.y * 0.8f);
                wanderTarget.y = GetGroundedY(wanderTarget);

                idleWaitTimer = Random.Range(0.4f, 1.0f);
                wanderTimer = 0f;
                return;
            }
        }

        // Rotate towards movement direction
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        // Move horizontally
        Vector3 newPos = transform.position + transform.forward * (moveSpeed * Time.deltaTime);
        newPos.x = Mathf.Clamp(newPos.x, roomBoundsMin.x * 0.85f, roomBoundsMax.x * 0.85f);
        newPos.z = Mathf.Clamp(newPos.z, roomBoundsMin.y * 0.85f, roomBoundsMax.y * 0.85f);

        // Ground / ramp tracking: smoothly move towards floor height (simulates gravity when dropping off ledge)
        float targetY = GetGroundedY(newPos);

        // Step height limit: prevent AI from climbing vertical walls/crates (max step height 0.5m)
        if (targetY > transform.position.y + 0.5f)
        {
            idleWaitTimer = Random.Range(0.5f, 1.2f);
            PickNewWanderTarget();
            return;
        }

        // Fast gravity when falling, smooth rise on ramps
        float vSpeed = (targetY < transform.position.y) ? 14f : 6f;
        newPos.y = Mathf.MoveTowards(transform.position.y, targetY, vSpeed * Time.deltaTime);

        transform.position = newPos;
    }

    protected void PickNewWanderTarget()
    {
        wanderTimer = 0f;
        // Test open candidate corridors so AI never aims into a wall
        for (int i = 0; i < 8; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(4f, 10f);
            Vector3 testPos = transform.position + new Vector3(Mathf.Sin(angle) * dist, 0f, Mathf.Cos(angle) * dist);
            testPos.x = Mathf.Clamp(testPos.x, roomBoundsMin.x * 0.8f, roomBoundsMax.x * 0.8f);
            testPos.z = Mathf.Clamp(testPos.z, roomBoundsMin.y * 0.8f, roomBoundsMax.y * 0.8f);

            Vector3 dir = (testPos - transform.position);
            dir.y = 0f;
            float checkDist = dir.magnitude;
            if (checkDist < 1.2f) continue;

            // Check if there is an open sightline to this candidate target
            Vector3 checkOrigin = transform.position + Vector3.up * 0.2f;
            if (!Physics.Raycast(checkOrigin, dir.normalized, checkDist * 0.85f))
            {
                float targetY = GetGroundedY(testPos);
                if (Mathf.Abs(targetY - transform.position.y) < 1.2f)
                {
                    wanderTarget = new Vector3(testPos.x, targetY, testPos.z);
                    return;
                }
            }
        }

        // Fallback: pick a point towards the center of the arena
        Vector3 centerDir = (Vector3.zero - transform.position);
        centerDir.y = 0f;
        Vector3 fallback = transform.position + centerDir.normalized * 5f;
        fallback.x = Mathf.Clamp(fallback.x, roomBoundsMin.x * 0.75f, roomBoundsMax.x * 0.75f);
        fallback.z = Mathf.Clamp(fallback.z, roomBoundsMin.y * 0.75f, roomBoundsMax.y * 0.75f);
        wanderTarget = new Vector3(fallback.x, GetGroundedY(fallback), fallback.z);
    }

    protected void RotateTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }
}
