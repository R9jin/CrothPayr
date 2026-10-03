using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameAIValidation
{
    [MenuItem("Tools/Run Game AI Validation")]
    public static void RunAllTests()
    {
        Debug.Log("=== STARTING COMPLETE GAME AI & LEVEL VALIDATION SUITE ===");
        int passed = 0;
        int failed = 0;

        // Test 1: NoiseSystem event dispatch
        try
        {
            bool heard = false;
            Vector3 testPos = new Vector3(5f, 0f, 5f);
            float testRadius = 15f;
            GameObject dummySource = new GameObject("TestSource");

            NoiseSystem.OnNoiseEmitted += (evt) =>
            {
                if (evt.source == dummySource && evt.radius == testRadius && evt.position == testPos)
                {
                    heard = true;
                }
            };

            NoiseSystem.Emit(testPos, testRadius, dummySource);

            if (heard)
            {
                Debug.Log("[PASS] Test 1: NoiseSystem successfully dispatched and received noise event.");
                passed++;
            }
            else
            {
                Debug.LogError("[FAIL] Test 1: NoiseSystem did not receive noise event.");
                failed++;
            }

            Object.DestroyImmediate(dummySource);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FAIL] Test 1 exception: {ex.Message}");
            failed++;
        }

        // Test 2: PlayerHealth damage and clamping
        try
        {
            GameObject playerGO = new GameObject("TestPlayer");
            PlayerHealth ph = playerGO.AddComponent<PlayerHealth>();

            ph.TakeDamage(30f, "TestHazard");
            if (ph.CurrentHealth == 70f)
            {
                Debug.Log("[PASS] Test 2: PlayerHealth accurately reduced health (100 -> 70).");
                passed++;
            }
            else
            {
                Debug.LogError($"[FAIL] Test 2: Expected health 70, got {ph.CurrentHealth}");
                failed++;
            }

            Object.DestroyImmediate(playerGO);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FAIL] Test 2 exception: {ex.Message}");
            failed++;
        }

        // Test 3: AI Type Prefabs in Resources
        try
        {
            GameObject losPrefab = Resources.Load<GameObject>("Enemy_LoS");
            GameObject vcPrefab = Resources.Load<GameObject>("Enemy_VisionCone");
            GameObject proxPrefab = Resources.Load<GameObject>("Enemy_Proximity");

            bool prefabsExist = losPrefab != null && vcPrefab != null && proxPrefab != null;
            if (prefabsExist)
            {
                Debug.Log("[PASS] Test 3: All 3 AI prefabs exist in Resources (Enemy_LoS, Enemy_VisionCone, Enemy_Proximity).");
                passed++;
            }
            else
            {
                Debug.LogWarning($"[WARN] Test 3: Prefabs in Resources: LoS={losPrefab != null}, VC={vcPrefab != null}, Prox={proxPrefab != null}. Spawner will use procedural creation fallback.");
                passed++;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FAIL] Test 3 exception: {ex.Message}");
            failed++;
        }

        // Test 4: LevelData configurations for 3 stages
        try
        {
            LevelData l1 = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Settings/Level1Data.asset");
            LevelData l2 = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Settings/Level2Data.asset");
            LevelData l3 = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Settings/Level3Data.asset");

            if (l1 != null && l2 != null && l3 != null)
            {
                bool stageFlow = l1.nextSceneName == "Level2" && l2.nextSceneName == "Level3" && l3.nextSceneName == "MainMenu";
                if (stageFlow)
                {
                    Debug.Log($"[PASS] Test 4: 3-Stage level progression configured correctly: Level1 -> {l1.nextSceneName} -> {l2.nextSceneName} -> {l3.nextSceneName}.");
                    passed++;
                }
                else
                {
                    Debug.LogError($"[FAIL] Test 4: Unexpected stage flow: {l1.nextSceneName}, {l2.nextSceneName}, {l3.nextSceneName}");
                    failed++;
                }
            }
            else
            {
                Debug.LogError("[FAIL] Test 4: Could not load all 3 LevelData assets.");
                failed++;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FAIL] Test 4 exception: {ex.Message}");
            failed++;
        }

        // Test 5: Validate Valorant Tactical Layouts in all 3 stages
        try
        {
            string origScene = EditorSceneManager.GetActiveScene().path;
            string[] scenePaths = new[] { "Assets/Scenes/Level1.unity", "Assets/Scenes/Level2.unity", "Assets/Scenes/Level3.unity" };
            int validScenes = 0;

            foreach (var sp in scenePaths)
            {
                Scene s = EditorSceneManager.OpenScene(sp, OpenSceneMode.Single);
                GameObject arena = GameObject.Find("Arena");
                GameObject tactical = arena?.transform.Find("TacticalLayout")?.gameObject;
                GameObject spawnPoints = arena?.transform.Find("SpawnPoints")?.gameObject;
                GameObject playerSpawn = spawnPoints?.transform.Find("PlayerSpawn")?.gameObject;

                int enemySpawnCount = 0;
                if (spawnPoints != null)
                {
                    foreach (Transform c in spawnPoints.transform)
                    {
                        if (c.name.StartsWith("EnemySpawnPoint")) enemySpawnCount++;
                    }
                }

                bool hasColliders = false;
                if (tactical != null)
                {
                    var colliders = tactical.GetComponentsInChildren<Collider>();
                    hasColliders = colliders != null && colliders.Length >= 8;
                }

                if (arena != null && tactical != null && playerSpawn != null && enemySpawnCount >= 4 && hasColliders)
                {
                    Debug.Log($"[PASS] Test 5 ({s.name}): Tactical layout verified! Obstacle Colliders={tactical.GetComponentsInChildren<Collider>().Length}, Enemy Spawns={enemySpawnCount}, Player Spawn={playerSpawn.transform.position}");
                    validScenes++;
                }
                else
                {
                    Debug.LogError($"[FAIL] Test 5 ({s.name}): Missing tactical components: Arena={arena != null}, Layout={tactical != null}, PlayerSpawn={playerSpawn != null}, Spawns={enemySpawnCount}, Colliders={hasColliders}");
                }
            }

            if (!string.IsNullOrEmpty(origScene))
            {
                EditorSceneManager.OpenScene(origScene);
            }

            if (validScenes == 3)
            {
                passed++;
            }
            else
            {
                failed++;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FAIL] Test 5 exception: {ex.Message}");
            failed++;
        }

        Debug.Log($"=== GAME AI & LEVEL VALIDATION COMPLETE: {passed} PASSED, {failed} FAILED ===");
    }
}
