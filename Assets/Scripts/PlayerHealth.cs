using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invulnerabilityDuration = 0.2f;

    private float currentHealth;
    private bool isDead = false;
    private float lastDamageTime = -10f;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
        isDead = false;

        // Ensure Player has Player tag
        if (!gameObject.CompareTag("Player"))
        {
            try
            {
                gameObject.tag = "Player";
            }
            catch
            {
                // In case Player tag is not pre-registered in project tags, fallback safely
            }
        }
    }

    private void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(float damage, string sourceName = "Enemy")
    {
        if (isDead) return;
        if (Time.time < lastDamageTime + invulnerabilityDuration) return;

        lastDamageTime = Time.time;
        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
            UIManager.Instance.TriggerDamageVignette();
        }

        if (currentHealth <= 0f)
        {
            Die(sourceName);
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
        }
    }

    private void Die(string sourceName)
    {
        if (isDead) return;
        isDead = true;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLevel($"Player Eliminated! You were taken down by {sourceName}.");
        }
    }
}
