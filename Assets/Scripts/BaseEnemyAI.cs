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
        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
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
    /// Roams towards current wanderTarget while avoiding walls.
    /// </summary>
    /// <summary>
    /// Computes the exact solid surface beneath the AI and returns the correct center Y position
    /// (floor height + 1.0m half-height of the 2m capsule). Never hits own colliders or other enemies.
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
            // Ignore projectiles
            if (hit.collider.CompareTag("Projectile")) continue;

            // Found solid floor, ramp, or platform surface
            return hit.point.y + 1.0f;
        }

        // Default ground floor is at Y = 0.0, so center Y is 1.0f
        return 1.0f;
    }

    /// <summary>
    /// Roams towards current wanderTarget while avoiding walls.
    /// </summary>
    protected void RoamAround()
    {
        if (idleWaitTimer > 0f)
        {
            idleWaitTimer -= Time.deltaTime;
            return;
        }

        wanderTimer += Time.deltaTime;

        Vector3 moveDir = (wanderTarget - transform.position);
        moveDir.y = 0f;
        float distToTarget = moveDir.magnitude;

        // If reached target or wandering too long, pick new target and wait briefly
        if (distToTarget < 1.2f || wanderTimer > 6f)
        {
            idleWaitTimer = Random.Range(1.0f, 2.5f);
            PickNewWanderTarget();
            return;
        }

        moveDir.Normalize();

        // Obstacle avoidance: SphereCast forward at waist level
        Vector3 waistPos = transform.position + Vector3.up * 0.2f;
        if (Physics.SphereCast(waistPos, 0.4f, transform.forward, out RaycastHit hit, 1.4f))
        {
            if (!hit.collider.isTrigger && hit.collider.transform != transform && !hit.collider.transform.IsChildOf(transform))
            {
                // Obstacle ahead: pause and choose new direction
                idleWaitTimer = Random.Range(0.6f, 1.5f);
                PickNewWanderTarget();
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
        newPos.y = Mathf.MoveTowards(transform.position.y, targetY, 8f * Time.deltaTime);

        transform.position = newPos;
    }

    protected void PickNewWanderTarget()
    {
        wanderTimer = 0f;
        float rx = Random.Range(roomBoundsMin.x * 0.75f, roomBoundsMax.x * 0.75f);
        float rz = Random.Range(roomBoundsMin.y * 0.75f, roomBoundsMax.y * 0.75f);
        float targetY = GetGroundedY(new Vector3(rx, transform.position.y, rz));
        wanderTarget = new Vector3(rx, targetY, rz);
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
