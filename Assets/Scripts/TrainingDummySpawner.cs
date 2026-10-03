using System.Collections.Generic;
using UnityEngine;

public class TrainingDummySpawner : MonoBehaviour
{
    [Header("Spawner Configuration")]
    [SerializeField] private GameObject dummyPrefab; // Backwards compatibility with scenes
    [SerializeField] private GameObject baseEnemyPrefab;
    [SerializeField] private GameObject losAIPrefab;
    [SerializeField] private GameObject visionConeAIPrefab;
    [SerializeField] private GameObject proximityAIPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 4.5f;
    [SerializeField] private int initialSpawnCount = 3;
    [SerializeField] private int maxConcurrentEnemies = 5;
    [SerializeField] private Vector2 roomBoundsMin = new Vector2(-12f, -12f);
    [SerializeField] private Vector2 roomBoundsMax = new Vector2(12f, 12f);

    [Header("AI Type Distribution (0 to 1)")]
    [Range(0f, 1f)] [SerializeField] private float losRatio = 0.34f;
    [Range(0f, 1f)] [SerializeField] private float visionConeRatio = 0.33f;
    [Range(0f, 1f)] [SerializeField] private float proximityRatio = 0.33f;

    private float timer = 0f;
    private readonly List<GameObject> activeEnemies = new List<GameObject>();

    public void SetBounds(Vector2 min, Vector2 max)
    {
        roomBoundsMin = min;
        roomBoundsMax = max;
    }

