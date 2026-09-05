using UnityEngine;

public class AmmoSpawner : MonoBehaviour
{
    [Header("Ammo Spawner Settings")]
    [SerializeField] private GameObject ammoPrefab;
    [SerializeField] private float spawnInterval = 30f;
    [SerializeField] private Vector2 roomBoundsMin = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 roomBoundsMax = new Vector2(10f, 10f);

    private float timer = 0f;
    private GameObject currentAmmoObject;

    public void SetBounds(Vector2 min, Vector2 max)
    {
        roomBoundsMin = min;
        roomBoundsMax = max;
    }

    private void Start()
    {
        if (ammoPrefab == null)
        {
            ammoPrefab = Resources.Load<GameObject>("AmmoPickup");
        }
        timer = 0f;
        // Spawn one at start so player always has a pickup available
        TrySpawnAmmo();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.IsLevelOver) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnAmmo();
        }
    }

    public void TrySpawnAmmo()
    {
        // Only one Ammo Object should exist at a time
        if (currentAmmoObject != null)
        {
            return;
        }

        if (ammoPrefab == null)
        {
            ammoPrefab = Resources.Load<GameObject>("AmmoPickup");
            if (ammoPrefab == null)
            {
                Debug.LogWarning("AmmoSpawner: No ammoPrefab found!");
                return;
            }
        }

        float x = Random.Range(roomBoundsMin.x * 0.75f, roomBoundsMax.x * 0.75f);
        float z = Random.Range(roomBoundsMin.y * 0.75f, roomBoundsMax.y * 0.75f);
        Vector3 spawnPos = new Vector3(x, 1.2f, z);

        currentAmmoObject = Instantiate(ammoPrefab, spawnPos, Quaternion.Euler(0f, 0f, 90f));
    }
}
