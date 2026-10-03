using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text dummyProgressText;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillStatusText;

    [Header("Ammo HUD (UPDATESPRITES_3)")]
    [SerializeField] private Image ammoIconImage;
    [SerializeField] private TMP_Text ammoCountText;

    [Header("Player Health Bar HUD")]
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private Image playerHealthFillImage;
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private Image damageVignetteImage;

    [Header("Victory Window")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text statsAmmoPickedText;
    [SerializeField] private TMP_Text statsShotsFiredText;
    [SerializeField] private TMP_Text statsDummiesKilledText;
    [SerializeField] private Button nextStageButton;

    [Header("Defeat Window")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private TMP_Text defeatReasonText;
    [SerializeField] private Button defeatMainMenuButton;
    [SerializeField] private Button defeatRetryButton;

    private Coroutine damageFlashCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        if (nextStageButton != null)
            nextStageButton.onClick.AddListener(OnNextStageClicked);

        if (defeatMainMenuButton != null)
            defeatMainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (defeatRetryButton != null)
            defeatRetryButton.onClick.AddListener(OnRetryClicked);

        // Ensure ammo sprite is loaded
        if (ammoIconImage != null && ammoIconImage.sprite == null)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("UPDATEDSPRITES");
            if (sprites != null)
            {
                foreach (var s in sprites)
                {
                    if (s.name == "UPDATESPRITES_3")
                    {
                        ammoIconImage.sprite = s;
                        break;
                    }
                }
            }
        }

        // Build Player Health UI if not already wired
        EnsurePlayerHealthUI();
    }

    private void EnsurePlayerHealthUI()
    {
        if (playerHealthSlider != null) return;

        Canvas rootCanvas = GetComponentInChildren<Canvas>();
        if (rootCanvas == null)
        {
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in allCanvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.name.Contains("Canvas") || c.name.Contains("HUD"))
                {
                    rootCanvas = c;
                    break;
                }
            }
        }

        if (rootCanvas == null) return;

        // ---- Damage Vignette Overlay ----
        if (damageVignetteImage == null)
        {
            GameObject vigGO = new GameObject("DamageVignette", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            vigGO.transform.SetParent(rootCanvas.transform, false);
            vigGO.transform.SetAsFirstSibling(); // Underneath other HUD elements
            RectTransform vigRect = vigGO.GetComponent<RectTransform>();
            vigRect.anchorMin = Vector2.zero;
            vigRect.anchorMax = Vector2.one;
            vigRect.offsetMin = Vector2.zero;
            vigRect.offsetMax = Vector2.zero;
            damageVignetteImage = vigGO.GetComponent<Image>();
            damageVignetteImage.color = new Color(0.9f, 0f, 0f, 0f);
            damageVignetteImage.raycastTarget = false;
        }

        // ---- Player Health Bar Container ----
        GameObject barRoot = new GameObject("PlayerHealthBar", typeof(RectTransform));
        barRoot.transform.SetParent(rootCanvas.transform, false);
        RectTransform rootRect = barRoot.GetComponent<RectTransform>();
        // Anchor to bottom-left (above ammo counter)
        rootRect.anchorMin = new Vector2(0f, 0f);
        rootRect.anchorMax = new Vector2(0f, 0f);
        rootRect.pivot = new Vector2(0f, 0f);
        rootRect.anchoredPosition = new Vector2(30f, 95f);
        rootRect.sizeDelta = new Vector2(240f, 28f);

        // Frame / Background
        GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgGO.transform.SetParent(barRoot.transform, false);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bgGO.GetComponent<Image>();
        bgImg.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);

        // Slider Root
        GameObject sliderGO = new GameObject("Slider", typeof(RectTransform));
        sliderGO.transform.SetParent(barRoot.transform, false);
        RectTransform sRect = sliderGO.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.02f, 0.1f);
        sRect.anchorMax = new Vector2(0.98f, 0.9f);
        sRect.offsetMin = Vector2.zero;
        sRect.offsetMax = Vector2.zero;

        playerHealthSlider = sliderGO.AddComponent<Slider>();
        playerHealthSlider.interactable = false;
        playerHealthSlider.transition = Selectable.Transition.None;
        playerHealthSlider.minValue = 0f;
        playerHealthSlider.maxValue = 100f;
        playerHealthSlider.value = 100f;

        // Fill Area
        GameObject faGO = new GameObject("Fill Area", typeof(RectTransform));
        faGO.transform.SetParent(sliderGO.transform, false);
        RectTransform faRect = faGO.GetComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.offsetMin = Vector2.zero;
        faRect.offsetMax = Vector2.zero;

        // Fill
        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillGO.transform.SetParent(faGO.transform, false);
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        playerHealthFillImage = fillGO.GetComponent<Image>();
        playerHealthFillImage.color = new Color(0.2f, 0.9f, 0.25f, 1f);

        playerHealthSlider.fillRect = fillRect;

        // HP Text Label
        GameObject textGO = new GameObject("HPText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(barRoot.transform, false);
        RectTransform tRect = textGO.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = new Vector2(8f, 0f);
        tRect.offsetMax = new Vector2(-8f, 0f);
        playerHealthText = textGO.GetComponent<TextMeshProUGUI>();
        playerHealthText.text = "HP: 100 / 100";
        playerHealthText.fontSize = 15f;
        playerHealthText.fontStyle = FontStyles.Bold;
        playerHealthText.alignment = TextAlignmentOptions.Center;
        playerHealthText.color = Color.white;
    }

    public void UpdatePlayerHealth(float current, float max)
    {
        EnsurePlayerHealthUI();

        if (playerHealthSlider != null)
        {
            playerHealthSlider.maxValue = max;
            playerHealthSlider.value = current;
        }

        if (playerHealthText != null)
        {
            playerHealthText.text = $"HEALTH: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        if (playerHealthFillImage != null)
        {
            float ratio = max > 0f ? current / max : 0f;
            if (ratio > 0.5f)
                playerHealthFillImage.color = Color.Lerp(Color.yellow, new Color(0.2f, 0.9f, 0.2f), (ratio - 0.5f) * 2f);
            else
                playerHealthFillImage.color = Color.Lerp(Color.red, Color.yellow, ratio * 2f);
        }
    }

    public void TriggerDamageVignette()
    {
        if (damageVignetteImage == null) return;
        if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
        damageFlashCoroutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (damageVignetteImage == null) yield break;

        damageVignetteImage.color = new Color(0.9f, 0f, 0f, 0.42f);
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(0.42f, 0f, elapsed / duration);
            damageVignetteImage.color = new Color(0.9f, 0f, 0f, a);
            yield return null;
        }

        damageVignetteImage.color = new Color(0.9f, 0f, 0f, 0f);
    }

    public void UpdateAmmoText(int current, int max)
    {
        if (ammoCountText != null)
        {
            ammoCountText.text = $"x {current}";
            ammoCountText.color = (current <= 5) ? new Color(1f, 0.25f, 0.25f) : Color.white;
        }

        if (ammoIconImage != null)
        {
            ammoIconImage.color = (current <= 5) ? new Color(1f, 0.35f, 0.35f) : Color.white;
        }
    }

    public void UpdateTimerText(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = string.Format("Time: {0:0}:{1:00}", minutes, seconds);
            timerText.color = (time <= 15f) ? Color.red : Color.white;
        }
    }

    public void UpdateDummyProgress(int current, int required)
    {
        if (dummyProgressText != null)
        {
            dummyProgressText.text = $"Enemies: <color=#55FF55>{current}</color> / {required}";
        }
    }

    public void UpdateSkillHUD(string skillName, string status)
    {
        if (skillNameText != null)
            skillNameText.text = $"Skill: {skillName}";

        if (skillStatusText != null)
            skillStatusText.text = status;
    }

    public void ShowVictoryWindow(int ammoPickedUp, int shotsFired, int dummiesKilled)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (statsAmmoPickedText != null)
                statsAmmoPickedText.text = $"Ammo Picked Up: {ammoPickedUp}";

            if (statsShotsFiredText != null)
                statsShotsFiredText.text = $"Shots Fired: {shotsFired}";

            if (statsDummiesKilledText != null)
                statsDummiesKilledText.text = $"Enemies Eliminated: {dummiesKilled}";

            // If final stage, button changes to FINISH CAMPAIGN
            if (nextStageButton != null && GameManager.Instance != null && GameManager.Instance.ActiveLevelData != null)
            {
                string next = GameManager.Instance.ActiveLevelData.nextSceneName;
                if (string.IsNullOrEmpty(next) || next == "MainMenu")
                {
                    TMP_Text btnText = nextStageButton.GetComponentInChildren<TMP_Text>();
                    if (btnText != null) btnText.text = "FINISH CAMPAIGN";
                }
            }
        }
    }

    public void ShowDefeatWindow(string reason)
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);

            if (defeatReasonText != null)
                defeatReasonText.text = reason;
        }
    }

    private void OnNextStageClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadNextStage();
    }

    private void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }

    private void OnRetryClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RetryLevel();
    }
}
