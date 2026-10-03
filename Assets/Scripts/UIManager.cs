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
    [SerializeField] private RectTransform ammoHUDPanel;
    [SerializeField] private TMP_Text ammoLabelText;
    [SerializeField] private Slider ammoSlider;
    [SerializeField] private Image ammoSliderFill;
    private Coroutine flashEmptyAmmoCoroutine;

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

        // Build Player Health & Ammo UI if not already wired
        EnsurePlayerHealthUI();
        EnsureAmmoHUD();
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
        Transform existingBar = rootCanvas.transform.Find("PlayerHealthBar");
        GameObject barRoot;
        if (existingBar != null)
        {
            barRoot = existingBar.gameObject;
        }
        else
        {
            barRoot = new GameObject("PlayerHealthBar", typeof(RectTransform));
            barRoot.transform.SetParent(rootCanvas.transform, false);
        }

        RectTransform rootRect = barRoot.GetComponent<RectTransform>();
        // Anchor to bottom-left (neatly stacked above SkillHUD)
        rootRect.anchorMin = new Vector2(0f, 0f);
        rootRect.anchorMax = new Vector2(0f, 0f);
        rootRect.pivot = new Vector2(0f, 0f);
        rootRect.anchoredPosition = new Vector2(20f, 98f);
        rootRect.sizeDelta = new Vector2(320f, 28f);

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

    public void EnsureAmmoHUD()
    {
        if (ammoHUDPanel != null && ammoCountText != null) return;

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

        // Check if AmmoHUD already exists in Canvas
        Transform existingHUD = rootCanvas.transform.Find("AmmoHUD");
        GameObject hudGO;
        if (existingHUD != null)
        {
            hudGO = existingHUD.gameObject;
        }
        else
        {
            hudGO = new GameObject("AmmoHUD", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            hudGO.transform.SetParent(rootCanvas.transform, false);
            hudGO.transform.SetAsLastSibling();
        }

        ammoHUDPanel = hudGO.GetComponent<RectTransform>();
        ammoHUDPanel.anchorMin = new Vector2(1f, 0f);
        ammoHUDPanel.anchorMax = new Vector2(1f, 0f);
        ammoHUDPanel.pivot = new Vector2(1f, 0f);
        ammoHUDPanel.anchoredPosition = new Vector2(-24f, 20f);
        ammoHUDPanel.sizeDelta = new Vector2(260f, 85f);

        Image panelBg = hudGO.GetComponent<Image>();
        if (panelBg != null)
        {
            panelBg.color = new Color(0.06f, 0.09f, 0.14f, 0.92f);
            panelBg.raycastTarget = false;
        }

        // Top accent line
        Transform accentTr = ammoHUDPanel.Find("AccentLine");
        if (accentTr == null)
        {
            GameObject accentGO = new GameObject("AccentLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            accentGO.transform.SetParent(ammoHUDPanel, false);
            RectTransform accentRect = accentGO.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 1f);
            accentRect.anchorMax = new Vector2(1f, 1f);
            accentRect.pivot = new Vector2(0.5f, 1f);
            accentRect.anchoredPosition = Vector2.zero;
            accentRect.sizeDelta = new Vector2(0f, 2.5f);
            Image accentImg = accentGO.GetComponent<Image>();
            accentImg.color = new Color(0.24f, 0.65f, 1f, 0.95f);
            accentImg.raycastTarget = false;
        }

        // Ammo Icon
        if (ammoIconImage == null)
        {
            Transform existingIcon = rootCanvas.transform.Find("AmmoIcon");
            if (existingIcon != null)
            {
                ammoIconImage = existingIcon.GetComponent<Image>();
            }
        }

        if (ammoIconImage != null)
        {
            ammoIconImage.transform.SetParent(ammoHUDPanel, false);
            RectTransform iconRect = ammoIconImage.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(14f, 2f);
            iconRect.sizeDelta = new Vector2(44f, 44f);
            ammoIconImage.preserveAspect = true;
            ammoIconImage.raycastTarget = false;
        }
        else
        {
            GameObject iconGO = new GameObject("AmmoIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGO.transform.SetParent(ammoHUDPanel, false);
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(14f, 2f);
            iconRect.sizeDelta = new Vector2(44f, 44f);
            ammoIconImage = iconGO.GetComponent<Image>();
            ammoIconImage.preserveAspect = true;
            ammoIconImage.raycastTarget = false;
        }

        // Load sprite if missing
        if (ammoIconImage.sprite == null)
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

        // Find or build ammoCountText
        if (ammoCountText == null)
        {
            Transform existingCount = rootCanvas.transform.Find("AmmoCount");
            if (existingCount != null)
            {
                ammoCountText = existingCount.GetComponent<TextMeshProUGUI>();
            }
        }

        TMP_FontAsset sharedFont = null;
        if (ammoCountText != null)
        {
            sharedFont = ammoCountText.font;
            ammoCountText.transform.SetParent(ammoHUDPanel, false);
            RectTransform countRect = ammoCountText.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0f, 0.5f);
            countRect.anchorMax = new Vector2(1f, 0.5f);
            countRect.pivot = new Vector2(0f, 0.5f);
            countRect.anchoredPosition = new Vector2(68f, -2f);
            countRect.sizeDelta = new Vector2(-76f, 40f);
            ammoCountText.alignment = TextAlignmentOptions.MidlineLeft;
            ammoCountText.enableWordWrapping = false;
            ammoCountText.richText = true;
            ammoCountText.raycastTarget = false;
        }
        else
        {
            GameObject countGO = new GameObject("AmmoCount", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            countGO.transform.SetParent(ammoHUDPanel, false);
            RectTransform countRect = countGO.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0f, 0.5f);
            countRect.anchorMax = new Vector2(1f, 0.5f);
            countRect.pivot = new Vector2(0f, 0.5f);
            countRect.anchoredPosition = new Vector2(68f, -2f);
            countRect.sizeDelta = new Vector2(-76f, 40f);
            ammoCountText = countGO.GetComponent<TextMeshProUGUI>();
            ammoCountText.alignment = TextAlignmentOptions.MidlineLeft;
            ammoCountText.enableWordWrapping = false;
            ammoCountText.richText = true;
            ammoCountText.raycastTarget = false;
        }

        // Header label: "AMMUNITION"
        Transform labelTr = ammoHUDPanel.Find("AmmoHeader");
        if (labelTr == null)
        {
            GameObject labelGO = new GameObject("AmmoHeader", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(ammoHUDPanel, false);
            RectTransform lRect = labelGO.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0f, 1f);
            lRect.anchorMax = new Vector2(1f, 1f);
            lRect.pivot = new Vector2(0f, 1f);
            lRect.anchoredPosition = new Vector2(68f, -9f);
            lRect.sizeDelta = new Vector2(-76f, 18f);

            ammoLabelText = labelGO.GetComponent<TextMeshProUGUI>();
            if (sharedFont != null) ammoLabelText.font = sharedFont;
            ammoLabelText.text = "AMMUNITION";
            ammoLabelText.fontSize = 11f;
            ammoLabelText.fontStyle = FontStyles.Bold;
            ammoLabelText.color = new Color(0.6f, 0.72f, 0.85f, 0.9f);
            ammoLabelText.alignment = TextAlignmentOptions.Left;
            ammoLabelText.raycastTarget = false;
        }
        else
        {
            ammoLabelText = labelTr.GetComponent<TextMeshProUGUI>();
        }

        // Sleek ammo progress bar at bottom of AmmoHUD
        Transform barTr = ammoHUDPanel.Find("AmmoProgressBar");
        if (barTr == null)
        {
            GameObject barGO = new GameObject("AmmoProgressBar", typeof(RectTransform));
            barGO.transform.SetParent(ammoHUDPanel, false);
            RectTransform barRect = barGO.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(1f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.anchoredPosition = new Vector2(0f, 6f);
            barRect.sizeDelta = new Vector2(-24f, 5f);

            // Bar background
            GameObject barBgGO = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            barBgGO.transform.SetParent(barGO.transform, false);
            RectTransform bbgRect = barBgGO.GetComponent<RectTransform>();
            bbgRect.anchorMin = Vector2.zero;
            bbgRect.anchorMax = Vector2.one;
            bbgRect.offsetMin = Vector2.zero;
            bbgRect.offsetMax = Vector2.zero;
            Image bbgImg = barBgGO.GetComponent<Image>();
            bbgImg.color = new Color(0.12f, 0.16f, 0.22f, 0.9f);
            bbgImg.raycastTarget = false;

            // Slider
            ammoSlider = barGO.AddComponent<Slider>();
            ammoSlider.interactable = false;
            ammoSlider.transition = Selectable.Transition.None;
            ammoSlider.minValue = 0f;
            ammoSlider.maxValue = 30f;
            ammoSlider.value = 30f;

            // Fill Area
            GameObject faGO = new GameObject("Fill Area", typeof(RectTransform));
            faGO.transform.SetParent(barGO.transform, false);
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
            ammoSliderFill = fillGO.GetComponent<Image>();
            ammoSliderFill.color = new Color(0.24f, 0.75f, 1f, 1f);
            ammoSliderFill.raycastTarget = false;

            ammoSlider.fillRect = fillRect;
        }
        else
        {
            ammoSlider = barTr.GetComponent<Slider>();
            Transform fTr = barTr.Find("Fill Area/Fill");
            if (fTr != null)
                ammoSliderFill = fTr.GetComponent<Image>();
        }
    }

    public void UpdateAmmoText(int current, int max)
    {
        EnsureAmmoHUD();

        if (ammoSlider != null)
        {
            ammoSlider.maxValue = max;
            ammoSlider.value = current;
        }

        if (ammoSliderFill != null)
        {
            if (current <= 0)
                ammoSliderFill.color = new Color(1f, 0.15f, 0.15f, 0.9f);
            else if (current <= 5)
                ammoSliderFill.color = new Color(1f, 0.55f, 0.15f, 1f);
            else
                ammoSliderFill.color = new Color(0.24f, 0.75f, 1f, 1f);
        }

        if (ammoCountText != null)
        {
            if (current <= 0)
            {
                ammoCountText.text = $"<size=34><b><color=#FF3333>0</color></b></size> <size=18><color=#8899AA>/ {max}</color></size>  <size=12><color=#FF4444>[EMPTY]</color></size>";
            }
            else if (current <= 5)
            {
                ammoCountText.text = $"<size=34><b><color=#FFAA33>{current}</color></b></size> <size=18><color=#88A0B8>/ {max}</color></size>  <size=12><color=#FFAA33>[LOW]</color></size>";
            }
            else
            {
                ammoCountText.text = $"<size=34><b><color=#FFFFFF>{current}</color></b></size> <size=18><color=#88A0B8>/ {max}</color></size>";
            }
        }

        if (ammoIconImage != null)
        {
            if (current <= 0)
                ammoIconImage.color = new Color(1f, 0.25f, 0.25f, 1f);
            else if (current <= 5)
                ammoIconImage.color = new Color(1f, 0.7f, 0.25f, 1f);
            else
                ammoIconImage.color = Color.white;
        }

        if (ammoLabelText != null)
        {
            if (current <= 0)
                ammoLabelText.text = "<color=#FF4444>NO AMMO - COLLECT CRATE</color>";
            else if (current <= 5)
                ammoLabelText.text = "<color=#FFAA33>LOW AMMO WARNING</color>";
            else
                ammoLabelText.text = "AMMUNITION";
        }
    }

    public void FlashEmptyAmmo()
    {
        EnsureAmmoHUD();
        if (ammoHUDPanel == null) return;
        if (flashEmptyAmmoCoroutine != null) StopCoroutine(flashEmptyAmmoCoroutine);
        flashEmptyAmmoCoroutine = StartCoroutine(FlashEmptyAmmoRoutine());
    }

    private IEnumerator FlashEmptyAmmoRoutine()
    {
        Image bg = ammoHUDPanel.GetComponent<Image>();
        if (bg == null) yield break;

        Color original = new Color(0.06f, 0.09f, 0.14f, 0.92f);
        Color flashColor = new Color(0.6f, 0.08f, 0.08f, 0.95f);

        for (int i = 0; i < 2; i++)
        {
            bg.color = flashColor;
            yield return new WaitForSecondsRealtime(0.08f);
            bg.color = original;
            yield return new WaitForSecondsRealtime(0.08f);
        }
        bg.color = original;
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
