using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject crosshairUI;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Image bulletColorIndicator;

    [Header("Zoom")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float zoomFOV = 30f;
    [SerializeField] private float zoomSpeed = 10f;

    [Header("Bullet")]
    [SerializeField] private float bulletSpeed = 40f;
    [SerializeField] private float fireRate = 0.1f; // seconds between shots while holding

    private Color currentBulletColor = Color.white;
    private bool isAiming;
    private float nextFireTime = 0f;

    private void Start()
    {
        if (playerCamera != null)
            playerCamera.fieldOfView = normalFOV;

        if (crosshairUI != null)
            crosshairUI.SetActive(false);

        if (bulletColorIndicator != null)
            bulletColorIndicator.color = currentBulletColor;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return; // don't act while paused

        HandleAimZoom();
        HandleColorSwitch();
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

    private void HandleColorSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            currentBulletColor = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.9f, 1f);

            if (bulletColorIndicator != null)
                bulletColorIndicator.color = currentBulletColor;
        }
    }

    private void HandleFiring()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime) // left click held = automatic
        {
            nextFireTime = Time.time + fireRate;
            FireBullet();
        }
    }

    private void FireBullet()
    {
        if (bulletPrefab == null || firePoint == null || playerCamera == null) return;

        Vector3 aimDirection = playerCamera.transform.forward;
        Quaternion aimRotation = Quaternion.LookRotation(aimDirection);

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, aimRotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Init(currentBulletColor, bulletSpeed);
    }
}