    private void Start()
    {
        if (baseEnemyPrefab == null && dummyPrefab != null)
        {
            baseEnemyPrefab = dummyPrefab;
        }
        if (baseEnemyPrefab == null)
        {
            baseEnemyPrefab = Resources.Load<GameObject>("TrainingDummy");
        }
        if (losAIPrefab == null)
        {
            losAIPrefab = Resources.Load<GameObject>("Enemy_LoS");
        }
        if (visionConeAIPrefab == null)
        {
            visionConeAIPrefab = Resources.Load<GameObject>("Enemy_VisionCone");
        }
        if (proximityAIPrefab == null)
        {
            proximityAIPrefab = Resources.Load<GameObject>("Enemy_Proximity");
        }

        // Adjust AI distribution based on active level
        ConfigureDistributionForCurrentLevel();

        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnAI();
        }
    }

    private void ConfigureDistributionForCurrentLevel()
    {
        if (GameManager.Instance != null && GameManager.Instance.ActiveLevelData != null)
        {
            string lvlName = GameManager.Instance.ActiveLevelData.name.ToLower();
            if (lvlName.Contains("1"))
            {
                // Level 1: Introduce LoS and Vision Cone
                losRatio = 0.5f;
                visionConeRatio = 0.5f;
                proximityRatio = 0.0f;
            }
            else if (lvlName.Contains("2"))
            {
                // Level 2: Introduce Proximity and stealth
                losRatio = 0.35f;
                visionConeRatio = 0.35f;
                proximityRatio = 0.30f;
            }
            else
            {
                // Level 3+: Full chaotic challenge with all 3 AI types
                losRatio = 0.34f;
                visionConeRatio = 0.33f;
                proximityRatio = 0.33f;
            }
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.IsLevelOver) return;

        // Clean up dead/destroyed references
        activeEnemies.RemoveAll(item => item == null);

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            if (activeEnemies.Count < maxConcurrentEnemies)
            {
                SpawnAI();
            }
        }
    }

    public void SpawnAI()
    {
        // Pick AI type based on distribution
        EnemyAIType typeToSpawn = PickRandomAIType();

        Vector3 spawnPos;
        Quaternion spawnRot;

        // Check if tactical spawn points exist in scene under "SpawnPoints"
        Transform spawnPointsRoot = GameObject.Find("SpawnPoints")?.transform;
        List<Transform> validPoints = new List<Transform>();
        if (spawnPointsRoot != null)
        {
            foreach (Transform child in spawnPointsRoot)
            {
                if (child.gameObject.activeInHierarchy && !child.name.ToLower().Contains("player") && !child.name.ToLower().Contains("ammo"))
                {
                    validPoints.Add(child);
                }
            }
        }

        if (validPoints.Count > 0)
        {
            Transform chosenPoint = validPoints[Random.Range(0, validPoints.Count)];
            spawnPos = chosenPoint.position;
            spawnRot = chosenPoint.rotation;
        }
        else
        {
            // Calculate random spawn position inside room bounds, away from center player spawn
            float x = Random.Range(roomBoundsMin.x * 0.8f, roomBoundsMax.x * 0.8f);
            float z = Random.Range(roomBoundsMin.y * 0.8f, roomBoundsMax.y * 0.8f);
            spawnPos = new Vector3(x, 1f, z);
            spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        // Snap to floor if platform or ground is beneath
        if (Physics.Raycast(spawnPos + Vector3.up * 2f, Vector3.down, out RaycastHit groundHit, 6f))
        {
            spawnPos.y = groundHit.point.y + 0.1f;
        }

        GameObject enemyObj = null;

        // Try spawning specific prefab if assigned
        switch (typeToSpawn)
        {
            case EnemyAIType.LineOfSight:
                if (losAIPrefab != null) enemyObj = Instantiate(losAIPrefab, spawnPos, spawnRot);
                break;
            case EnemyAIType.VisionCone:
                if (visionConeAIPrefab != null) enemyObj = Instantiate(visionConeAIPrefab, spawnPos, spawnRot);
                break;
            case EnemyAIType.Proximity:
                if (proximityAIPrefab != null) enemyObj = Instantiate(proximityAIPrefab, spawnPos, spawnRot);
                break;
        }

        // If no specific prefab was instantiated, create from base dummy prefab
        if (enemyObj == null)
        {
            if (baseEnemyPrefab == null)
            {
                baseEnemyPrefab = Resources.Load<GameObject>("TrainingDummy");
            }

            if (baseEnemyPrefab != null)
            {
                enemyObj = Instantiate(baseEnemyPrefab, spawnPos, spawnRot);

                // Remove base dummy script if present
                TrainingDummy dummyComp = enemyObj.GetComponent<TrainingDummy>();
                if (dummyComp != null)
                {
                    DestroyImmediate(dummyComp);
                }

                // Attach the specialized AI script
                switch (typeToSpawn)
                {
                    case EnemyAIType.LineOfSight:
                        enemyObj.AddComponent<LineOfSightAI>();
                        break;
                    case EnemyAIType.VisionCone:
                        enemyObj.AddComponent<VisionConeAI>();
                        break;
                    case EnemyAIType.Proximity:
                        enemyObj.AddComponent<ProximityAI>();
                        break;
                }
            }
        }

        if (enemyObj != null)
        {
            BaseEnemyAI aiComp = enemyObj.GetComponent<BaseEnemyAI>();
            if (aiComp == null)
            {
                TrainingDummy oldDummy = enemyObj.GetComponent<TrainingDummy>();
                if (oldDummy != null && !(oldDummy is BaseEnemyAI))
                {
                    DestroyImmediate(oldDummy);
                }

                switch (typeToSpawn)
                {
                    case EnemyAIType.LineOfSight:
                        aiComp = enemyObj.AddComponent<LineOfSightAI>();
                        break;
                    case EnemyAIType.VisionCone:
                        aiComp = enemyObj.AddComponent<VisionConeAI>();
                        break;
                    case EnemyAIType.Proximity:
                        aiComp = enemyObj.AddComponent<ProximityAI>();
                        break;
                }
            }

            if (aiComp != null)
            {
                aiComp.SetBounds(roomBoundsMin, roomBoundsMax);
            }

            activeEnemies.Add(enemyObj);
        }
    }

    private EnemyAIType PickRandomAIType()
    {
        float total = losRatio + visionConeRatio + proximityRatio;
        if (total <= 0.001f) return EnemyAIType.LineOfSight;

        float rnd = Random.Range(0f, total);
        if (rnd < losRatio)
        {
            return EnemyAIType.LineOfSight;
        }
        else if (rnd < losRatio + visionConeRatio)
        {
            return EnemyAIType.VisionCone;
        }
        else
        {
            return EnemyAIType.Proximity;
        }
    }
}
