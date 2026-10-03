using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ValorantLevelBuilder
{
    private const string BUILD_KEY = "ValorantLevelsBuilt_v3_URPTexturesFixed";

    static ValorantLevelBuilder()
    {
        CheckAndAutoBuild();
    }

    private static void CheckAndAutoBuild()
    {
        if (!EditorPrefs.GetBool(BUILD_KEY, false))
        {
            BuildAllLevels();
            EditorPrefs.SetBool(BUILD_KEY, true);
        }
    }

    [MenuItem("Tools/Build Valorant Level Layouts")]
    public static void BuildAllLevels()
    {
        Debug.Log("=================================================");
        Debug.Log(">>> STARTING VALORANT TACTICAL LEVEL GENERATION <<<");
        Debug.Log("=================================================");

        string currentScene = EditorSceneManager.GetActiveScene().path;

        try
        {
            BuildLevel1();
            BuildLevel2();
            BuildLevel3();

            Debug.Log("=================================================");
            Debug.Log(">>> ALL 3 VALORANT STAGES BUILT SUCCESSFULLY! <<<");
            Debug.Log("=================================================");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ERROR] Failed to build Valorant levels: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            if (!string.IsNullOrEmpty(currentScene))
            {
                EditorSceneManager.OpenScene(currentScene);
            }
            AssetDatabase.SaveAssets();
        }
    }

    #region Level 1 - Site A (Ascent Armory)
    public static void BuildLevel1()
    {
        string scenePath = "Assets/Scenes/Level1.unity";
        Debug.Log($"[ValorantLevelBuilder] Building Level 1: {scenePath}...");
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var mats = LoadTacticalMaterials();
        float roomSize = 32f;
        float halfSize = roomSize * 0.5f;

        Transform arenaTr = SetupArenaBase(roomSize, mats);
        CleanOldProps();

        // Create Tactical Layout Root
        GameObject layoutObj = new GameObject("TacticalLayout");
        layoutObj.transform.SetParent(arenaTr, false);

        // --- 1. A-Main Chokepoint & Pillars (Funnel entry from South spawn) ---
        // West entry pillar
        CreateBox("ChokeWall_West", new Vector3(-8f, 2.5f, -7f), new Vector3(8f, 5f, 1.5f), mats.slateWall, layoutObj.transform);
        // East entry pillar
        CreateBox("ChokeWall_East", new Vector3(8f, 2.5f, -7f), new Vector3(8f, 5f, 1.5f), mats.slateWall, layoutObj.transform);
        // Center choke gap is 8m (X: -4 to 4, Z: -7)

        // West Corner Cubby Wall (Slice the pie corner)
        CreateBox("CubbyWall_West", new Vector3(-12f, 2.5f, -10f), new Vector3(1.5f, 5f, 5f), mats.slateWall, layoutObj.transform);

        // Attacker Spawn Barrier (Low cover at spawn)
        CreateBox("AttackerSpawnCover", new Vector3(0f, 0.6f, -14.5f), new Vector3(6f, 1.2f, 0.8f), mats.orangeCrate, layoutObj.transform);

        // --- 2. Central "A-Site" Plant Zone & Radianite Crate Clusters ---
        // Default Plant: Double-stack Radianite Box with stepped peek
        CreateBox("Radianite_A_Main_Stack", new Vector3(0f, 1.2f, 2f), new Vector3(1.4f, 2.4f, 1.4f), mats.orangeCrate, layoutObj.transform);
        CreateBox("Radianite_A_Peek_Crate", new Vector3(1.4f, 0.6f, 2f), new Vector3(1.2f, 1.2f, 1.2f), mats.orangeCrate, layoutObj.transform);
        CreateBox("Radianite_A_Wing_Crate", new Vector3(-1.4f, 0.6f, 2f), new Vector3(1.2f, 1.2f, 1.2f), mats.orangeCrate, layoutObj.transform);

        // Site Half-Wall (Crouch to hide, stand to headshot peek!)
        CreateBox("Site_HalfWall_West", new Vector3(-5.5f, 0.575f, 3.5f), new Vector3(3.5f, 1.15f, 0.6f), mats.slateWall, layoutObj.transform);
        CreateBox("Site_HalfWall_East", new Vector3(5.5f, 0.575f, 1f), new Vector3(3.5f, 1.15f, 0.6f), mats.slateWall, layoutObj.transform);

        // --- 3. A-Long Sightline (East flank for sniper/LoS duels) ---
        CreateBox("Long_DividingWall", new Vector3(9f, 2.5f, 3f), new Vector3(0.8f, 5f, 10f), mats.slateWall, layoutObj.transform);
        CreateBox("Long_PeekBox_1", new Vector3(12f, 0.575f, -2f), new Vector3(2.5f, 1.15f, 0.8f), mats.orangeCrate, layoutObj.transform);
        CreateBox("Long_PeekBox_2", new Vector3(13.5f, 0.6f, 5f), new Vector3(1.2f, 1.2f, 1.2f), mats.orangeCrate, layoutObj.transform);

        // --- 4. A-Heaven: Elevated Balcony (Y = 2.4m) Overlooking Site ---
        CreatePlatform("A_Heaven_Platform", new Vector3(0f, 2.4f, 11.5f), new Vector3(18f, 0.35f, 5.5f), mats.bluePlatform, layoutObj.transform);
        // Support pillars under Heaven
        CreateBox("Heaven_Pillar_NW", new Vector3(-7f, 1.2f, 13f), new Vector3(0.8f, 2.4f, 0.8f), mats.slateWall, layoutObj.transform);
        CreateBox("Heaven_Pillar_NE", new Vector3(7f, 1.2f, 13f), new Vector3(0.8f, 2.4f, 0.8f), mats.slateWall, layoutObj.transform);
        CreateBox("Heaven_Pillar_SW", new Vector3(-7f, 1.2f, 9.5f), new Vector3(0.8f, 2.4f, 0.8f), mats.slateWall, layoutObj.transform);
        CreateBox("Heaven_Pillar_SE", new Vector3(7f, 1.2f, 9.5f), new Vector3(0.8f, 2.4f, 0.8f), mats.slateWall, layoutObj.transform);

        // Heaven Parapets / Headshot Peek Railings (Y = 2.4 to 3.4m)
        CreateBox("Heaven_Parapet_West", new Vector3(-5.5f, 2.925f, 8.85f), new Vector3(6f, 1.05f, 0.3f), mats.slateWall, layoutObj.transform);
        CreateBox("Heaven_Parapet_East", new Vector3(5.5f, 2.925f, 8.85f), new Vector3(6f, 1.05f, 0.3f), mats.slateWall, layoutObj.transform);
        // Note: Center gap (X: -2.5 to 2.5) provides clear sightline down to site!

        // --- 5. A-Heaven Access Ramp (Smooth walkable slope, ~20 degrees) ---
        CreateRamp("Heaven_Ramp", new Vector3(-12f, 0f, 2f), new Vector3(-12f, 2.4f, 8.8f), 2.5f, mats.bluePlatform, layoutObj.transform);

        // --- 6. West Flank / "Tree" Connector ---
        CreateBox("Tree_DividingWall", new Vector3(-8f, 2.5f, 1f), new Vector3(0.8f, 5f, 8f), mats.slateWall, layoutObj.transform);
        CreateBox("Tree_CornerCrate", new Vector3(-13.5f, 0.6f, -1f), new Vector3(1.2f, 1.2f, 1.2f), mats.orangeCrate, layoutObj.transform);

        // --- 7. Tactical Spawn Points ---
        SetupSpawnPoints(arenaTr,
            playerPos: new Vector3(0f, 1.1f, -13f),
            playerRot: Quaternion.Euler(0f, 0f, 0f),
            enemyPoints: new[]
            {
                new Vector3(0f, 1.1f, 4f),      // Site default
                new Vector3(0f, 3.4f, 11.5f),   // Heaven sniper
                new Vector3(12.5f, 1.1f, 7f),   // Long sightline
                new Vector3(-11f, 1.1f, -1f),   // West connector / tree
                new Vector3(0f, 1.1f, 11.5f),   // Under Heaven (Hell)
            },
            ammoPoints: new[]
            {
                new Vector3(-4f, 1.2f, -3f),
                new Vector3(4f, 1.2f, 5f)
            }
        );

        UpdateSpawnersInScene(new Vector2(-14f, -14f), new Vector2(14f, 14f));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ValorantLevelBuilder] Level 1 successfully built and saved!");
    }
    #endregion

    #region Level 2 - B-Site & Mid Courtyard (Bind / Haven Style)
    public static void BuildLevel2()
    {
        string scenePath = "Assets/Scenes/Level2.unity";
        Debug.Log($"[ValorantLevelBuilder] Building Level 2: {scenePath}...");
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var mats = LoadTacticalMaterials();
        float roomSize = 42f;
        float halfSize = roomSize * 0.5f;

        Transform arenaTr = SetupArenaBase(roomSize, mats);
        CleanOldProps();

        GameObject layoutObj = new GameObject("TacticalLayout");
        layoutObj.transform.SetParent(arenaTr, false);

        // --- 1. Attacker Spawn & Entry Chokes (South) ---
        CreateBox("AttackerCover_West", new Vector3(-4f, 0.6f, -16f), new Vector3(3f, 1.2f, 0.8f), mats.orangeCrate, layoutObj.transform);
        CreateBox("AttackerCover_East", new Vector3(4f, 0.6f, -16f), new Vector3(3f, 1.2f, 0.8f), mats.orangeCrate, layoutObj.transform);

        // --- 2. Central Mid Courtyard & Generator Platform ---
        // Elevated Generator Deck (Y = 2.0m, size 8m x 7m)
        CreatePlatform("Mid_Generator_Platform", new Vector3(0f, 2.0f, 0f), new Vector3(8f, 0.35f, 7f), mats.bluePlatform, layoutObj.transform);
        // Central Tech Generator unit on top
        CreateBox("Mid_Generator_Core", new Vector3(0f, 2.9f, 0f), new Vector3(2.6f, 1.5f, 2.6f), mats.orangeCrate, layoutObj.transform);
        // Support legs
        CreateBox("GenPillar_SW", new Vector3(-3.2f, 1.0f, -2.8f), new Vector3(0.8f, 2f, 0.8f), mats.slateWall, layoutObj.transform);
        CreateBox("GenPillar_SE", new Vector3(3.2f, 1.0f, -2.8f), new Vector3(0.8f, 2f, 0.8f), mats.slateWall, layoutObj.transform);
        CreateBox("GenPillar_NW", new Vector3(-3.2f, 1.0f, 2.8f), new Vector3(0.8f, 2f, 0.8f), mats.slateWall, layoutObj.transform);
        CreateBox("GenPillar_NE", new Vector3(3.2f, 1.0f, 2.8f), new Vector3(0.8f, 2f, 0.8f), mats.slateWall, layoutObj.transform);

        // Mid Opposing Walkable Ramps
        CreateRamp("Mid_South_Ramp", new Vector3(0f, 0f, -7.5f), new Vector3(0f, 2.0f, -3.5f), 2.6f, mats.bluePlatform, layoutObj.transform);
        CreateRamp("Mid_North_Ramp", new Vector3(0f, 0f, 7.5f), new Vector3(0f, 2.0f, 3.5f), 2.6f, mats.bluePlatform, layoutObj.transform);

        // Mid Half-Wall Cover
        CreateBox("Mid_HalfWall_West", new Vector3(-6.5f, 0.575f, -1f), new Vector3(3.5f, 1.15f, 0.6f), mats.slateWall, layoutObj.transform);
        CreateBox("Mid_HalfWall_East", new Vector3(6.5f, 0.575f, 1f), new Vector3(3.5f, 1.15f, 0.6f), mats.slateWall, layoutObj.transform);

        // --- 3. B-Site Defense Zone (North-West Sector) ---
        // Dividing wall isolating B-site with tactical entry choke
        CreateBox("BSite_Wall_South", new Vector3(-13.5f, 2.5f, 5f), new Vector3(9f, 5f, 1f), mats.slateWall, layoutObj.transform);
        CreateBox("BSite_Wall_East", new Vector3(-7f, 2.5f, 11f), new Vector3(1f, 5f, 11f), mats.slateWall, layoutObj.transform);

        // B-Site Radianite Pyramids & Stacks
        CreateBox("BSite_Radianite_Stack", new Vector3(-13f, 1.2f, 11f), new Vector3(1.4f, 2.4f, 1.4f), mats.orangeCrate, layoutObj.transform);
        CreateBox("BSite_Radianite_Step", new Vector3(-11.6f, 0.6f, 11f), new Vector3(1.2f, 1.2f, 1.2f), mats.orangeCrate, layoutObj.transform);
        CreateBox("BSite_HalfWall", new Vector3(-13f, 0.575f, 8f), new Vector3(3f, 1.15f, 0.5f), mats.orangeCrate, layoutObj.transform);

        // Elevated "Hookah / Window" Platform (North-West, Y = 2.2m)
        CreatePlatform("Hookah_Platform", new Vector3(-15.5f, 2.2f, 17f), new Vector3(7f, 0.35f, 5f), mats.bluePlatform, layoutObj.transform);
        CreateRamp("Hookah_Ramp", new Vector3(-15.5f, 0f, 11.5f), new Vector3(-15.5f, 2.2f, 14.5f), 2.2f, mats.bluePlatform, layoutObj.transform);
        CreateBox("Hookah_Parapet", new Vector3(-15.5f, 2.75f, 14.6f), new Vector3(4f, 0.8f, 0.25f), mats.slateWall, layoutObj.transform);

        // --- 4. East Flank: "Elbow & Connector" (Tight 90-degree corners for Vision Cone & Stealth) ---
        CreateBox("East_Choke_Wall_1", new Vector3(12f, 2.5f, -6f), new Vector3(1f, 5f, 12f), mats.slateWall, layoutObj.transform);
        CreateBox("East_Choke_Wall_2", new Vector3(15f, 2.5f, 6f), new Vector3(7f, 5f, 1f), mats.slateWall, layoutObj.transform);

        // 90-degree Corner Cubby (slice angle)
        CreateBox("East_Cubby_Box", new Vector3(18f, 0.6f, -10f), new Vector3(1.2f, 1.2f, 1.2f), mats.orangeCrate, layoutObj.transform);
        CreateBox("East_Alley_Peek", new Vector3(16f, 0.575f, 11f), new Vector3(2.5f, 1.15f, 0.6f), mats.orangeCrate, layoutObj.transform);

        // --- 5. Spawn Points ---
        SetupSpawnPoints(arenaTr,
            playerPos: new Vector3(0f, 1.1f, -18f),
            playerRot: Quaternion.Euler(0f, 0f, 0f),
            enemyPoints: new[]
            {
                new Vector3(0f, 3.0f, 0f),      // Mid high-ground generator
                new Vector3(-13f, 1.1f, 11f),   // B-Site default
                new Vector3(-15.5f, 3.2f, 17f), // Hookah elevated window
                new Vector3(16f, 1.1f, 2f),     // East elbow choke
                new Vector3(-14f, 1.1f, -3f),   // B-Main flank
                new Vector3(0f, 1.1f, 14f),     // Mid back lane
            },
            ammoPoints: new[]
            {
                new Vector3(0f, 1.2f, -10f),
                new Vector3(-10f, 1.2f, 14f),
                new Vector3(14f, 1.2f, 12f)
            }
        );

        UpdateSpawnersInScene(new Vector2(-18f, -18f), new Vector2(18f, 18f));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ValorantLevelBuilder] Level 2 successfully built and saved!");
    }
    #endregion

    #region Level 3 - Tech Vault & Dual Catwalk Arena (Split Metropolis)
    public static void BuildLevel3()
    {
        string scenePath = "Assets/Scenes/Level3.unity";
        Debug.Log($"[ValorantLevelBuilder] Building Level 3: {scenePath}...");
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var mats = LoadTacticalMaterials();
        float roomSize = 52f;
        float halfSize = roomSize * 0.5f;

        Transform arenaTr = SetupArenaBase(roomSize, mats);
        CleanOldProps();

        GameObject layoutObj = new GameObject("TacticalLayout");
        layoutObj.transform.SetParent(arenaTr, false);

        // --- 1. Attacker Spawn Gateway (South) ---
        CreateBox("SpawnPillar_Left", new Vector3(-5f, 2.5f, -20f), new Vector3(2f, 5f, 1.5f), mats.slateWall, layoutObj.transform);
        CreateBox("SpawnPillar_Right", new Vector3(5f, 2.5f, -20f), new Vector3(2f, 5f, 1.5f), mats.slateWall, layoutObj.transform);
        CreateBox("SpawnCover_Center", new Vector3(0f, 0.6f, -19.5f), new Vector3(4f, 1.2f, 0.8f), mats.orangeCrate, layoutObj.transform);

        // --- 2. Dual Elevated Catwalks: "Heaven West" & "Heaven East" (Y = 2.6m) ---
        // West Catwalk Deck (5m wide x 22m long)
        CreatePlatform("West_Heaven_Catwalk", new Vector3(-18f, 2.6f, 1f), new Vector3(5f, 0.35f, 22f), mats.bluePlatform, layoutObj.transform);
        CreateRamp("West_Heaven_Ramp", new Vector3(-18f, 0f, -16f), new Vector3(-18f, 2.6f, -10f), 2.6f, mats.bluePlatform, layoutObj.transform);
        // West Catwalk Parapet (Inner edge facing center)
        CreateBox("West_Parapet_Front", new Vector3(-15.8f, 3.125f, 1f), new Vector3(0.3f, 1.05f, 16f), mats.slateWall, layoutObj.transform);

        // East Catwalk Deck (5m wide x 22m long)
        CreatePlatform("East_Heaven_Catwalk", new Vector3(18f, 2.6f, 1f), new Vector3(5f, 0.35f, 22f), mats.bluePlatform, layoutObj.transform);
        CreateRamp("East_Heaven_Ramp", new Vector3(18f, 0f, -16f), new Vector3(18f, 2.6f, -10f), 2.6f, mats.bluePlatform, layoutObj.transform);
        // East Catwalk Parapet
        CreateBox("East_Parapet_Front", new Vector3(15.8f, 3.125f, 1f), new Vector3(0.3f, 1.05f, 16f), mats.slateWall, layoutObj.transform);

        // North Connecting High-Ground Bridge (Y = 2.8m, connects East and West Catwalks!)
        CreatePlatform("North_Skybridge", new Vector3(0f, 2.8f, 15f), new Vector3(36f, 0.4f, 5f), mats.bluePlatform, layoutObj.transform);
        CreateBox("BridgePillar_1", new Vector3(-8f, 1.4f, 15f), new Vector3(1.2f, 2.8f, 1.2f), mats.slateWall, layoutObj.transform);
        CreateBox("BridgePillar_2", new Vector3(8f, 1.4f, 15f), new Vector3(1.2f, 2.8f, 1.2f), mats.slateWall, layoutObj.transform);
        CreateBox("Bridge_Parapet_Left", new Vector3(-8f, 3.325f, 12.7f), new Vector3(12f, 1.05f, 0.3f), mats.slateWall, layoutObj.transform);
        CreateBox("Bridge_Parapet_Right", new Vector3(8f, 3.325f, 12.7f), new Vector3(12f, 1.05f, 0.3f), mats.slateWall, layoutObj.transform);

        // --- 3. Central "Tech Vault" Complex (4 Bastions & Cross Corridors) ---
        // 4 Large Corner Bastions (3m x 3m x 4.5m high)
        CreateBox("VaultBastion_SW", new Vector3(-6f, 2.25f, -5f), new Vector3(3.2f, 4.5f, 3.2f), mats.slateWall, layoutObj.transform);
        CreateBox("VaultBastion_SE", new Vector3(6f, 2.25f, -5f), new Vector3(3.2f, 4.5f, 3.2f), mats.slateWall, layoutObj.transform);
        CreateBox("VaultBastion_NW", new Vector3(-6f, 2.25f, 5f), new Vector3(3.2f, 4.5f, 3.2f), mats.slateWall, layoutObj.transform);
        CreateBox("VaultBastion_NE", new Vector3(6f, 2.25f, 5f), new Vector3(3.2f, 4.5f, 3.2f), mats.slateWall, layoutObj.transform);

        // Central Reactor Column inside Vault
        CreateBox("Vault_ReactorCore", new Vector3(0f, 1.75f, 0f), new Vector3(2.5f, 3.5f, 2.5f), mats.blueSolid, layoutObj.transform);

        // Radianite Stacks around Vault perimeter
        CreateBox("Vault_South_Peek", new Vector3(0f, 0.575f, -6.8f), new Vector3(3.5f, 1.15f, 0.6f), mats.orangeCrate, layoutObj.transform);
        CreateBox("Vault_North_Peek", new Vector3(0f, 0.575f, 6.8f), new Vector3(3.5f, 1.15f, 0.6f), mats.orangeCrate, layoutObj.transform);
        CreateBox("Vault_West_Stack", new Vector3(-9.5f, 1.2f, 0f), new Vector3(1.4f, 2.4f, 1.4f), mats.orangeCrate, layoutObj.transform);
        CreateBox("Vault_West_Peek", new Vector3(-9.5f, 0.6f, 1.4f), new Vector3(1.2f, 1.2f, 1.2f), mats.orangeCrate, layoutObj.transform);
        CreateBox("Vault_East_Stack", new Vector3(9.5f, 1.2f, 0f), new Vector3(1.4f, 2.4f, 1.4f), mats.orangeCrate, layoutObj.transform);
        CreateBox("Vault_East_Peek", new Vector3(9.5f, 0.6f, -1.4f), new Vector3(1.2f, 1.2f, 1.2f), mats.orangeCrate, layoutObj.transform);

        // --- 4. Courtyard Flanks & Underpass ---
        CreateBox("WestAlley_PeekBox", new Vector3(-12f, 0.575f, -12f), new Vector3(2.5f, 1.15f, 0.6f), mats.orangeCrate, layoutObj.transform);
        CreateBox("EastAlley_PeekBox", new Vector3(12f, 0.575f, -12f), new Vector3(2.5f, 1.15f, 0.6f), mats.orangeCrate, layoutObj.transform);
        CreateBox("Underpass_Cover", new Vector3(0f, 0.6f, 18f), new Vector3(3f, 1.2f, 1.2f), mats.orangeCrate, layoutObj.transform);

        // --- 5. Spawn Points ---
        SetupSpawnPoints(arenaTr,
            playerPos: new Vector3(0f, 1.1f, -23f),
            playerRot: Quaternion.Euler(0f, 0f, 0f),
            enemyPoints: new[]
            {
                new Vector3(0f, 1.1f, 0f),       // Center Vault Core
                new Vector3(-18f, 3.6f, 2f),     // West Heaven Catwalk
                new Vector3(18f, 3.6f, 2f),      // East Heaven Catwalk
                new Vector3(0f, 3.8f, 15f),      // North Connecting Bridge
                new Vector3(0f, 1.1f, 15f),      // Underpass / Sewer tunnel
                new Vector3(-11f, 1.1f, -10f),   // West flank alley
                new Vector3(11f, 1.1f, -10f),    // East flank alley
            },
            ammoPoints: new[]
            {
                new Vector3(0f, 1.2f, -14f),
                new Vector3(-18f, 3.8f, 7f),
                new Vector3(18f, 3.8f, 7f),
                new Vector3(0f, 1.2f, 10f)
            }
        );

        UpdateSpawnersInScene(new Vector2(-23f, -23f), new Vector2(23f, 23f));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ValorantLevelBuilder] Level 3 successfully built and saved!");
    }
    #endregion

    #region Geometry Construction Helpers
    private struct TacticalMats
    {
        public Material whiteGrid;
        public Material slateWall;
        public Material bluePlatform;
        public Material orangeCrate;
        public Material blueSolid;
    }

    private static TacticalMats LoadTacticalMaterials()
    {
        TacticalMats tm = new TacticalMats();

        tm.whiteGrid = AssetDatabase.LoadAssetAtPath<Material>("Assets/Survivalist/StarterAssets/Environment/Art/Materials/GridWhite_01_Mat.mat");
        tm.slateWall = AssetDatabase.LoadAssetAtPath<Material>("Assets/Survivalist/StarterAssets/Environment/Art/Materials/GreyBlue_Mat.mat");
        tm.bluePlatform = AssetDatabase.LoadAssetAtPath<Material>("Assets/Survivalist/StarterAssets/Environment/Art/Materials/GridBlue_01_Mat.mat");
        tm.orangeCrate = AssetDatabase.LoadAssetAtPath<Material>("Assets/Survivalist/StarterAssets/Environment/Art/Materials/GridOrange_01_Mat.mat");
        tm.blueSolid = AssetDatabase.LoadAssetAtPath<Material>("Assets/Survivalist/StarterAssets/Environment/Art/Materials/Blue_Mat.mat");

        // Fallbacks
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (tm.whiteGrid == null) { tm.whiteGrid = new Material(litShader) { color = new Color(0.85f, 0.85f, 0.85f) }; }
        if (tm.slateWall == null) { tm.slateWall = new Material(litShader) { color = new Color(0.25f, 0.3f, 0.38f) }; }
        if (tm.bluePlatform == null) { tm.bluePlatform = new Material(litShader) { color = new Color(0.2f, 0.45f, 0.75f) }; }
        if (tm.orangeCrate == null) { tm.orangeCrate = new Material(litShader) { color = new Color(0.95f, 0.45f, 0.1f) }; }
        if (tm.blueSolid == null) { tm.blueSolid = new Material(litShader) { color = new Color(0.1f, 0.5f, 0.9f) }; }

        return tm;
    }

    private static Transform SetupArenaBase(float roomSize, TacticalMats mats)
    {
        float half = roomSize * 0.5f;

        GameObject arenaObj = GameObject.Find("Arena");
        if (arenaObj == null) arenaObj = new GameObject("Arena");

        // Find or create Ground
        Transform groundTr = arenaObj.transform.Find("Ground");
        if (groundTr == null)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = "Ground";
            g.transform.SetParent(arenaObj.transform, false);
            groundTr = g.transform;
        }
        groundTr.localPosition = new Vector3(0f, -0.5f, 0f);
        groundTr.localScale = new Vector3(roomSize, 1f, roomSize);
        groundTr.localRotation = Quaternion.identity;

        MeshRenderer groundMr = groundTr.GetComponent<MeshRenderer>();
        if (groundMr != null) groundMr.sharedMaterial = mats.whiteGrid;

        BoxCollider groundBc = groundTr.GetComponent<BoxCollider>();
        if (groundBc == null) groundBc = groundTr.gameObject.AddComponent<BoxCollider>();
        groundBc.size = Vector3.one;

        // Find or create Walls
        Transform wallsTr = arenaObj.transform.Find("Walls");
        if (wallsTr == null)
        {
            GameObject w = new GameObject("Walls");
            w.transform.SetParent(arenaObj.transform, false);
            wallsTr = w.transform;
        }

        SetupPerimeterWall(wallsTr, "Wall_North", new Vector3(0f, 3f, half), new Vector3(roomSize, 6f, 1f), mats.slateWall);
        SetupPerimeterWall(wallsTr, "Wall_South", new Vector3(0f, 3f, -half), new Vector3(roomSize, 6f, 1f), mats.slateWall);
        SetupPerimeterWall(wallsTr, "Wall_East", new Vector3(half, 3f, 0f), new Vector3(1f, 6f, roomSize), mats.slateWall);
        SetupPerimeterWall(wallsTr, "Wall_West", new Vector3(-half, 3f, 0f), new Vector3(1f, 6f, roomSize), mats.slateWall);

        return arenaObj.transform;
    }

    private static void SetupPerimeterWall(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        Transform wTr = parent.Find(name);
        if (wTr == null)
        {
            GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.name = name;
            w.transform.SetParent(parent, false);
            wTr = w.transform;
        }
        wTr.localPosition = pos;
        wTr.localScale = scale;
        wTr.localRotation = Quaternion.identity;

        MeshRenderer mr = wTr.GetComponent<MeshRenderer>();
        if (mr != null) mr.sharedMaterial = mat;

        BoxCollider bc = wTr.GetComponent<BoxCollider>();
        if (bc == null) bc = wTr.gameObject.AddComponent<BoxCollider>();
        bc.size = Vector3.one;
    }

    private static void CleanOldProps()
    {
        GameObject props = GameObject.Find("Props");
        if (props != null)
        {
            Undo.DestroyObjectImmediate(props);
        }

        // Clean any existing TacticalLayout under Arena
        GameObject arena = GameObject.Find("Arena");
        if (arena != null)
        {
            Transform oldLayout = arena.transform.Find("TacticalLayout");
            if (oldLayout != null)
            {
                Undo.DestroyObjectImmediate(oldLayout.gameObject);
            }
        }
    }

    private static GameObject CreateBox(string name, Vector3 pos, Vector3 size, Material mat, Transform parent)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = pos;
        box.transform.localScale = size;
        box.transform.localRotation = Quaternion.identity;

        MeshRenderer mr = box.GetComponent<MeshRenderer>();
        if (mr != null) mr.sharedMaterial = mat;

        BoxCollider bc = box.GetComponent<BoxCollider>();
        if (bc == null) bc = box.AddComponent<BoxCollider>();
        bc.size = Vector3.one;

        return box;
    }

    private static GameObject CreatePlatform(string name, Vector3 pos, Vector3 size, Material mat, Transform parent)
    {
        return CreateBox(name, pos, size, mat, parent);
    }

    private static GameObject CreateRamp(string name, Vector3 startPos, Vector3 endPos, float width, Material mat, Transform parent)
    {
        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = name;
        ramp.transform.SetParent(parent, false);

        Vector3 midPos = (startPos + endPos) * 0.5f;
        Vector3 dir = endPos - startPos;
        float runDistance = new Vector2(dir.x, dir.z).magnitude;
        float heightDiff = dir.y;
        float slopeLength = dir.magnitude;

        // Position at midpoint
        ramp.transform.localPosition = midPos;

        // Thickness 0.25f, width, slopeLength
        ramp.transform.localScale = new Vector3(width, 0.25f, slopeLength);

        // Rotation: look along horizontal direction, tilted by pitch
        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float pitch = -Mathf.Atan2(heightDiff, runDistance) * Mathf.Rad2Deg;
        ramp.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);

        MeshRenderer mr = ramp.GetComponent<MeshRenderer>();
        if (mr != null) mr.sharedMaterial = mat;

        BoxCollider bc = ramp.GetComponent<BoxCollider>();
        if (bc == null) bc = ramp.AddComponent<BoxCollider>();
        bc.size = Vector3.one;

        return ramp;
    }

    private static void SetupSpawnPoints(Transform arenaTr, Vector3 playerPos, Quaternion playerRot, Vector3[] enemyPoints, Vector3[] ammoPoints)
    {
        Transform spawnPointsTr = arenaTr.Find("SpawnPoints");
        if (spawnPointsTr == null)
        {
            GameObject sp = new GameObject("SpawnPoints");
            sp.transform.SetParent(arenaTr, false);
            spawnPointsTr = sp.transform;
        }

        // Clean old spawn points
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform c in spawnPointsTr) toDestroy.Add(c.gameObject);
        foreach (var go in toDestroy) Undo.DestroyObjectImmediate(go);

        // 1. Tactical Player Spawn Point
        GameObject playerSpawn = new GameObject("PlayerSpawn");
        playerSpawn.transform.SetParent(spawnPointsTr, false);
        playerSpawn.transform.position = playerPos;
        playerSpawn.transform.rotation = playerRot;

        // 2. Tactical Enemy Spawn Points
        for (int i = 0; i < enemyPoints.Length; i++)
        {
            GameObject esp = new GameObject($"EnemySpawnPoint_{i + 1}");
            esp.transform.SetParent(spawnPointsTr, false);
            esp.transform.position = enemyPoints[i];
            esp.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        // 3. Ammo Spawn Points
        for (int i = 0; i < ammoPoints.Length; i++)
        {
            GameObject asp = new GameObject($"AmmoSpawnPoint_{i + 1}");
            asp.transform.SetParent(spawnPointsTr, false);
            asp.transform.position = ammoPoints[i];
        }

        // Move Player in scene to playerSpawn position
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = playerPos;
            player.transform.rotation = playerRot;
            if (cc != null) cc.enabled = true;

            // Remove obsolete prototype capsule MeshRenderer and MeshFilter from root Player
            MeshRenderer mr = player.GetComponent<MeshRenderer>();
            if (mr != null) Undo.DestroyObjectImmediate(mr);
            MeshFilter mf = player.GetComponent<MeshFilter>();
            if (mf != null) Undo.DestroyObjectImmediate(mf);
        }
    }

    private static void UpdateSpawnersInScene(Vector2 boundsMin, Vector2 boundsMax)
    {
        TrainingDummySpawner spawner = Object.FindFirstObjectByType<TrainingDummySpawner>();
        if (spawner != null)
        {
            spawner.SetBounds(boundsMin, boundsMax);
            EditorUtility.SetDirty(spawner);
        }

        AmmoSpawner ammoSpawner = Object.FindFirstObjectByType<AmmoSpawner>();
        if (ammoSpawner != null)
        {
            ammoSpawner.SetBounds(boundsMin, boundsMax);
            EditorUtility.SetDirty(ammoSpawner);
        }
    }
    #endregion
}
