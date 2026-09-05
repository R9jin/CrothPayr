using UnityEngine;

public class TrainingDummySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject dummyPrefab;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int initialSpawnCount = 3;
    [SerializeField] private Vector2 roomBoundsMin = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 roomBoundsMax = new Vector2(10f, 10f);

    private float timer = 0f;

    public void SetBounds(Vector2 min, Vector2 max)
    {
        roomBoundsMin = min;
        roomBoundsMax = max;
    }

    private void Start()
    {
        if (dummyPrefab == null)
        {
            dummyPrefab = Resources.Load<GameObject>("TrainingDummy");
        }

        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnDummy();
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.IsLevelOver) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnDummy();
        }
    }

    public void SpawnDummy()
    {
        if (dummyPrefab == null)
        {
            dummyPrefab = Resources.Load<GameObject>("TrainingDummy");
            if (dummyPrefab == null)
            {
                Debug.LogWarning("TrainingDummySpawner: No dummyPrefab found!");
                return;
            }
        }

        float x = Random.Range(roomBoundsMin.x * 0.85f, roomBoundsMax.x * 0.85f);
        float z = Random.Range(roomBoundsMin.y * 0.85f, roomBoundsMax.y * 0.85f);
        Vector3 spawnPos = new Vector3(x, 1f, z);

        Instantiate(dummyPrefab, spawnPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
    }
}
