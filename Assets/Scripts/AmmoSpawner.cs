using System.Collections.Generic;
using UnityEngine;

public class AmmoSpawner : MonoBehaviour
{
    [Header("Ammo Spawner Settings")]
    [SerializeField] private GameObject ammoPrefab;
    [Tooltip("Seconds between automatic ammo spawns. Lower values mean faster ammo respawn.")]
    [SerializeField] private float spawnInterval = 6f;
    [Tooltip("Maximum number of ammo pickups allowed concurrently on the map.")]
    [SerializeField] private int maxActiveAmmo = 4;
    [Tooltip("Number of ammo pickups to spawn immediately when the level starts.")]
    [SerializeField] private int initialSpawnCount = 2;
    [SerializeField] private Vector2 roomBoundsMin = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 roomBoundsMax = new Vector2(10f, 10f);

    private float timer = 0f;
    private readonly List<GameObject> activeAmmoList = new List<GameObject>();
    private readonly List<Transform> tacticalSpawnPoints = new List<Transform>();

    public void SetBounds(Vector2 min, Vector2 max)
    {
        roomBoundsMin = min;
        roomBoundsMax = max;
    }

    private void Awake()
    {
        // Enforce fast ammo spawn rate even if an old scene has an outdated serialized value (e.g. 30s)
        if (spawnInterval > 8f)
        {
            spawnInterval = 6f;
        }
        if (maxActiveAmmo < 3)
        {
            maxActiveAmmo = 4;
        }
    }

    private void Start()
    {
        if (ammoPrefab == null)
        {
            ammoPrefab = Resources.Load<GameObject>("AmmoPickup");
        }

        // Cache tactical spawn points if present in scene
        CacheTacticalPoints();

        timer = 0f;

        // Spawn initial batch so player immediately has accessible ammo
        for (int i = 0; i < initialSpawnCount; i++)
        {
            TrySpawnAmmo();
        }
    }

    private void CacheTacticalPoints()
    {
        tacticalSpawnPoints.Clear();
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj != null && obj.name.StartsWith("AmmoSpawnPoint"))
            {
                tacticalSpawnPoints.Add(obj.transform);
            }
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.IsLevelOver) return;

        // Clean up collected or destroyed ammo references
        activeAmmoList.RemoveAll(item => item == null);

        // If all ammo has been collected, accelerate spawn timer so player isn't starved
        if (activeAmmoList.Count == 0 && timer < spawnInterval - 1.5f)
        {
            timer = spawnInterval - 1.5f;
        }

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnAmmo();
        }
    }

    public void TrySpawnAmmo()
    {
        // Clean nulls
        activeAmmoList.RemoveAll(item => item == null);

        // Cap concurrent pickups on map
        if (activeAmmoList.Count >= maxActiveAmmo)
        {
            return;
        }

        if (ammoPrefab == null)
        {
            ammoPrefab = Resources.Load<GameObject>("AmmoPickup");
            if (ammoPrefab == null)
            {
                Debug.LogWarning("[AmmoSpawner] No AmmoPickup prefab found in Resources!");
                return;
            }
        }

        Vector3 spawnPos = DetermineSpawnPosition();

        GameObject ammoObj = Instantiate(ammoPrefab, spawnPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        activeAmmoList.Add(ammoObj);
    }

    private Vector3 DetermineSpawnPosition()
    {
        // 1. Try to pick an unoccupied tactical spawn point
        if (tacticalSpawnPoints.Count > 0)
        {
            // Shuffle/randomize inspection order
            List<Transform> candidatePoints = new List<Transform>(tacticalSpawnPoints);
            for (int i = candidatePoints.Count - 1; i > 0; i--)
            {
                int rnd = Random.Range(0, i + 1);
                var temp = candidatePoints[i];
                candidatePoints[i] = candidatePoints[rnd];
                candidatePoints[rnd] = temp;
            }

            foreach (var pt in candidatePoints)
            {
                if (pt == null) continue;
                bool isOccupied = false;
                foreach (var active in activeAmmoList)
                {
                    if (active != null && Vector3.Distance(active.transform.position, pt.position) < 3.5f)
                    {
                        isOccupied = true;
                        break;
                    }
                }

                if (!isOccupied)
                {
                    return pt.position;
                }
            }
        }

        // 2. Fallback: Raycast down to find open ground within room bounds
        for (int attempt = 0; attempt < 12; attempt++)
        {
            float rx = Random.Range(roomBoundsMin.x * 0.75f, roomBoundsMax.x * 0.75f);
            float rz = Random.Range(roomBoundsMin.y * 0.75f, roomBoundsMax.y * 0.75f);
            Vector3 rayStart = new Vector3(rx, 8f, rz);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 16f))
            {
                if (hit.collider.isTrigger) continue;
                if (hit.collider.CompareTag("Player")) continue;

                // Ensure distance from existing active ammo
                bool tooClose = false;
                foreach (var active in activeAmmoList)
                {
                    if (active != null && Vector3.Distance(active.transform.position, hit.point) < 4f)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    return hit.point + Vector3.up * 0.45f;
                }
            }
        }

        // 3. Simple default fallback
        float defX = Random.Range(roomBoundsMin.x * 0.5f, roomBoundsMax.x * 0.5f);
        float defZ = Random.Range(roomBoundsMin.y * 0.5f, roomBoundsMax.y * 0.5f);
        return new Vector3(defX, 0.45f, defZ);
    }
}
