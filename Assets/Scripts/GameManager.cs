using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Configuration")]
    [SerializeField] private LevelData levelData;

    [Header("UI Reference")]
    [SerializeField] private UIManager uiManager;

    private int currentAmmo = 30;
    private int maxAmmo = 30;
    private float timeRemaining = 90f;
    private int requiredDummies = 5;
    private int dummiesKilled = 0;
    private bool isLevelOver = false;

    // Statistics
    private int ammoPickedUp = 0;
    private int shotsFired = 0;

    public LevelData ActiveLevelData => levelData;
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;
    public float TimeRemaining => timeRemaining;
    public bool IsLevelOver => isLevelOver;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (levelData != null)
        {
            currentAmmo = levelData.startingAmmo;
            maxAmmo = levelData.maxAmmo;
            timeRemaining = levelData.timerDuration;
            requiredDummies = levelData.requiredDummies;
        }
        else
        {
            currentAmmo = 30;
            maxAmmo = 30;
            timeRemaining = 90f;
            requiredDummies = 5;
        }

        dummiesKilled = 0;
        ammoPickedUp = 0;
        shotsFired = 0;
        isLevelOver = false;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (uiManager == null)
        {
            uiManager = FindAnyObjectByType<UIManager>();
        }

        if (uiManager != null)
        {
            uiManager.UpdateAmmoText(currentAmmo, maxAmmo);
            uiManager.UpdateTimerText(timeRemaining);
            uiManager.UpdateDummyProgress(dummiesKilled, requiredDummies);
        }

        SetupSpawners();
        RandomizePlayerSpawn();
    }

    private void SetupSpawners()
    {
        Vector2 bMin = levelData != null ? levelData.roomBoundsMin : new Vector2(-14f, -14f);
        Vector2 bMax = levelData != null ? levelData.roomBoundsMax : new Vector2(14f, 14f);

        TrainingDummySpawner dummySpawner = GetComponent<TrainingDummySpawner>();
        if (dummySpawner == null) dummySpawner = gameObject.AddComponent<TrainingDummySpawner>();
        dummySpawner.SetBounds(bMin, bMax);

        AmmoSpawner ammoSpawner = GetComponent<AmmoSpawner>();
        if (ammoSpawner == null) ammoSpawner = gameObject.AddComponent<AmmoSpawner>();
        ammoSpawner.SetBounds(bMin, bMax);
    }

    private void RandomizePlayerSpawn()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        Vector2 bMin = levelData != null ? levelData.roomBoundsMin : new Vector2(-10f, -10f);
        Vector2 bMax = levelData != null ? levelData.roomBoundsMax : new Vector2(10f, 10f);

        // Keep player safe from wall collisions (inside 55% of room bounds)
        float rx = Random.Range(bMin.x * 0.55f, bMax.x * 0.55f);
        float rz = Random.Range(bMin.y * 0.55f, bMax.y * 0.55f);

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = new Vector3(rx, 1.1f, rz);
        player.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Physics.SyncTransforms();
        if (cc != null) cc.enabled = true;
    }

    private void Update()
    {
        if (isLevelOver) return;

        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                LoseLevel("Time Out! Failed to eliminate all required training dummies.");
            }

            if (uiManager != null)
            {
                uiManager.UpdateTimerText(timeRemaining);
            }
        }
    }

    public bool HasAmmo()
    {
        return currentAmmo > 0;
    }

    public void RecordShotFired()
    {
        if (isLevelOver) return;

        currentAmmo = Mathf.Max(0, currentAmmo - 1);
        shotsFired++;

        if (uiManager != null)
        {
            uiManager.UpdateAmmoText(currentAmmo, maxAmmo);
        }
    }

    public void CollectAmmo(int amount)
    {
        if (isLevelOver) return;

        int added = Mathf.Min(amount, maxAmmo - currentAmmo);
        currentAmmo = Mathf.Min(maxAmmo, currentAmmo + amount);
        ammoPickedUp += amount;

        if (uiManager != null)
        {
            uiManager.UpdateAmmoText(currentAmmo, maxAmmo);
        }
    }

    public void RecordDummyKilled()
    {
        if (isLevelOver) return;

        dummiesKilled++;

        if (uiManager != null)
        {
            uiManager.UpdateDummyProgress(dummiesKilled, requiredDummies);
        }

        if (dummiesKilled >= requiredDummies)
        {
            WinLevel();
        }
    }

    private void WinLevel()
    {
        isLevelOver = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (uiManager != null)
        {
            uiManager.ShowVictoryWindow(ammoPickedUp, shotsFired, dummiesKilled);
        }
    }

    private void LoseLevel(string reason)
    {
        isLevelOver = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (uiManager != null)
        {
            uiManager.ShowDefeatWindow(reason);
        }
    }

    public void LoadNextStage()
    {
        Time.timeScale = 1f;
        if (levelData != null && !string.IsNullOrEmpty(levelData.nextSceneName))
        {
            SceneManager.LoadScene(levelData.nextSceneName);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
