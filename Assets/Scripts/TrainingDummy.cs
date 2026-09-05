using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainingDummy : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float heightOffset = 2.4f;

    private float currentHealth;
    private Canvas healthCanvas;
    private Slider healthSlider;
    private Image fillImage;
    private Image bgImage;
    private Renderer[] renderers;
    private Color[] originalColors;
    private bool isDead = false;

    private static Sprite s_bgSprite;
    private static Sprite s_fillSprite;

    private void Awake()
    {
        currentHealth = maxHealth;
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers != null && renderers.Length > 0)
        {
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].sharedMaterial != null)
                    originalColors[i] = renderers[i].sharedMaterial.color;
            }
        }

        EnsureSpritesLoaded();
        CreateHealthBarUI();
    }

    private static void EnsureSpritesLoaded()
    {
        if (s_bgSprite != null && s_fillSprite != null) return;

        Sprite[] sprites = Resources.LoadAll<Sprite>("UPDATEDSPRITES");
        if (sprites != null)
        {
            foreach (var sp in sprites)
            {
                if (sp.name == "UPDATESPRITES_1") s_bgSprite = sp;
                else if (sp.name == "UPDATESPRITES_2") s_fillSprite = sp;
            }
        }
    }

    private void CreateHealthBarUI()
    {
        // WorldSpace Canvas above dummy
        GameObject canvasGO = new GameObject("DummyHealthCanvas");
        canvasGO.transform.SetParent(transform, false);
        canvasGO.transform.localPosition = new Vector3(0f, heightOffset, 0f);

        healthCanvas = canvasGO.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform cRect = canvasGO.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(2.4f, 0.42f);
        canvasGO.transform.localScale = Vector3.one;

        // ---- Slider Root ----
        GameObject sliderGO = new GameObject("HealthSlider", typeof(RectTransform));
        sliderGO.transform.SetParent(canvasGO.transform, false);
        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.04f, 0.08f);
        sliderRect.anchorMax = new Vector2(0.96f, 0.65f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        healthSlider = sliderGO.AddComponent<Slider>();
        healthSlider.interactable = false;
        healthSlider.transition = Selectable.Transition.None;
        healthSlider.direction = Slider.Direction.LeftToRight;
        healthSlider.minValue = 0f;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        // ---- Background Image (UPDATESPRITES_1: dark frame) ----
        GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgGO.transform.SetParent(sliderGO.transform, false);
        bgImage = bgGO.GetComponent<Image>();
        if (s_bgSprite != null)
        {
            bgImage.sprite = s_bgSprite;
            bgImage.type = Image.Type.Simple;
            bgImage.color = Color.white;
        }
        else
        {
            bgImage.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
        }
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        healthSlider.targetGraphic = bgImage;

        // ---- Fill Area (zero offsets so Slider properly scales fillRect) ----
        GameObject fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform faRect = fillAreaGO.GetComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.offsetMin = Vector2.zero;
        faRect.offsetMax = Vector2.zero;

        // ---- Fill Image (UPDATESPRITES_2: red bar fill) ----
        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        fillImage = fillGO.GetComponent<Image>();
        if (s_fillSprite != null)
        {
            fillImage.sprite = s_fillSprite;
            fillImage.type = Image.Type.Simple;
            fillImage.color = Color.white;
        }
        else
        {
            fillImage.color = new Color(0.85f, 0.12f, 0.12f, 1f);
        }
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        healthSlider.fillRect = fillRect;

        // ---- Label Above Slider ----
        GameObject textGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI lbl = textGO.GetComponent<TextMeshProUGUI>();
        lbl.text = "TRAINING DUMMY";
        lbl.fontSize = 0.19f;
        lbl.fontStyle = FontStyles.Bold;
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.color = Color.white;
        RectTransform tRect = textGO.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0f, 0.65f);
        tRect.anchorMax = new Vector2(1f, 1f);
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (healthCanvas != null)
        {
            Camera cam = Camera.main;
            if (cam == null) cam = FindAnyObjectByType<Camera>();
            if (cam != null)
            {
                healthCanvas.transform.rotation = cam.transform.rotation;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        StartCoroutine(DamageFlashRoutine());

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                    renderers[i].material.color = Color.red;
            }
        }

        yield return new WaitForSeconds(0.08f);

        if (renderers != null && originalColors != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null && i < originalColors.Length)
                    renderers[i].material.color = originalColors[i];
            }
        }
    }

    private void Die()
    {
        isDead = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RecordDummyKilled();
        }
        Destroy(gameObject);
    }
}
