using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject crosshairUI;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Zoom")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float zoomFOV = 30f;
    [SerializeField] private float zoomSpeed = 10f;

    [Header("Bullet")]
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float fireRate = 0.12f;

    private bool isAiming;
    private float nextFireTime = 0f;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = normalFOV;
        }

        if (crosshairUI != null)
        {
            crosshairUI.SetActive(false);
        }

        if (bulletPrefab == null)
        {
            bulletPrefab = Resources.Load<GameObject>("GameplayBullet");
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (GameManager.Instance != null && GameManager.Instance.IsLevelOver) return;

        HandleAimZoom();
        HandleFiring();
    }

    private void HandleAimZoom()
    {
        isAiming = Input.GetMouseButton(1); // right click held

        if (crosshairUI != null)
            crosshairUI.SetActive(isAiming);

        if (playerCamera != null)
        {
            float targetFOV = isAiming ? zoomFOV : normalFOV;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        }
    }

    private void HandleFiring()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime) // left click held
        {
            if (GameManager.Instance != null && !GameManager.Instance.HasAmmo())
            {
                return;
            }

            nextFireTime = Time.time + fireRate;
            FireBullet();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RecordShotFired();
            }
        }
    }

    private void FireBullet()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        if (bulletPrefab == null)
        {
            bulletPrefab = Resources.Load<GameObject>("GameplayBullet");
            if (bulletPrefab == null)
            {
                Debug.LogWarning("PlayerShooting: No bullet prefab assigned or found in Resources!");
                return;
            }
        }

        Vector3 aimDirection = playerCamera.transform.forward;

        // Position spawn slightly in front of camera or fire point to avoid clipping into player colliders
        Vector3 spawnPos;
        if (firePoint != null)
        {
            spawnPos = firePoint.position + aimDirection * 0.45f;
        }
        else
        {
            spawnPos = playerCamera.transform.position + aimDirection * 0.7f;
        }

        Quaternion aimRotation = Quaternion.LookRotation(aimDirection);
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, aimRotation);

        // Ignore collision between spawned bullet and all player colliders
        Collider bulletCol = bulletObj.GetComponent<Collider>();
        if (bulletCol != null)
        {
            Collider[] playerColliders = GetComponentsInChildren<Collider>();
            foreach (var pc in playerColliders)
            {
                Physics.IgnoreCollision(bulletCol, pc);
            }
        }

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Init(Color.yellow, bulletSpeed);
        }
    }
}