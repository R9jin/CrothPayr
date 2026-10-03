using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Cards / Buttons")]
    [SerializeField] private Button charAButton;
    [SerializeField] private Button charBButton;
    [SerializeField] private Button charCButton;

    [Header("Card Outlines/Highlights")]
    [SerializeField] private Image charAHighlight;
    [SerializeField] private Image charBHighlight;
    [SerializeField] private Image charCHighlight;

    [Header("Details Display")]
    [SerializeField] private TMP_Text selectedTitleText;
    [SerializeField] private TMP_Text selectedSkillText;
    [SerializeField] private TMP_Text selectedDescriptionText;

    [Header("Stage Selection UI")]
    [SerializeField] private Button stage1Button;
    [SerializeField] private Button stage2Button;
    [SerializeField] private Button stage3Button;
    [SerializeField] private Image stage1Highlight;
    [SerializeField] private Image stage2Highlight;
    [SerializeField] private Image stage3Highlight;
    [SerializeField] private TMP_Text stageInfoText;

    [Header("Action Buttons")]
    [SerializeField] private Button deployButton;
    [SerializeField] private Button backButton;

    [Header("Menu Manager")]
    [SerializeField] private MenuManager menuManager;

    private readonly Color activeHighlightColor = new Color(0.2f, 0.95f, 0.4f, 1f);
    private readonly Color inactiveHighlightColor = new Color(0.25f, 0.30f, 0.38f, 0.5f);
    private readonly Color buttonActiveColor = new Color(0.12f, 0.48f, 0.28f, 1f);
    private readonly Color buttonInactiveColor = new Color(0.14f, 0.18f, 0.24f, 0.95f);
    private int currentStage = 1;

    private void Awake()
    {
        if (charAButton != null) charAButton.onClick.AddListener(() => Select(CharacterType.CharacterA_Roll));
        if (charBButton != null) charBButton.onClick.AddListener(() => Select(CharacterType.CharacterB_DoubleJump));
        if (charCButton != null) charCButton.onClick.AddListener(() => Select(CharacterType.CharacterC_Teleport));

        if (menuManager == null) menuManager = Object.FindFirstObjectByType<MenuManager>();

        if (deployButton != null)
        {
            deployButton.onClick.AddListener(() =>
            {
                if (menuManager != null) menuManager.ConfirmAndStartGame();
            });
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(() =>
            {
                if (menuManager != null) menuManager.CloseCharacterSelect();
            });
        }

        EnforceCleanTacticalLayout();
    }

    private void OnEnable()
    {
        EnforceCleanTacticalLayout();

        Select(CharacterSelection.SelectedCharacter);

        if (menuManager != null)
        {
            currentStage = menuManager.SelectedStage;
        }
        else
        {
            currentStage = PlayerPrefs.GetInt("SelectedStage", 1);
        }

        SelectStage(currentStage);
    }

    public void Select(CharacterType type)
    {
        CharacterSelection.SelectedCharacter = type;

        if (charAHighlight != null) charAHighlight.color = (type == CharacterType.CharacterA_Roll) ? activeHighlightColor : inactiveHighlightColor;
        if (charBHighlight != null) charBHighlight.color = (type == CharacterType.CharacterB_DoubleJump) ? activeHighlightColor : inactiveHighlightColor;
        if (charCHighlight != null) charCHighlight.color = (type == CharacterType.CharacterC_Teleport) ? activeHighlightColor : inactiveHighlightColor;

        if (selectedTitleText != null)
        {
            selectedTitleText.text = $"<color=#00FFAA>{CharacterSelection.GetCharacterName(type).ToUpper()}</color>   <color=#666666>|</color>   <color=#FFD700>SKILL: {CharacterSelection.GetSkillName(type).ToUpper()}</color>";
        }

        if (selectedSkillText != null)
        {
            selectedSkillText.gameObject.SetActive(false); // Merged into header to avoid text collision
        }

        if (selectedDescriptionText != null)
        {
            selectedDescriptionText.text = CharacterSelection.GetSkillDescription(type);
        }
    }

    public void SelectStage(int stageNumber)
    {
        currentStage = Mathf.Clamp(stageNumber, 1, 3);

        if (menuManager != null)
        {
            menuManager.SetSelectedStage(currentStage);
        }
        else
        {
            PlayerPrefs.SetInt("SelectedStage", currentStage);
            PlayerPrefs.Save();
        }

        // Update Stage Highlights
        if (stage1Highlight != null) stage1Highlight.color = (currentStage == 1) ? buttonActiveColor : buttonInactiveColor;
        if (stage2Highlight != null) stage2Highlight.color = (currentStage == 2) ? buttonActiveColor : buttonInactiveColor;
        if (stage3Highlight != null) stage3Highlight.color = (currentStage == 3) ? buttonActiveColor : buttonInactiveColor;

        // Update Stage Description
        if (stageInfoText != null)
        {
            switch (currentStage)
            {
                case 1:
                    stageInfoText.text = "<b><color=#55FF88>STAGE 1: ARMORY</color></b> — Objective: Eliminate 5 Targets in 90s\n<color=#A0C8D8>Intelligence: Line of Sight Snipers & Vision Cone Rushers</color>";
                    break;
                case 2:
                    stageInfoText.text = "<b><color=#55FF88>STAGE 2: ENERGY LAB</color></b> — Objective: Eliminate 8 Targets in 90s\n<color=#A0C8D8>Intelligence: Acoustic Proximity AIs (Sneak past with Crouch [C / Ctrl])</color>";
                    break;
                case 3:
                    stageInfoText.text = "<b><color=#55FF88>STAGE 3: TECH VAULT (FINAL)</color></b> — Objective: Eliminate 10 Targets in 120s\n<color=#A0C8D8>Intelligence: High-threat sector with all 3 AI archetypes coordinated</color>";
                    break;
            }
        }
    }

    private void EnforceCleanTacticalLayout()
    {
        // 1. Root Panel: Size 980x710 centered
        RectTransform rootRect = GetComponent<RectTransform>();
        if (rootRect != null)
        {
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(980f, 710f);

            Image rootImg = GetComponent<Image>();
            if (rootImg != null)
            {
                rootImg.color = new Color(0.05f, 0.07f, 0.11f, 0.96f);
            }
        }

        // 2. Title Text: Top center
        Transform titleTr = transform.Find("TitleText");
        if (titleTr != null)
        {
            RectTransform tr = titleTr.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 1f);
            tr.anchorMax = new Vector2(0.5f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.anchoredPosition = new Vector2(0f, -14f);
            tr.sizeDelta = new Vector2(920f, 36f);

            TMP_Text tmp = titleTr.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = "<b>TACTICAL OPERATIVE & MISSION SELECT</b>";
                tmp.fontSize = 20f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(1f, 0.82f, 0.25f, 1f);
            }
        }

        // 3. Format Character Cards
        FormatCard(charAButton, -310f, "OPERATIVE A", "Acrobat", "Combat Roll", "2 Charges [Q]");
        FormatCard(charBButton, 0f, "OPERATIVE B", "Striker", "Double Jump", "Air Leap [Space]");
        FormatCard(charCButton, 310f, "OPERATIVE C", "Mystic", "Phase Teleport", "Tactical Blink [Q/E]");

        // 4. Operative Details Box (PreviewPanel)
        Transform previewTr = transform.Find("PreviewPanel");
        if (previewTr != null)
        {
            RectTransform pRect = previewTr.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 1f);
            pRect.anchorMax = new Vector2(0.5f, 1f);
            pRect.pivot = new Vector2(0.5f, 1f);
            pRect.anchoredPosition = new Vector2(0f, -228f);
            pRect.sizeDelta = new Vector2(910f, 66f);

            Image pImg = previewTr.GetComponent<Image>();
            if (pImg != null) pImg.color = new Color(0.09f, 0.12f, 0.17f, 0.95f);

            if (selectedTitleText != null)
            {
                RectTransform tRect = selectedTitleText.GetComponent<RectTransform>();
                tRect.anchorMin = new Vector2(0f, 1f);
                tRect.anchorMax = new Vector2(1f, 1f);
                tRect.pivot = new Vector2(0f, 1f);
                tRect.anchoredPosition = new Vector2(18f, -7f);
                tRect.sizeDelta = new Vector2(-36f, 22f);
                selectedTitleText.fontSize = 14f;
                selectedTitleText.fontStyle = FontStyles.Bold;
                selectedTitleText.alignment = TextAlignmentOptions.Left;
            }

            if (selectedSkillText != null)
            {
                selectedSkillText.gameObject.SetActive(false);
            }

            if (selectedDescriptionText != null)
            {
                RectTransform dRect = selectedDescriptionText.GetComponent<RectTransform>();
                dRect.anchorMin = new Vector2(0f, 0f);
                dRect.anchorMax = new Vector2(1f, 0f);
                dRect.pivot = new Vector2(0f, 0f);
                dRect.anchoredPosition = new Vector2(18f, 7f);
                dRect.sizeDelta = new Vector2(-36f, 24f);
                selectedDescriptionText.fontSize = 12.5f;
                selectedDescriptionText.color = new Color(0.82f, 0.86f, 0.90f, 1f);
                selectedDescriptionText.alignment = TextAlignmentOptions.Left;
            }
        }

        // 5. Stage Selector Bar & Buttons
        BuildOrFormatStageSelector();

        // 6. Action Buttons: Anchored at bottom
        if (backButton != null)
        {
            RectTransform bRect = backButton.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0f);
            bRect.anchorMax = new Vector2(0.5f, 0f);
            bRect.pivot = new Vector2(0.5f, 0f);
            bRect.anchoredPosition = new Vector2(-240f, 22f);
            bRect.sizeDelta = new Vector2(230f, 46f);

            TMP_Text bText = backButton.GetComponentInChildren<TMP_Text>();
            if (bText != null)
            {
                bText.text = "<b>< BACK TO MENU</b>";
                bText.fontSize = 13.5f;
                bText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            }

            Image bImg = backButton.GetComponent<Image>();
            if (bImg != null) bImg.color = new Color(0.22f, 0.26f, 0.32f, 0.95f);
        }

        if (deployButton != null)
        {
            RectTransform dRect = deployButton.GetComponent<RectTransform>();
            dRect.anchorMin = new Vector2(0.5f, 0f);
            dRect.anchorMax = new Vector2(0.5f, 0f);
            dRect.pivot = new Vector2(0.5f, 0f);
            dRect.anchoredPosition = new Vector2(180f, 22f);
            dRect.sizeDelta = new Vector2(360f, 46f);

            TMP_Text dText = deployButton.GetComponentInChildren<TMP_Text>();
            if (dText != null)
            {
                dText.text = "<b>DEPLOY OPERATIVE ></b>";
                dText.fontSize = 14.5f;
                dText.color = Color.white;
            }

            Image dImg = deployButton.GetComponent<Image>();
            if (dImg != null) dImg.color = new Color(0.12f, 0.60f, 0.30f, 1f);
        }
    }

    private void FormatCard(Button btn, float posX, string header, string className, string skill, string tag)
    {
        if (btn == null) return;

        RectTransform r = btn.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 1f);
        r.anchorMax = new Vector2(0.5f, 1f);
        r.pivot = new Vector2(0.5f, 1f);
        r.anchoredPosition = new Vector2(posX, -58f);
        r.sizeDelta = new Vector2(285f, 155f);

        Image cardImg = btn.GetComponent<Image>();
        if (cardImg != null) cardImg.color = new Color(0.10f, 0.13f, 0.18f, 0.92f);

        // Child texts
        TMP_Text[] texts = btn.GetComponentsInChildren<TMP_Text>(true);
        foreach (var t in texts)
        {
            string n = t.gameObject.name;
            RectTransform tr = t.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 1f);
            tr.anchorMax = new Vector2(0.5f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);

            if (n.Contains("Header"))
            {
                tr.anchoredPosition = new Vector2(0f, -8f);
                tr.sizeDelta = new Vector2(265f, 22f);
                t.text = header;
                t.fontSize = 14f;
                t.fontStyle = FontStyles.Bold;
                t.alignment = TextAlignmentOptions.Center;
                t.color = Color.white;
            }
            else if (n.Contains("Class"))
            {
                tr.anchoredPosition = new Vector2(0f, -32f);
                tr.sizeDelta = new Vector2(265f, 20f);
                t.text = className;
                t.fontSize = 12.5f;
                t.fontStyle = FontStyles.Normal;
                t.alignment = TextAlignmentOptions.Center;
                t.color = new Color(0f, 0.88f, 1f, 1f);
            }
            else if (n.Contains("Skill"))
            {
                tr.anchoredPosition = new Vector2(0f, -66f);
                tr.sizeDelta = new Vector2(265f, 24f);
                t.text = skill;
                t.fontSize = 14.5f;
                t.fontStyle = FontStyles.Bold;
                t.alignment = TextAlignmentOptions.Center;
                t.color = new Color(1f, 0.85f, 0.35f, 1f);
            }
            else if (n.Contains("Tag"))
            {
                tr.anchoredPosition = new Vector2(0f, -96f);
                tr.sizeDelta = new Vector2(265f, 20f);
                t.text = tag;
                t.fontSize = 11.5f;
                t.alignment = TextAlignmentOptions.Center;
                t.color = new Color(0.70f, 0.76f, 0.82f, 1f);
            }
        }
    }

    private void BuildOrFormatStageSelector()
    {
        Transform barTr = transform.Find("StageSelectorBar");
        GameObject stageBarRoot;
        if (barTr == null)
        {
            stageBarRoot = new GameObject("StageSelectorBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            stageBarRoot.transform.SetParent(transform, false);
        }
        else
        {
            stageBarRoot = barTr.gameObject;
        }

        RectTransform barRect = stageBarRoot.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 1f);
        barRect.anchorMax = new Vector2(0.5f, 1f);
        barRect.pivot = new Vector2(0.5f, 1f);
        barRect.anchoredPosition = new Vector2(0f, -304f);
        barRect.sizeDelta = new Vector2(910f, 76f);

        Image barImg = stageBarRoot.GetComponent<Image>();
        if (barImg != null) barImg.color = new Color(0.08f, 0.10f, 0.14f, 0.85f);

        // Section header label
        Transform headerTr = stageBarRoot.transform.Find("StageHeaderLabel");
        TextMeshProUGUI headerTMP;
        if (headerTr == null)
        {
            GameObject hGO = new GameObject("StageHeaderLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            hGO.transform.SetParent(stageBarRoot.transform, false);
            headerTMP = hGO.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            headerTMP = headerTr.GetComponent<TextMeshProUGUI>();
        }
        RectTransform hRect = headerTMP.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0.5f, 1f);
        hRect.anchorMax = new Vector2(0.5f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.anchoredPosition = new Vector2(0f, -6f);
        hRect.sizeDelta = new Vector2(880f, 18f);
        headerTMP.text = "<color=#FFCC00><b>MISSION DEPLOYMENT SECTOR</b></color>";
        headerTMP.fontSize = 11.5f;
        headerTMP.alignment = TextAlignmentOptions.Center;

        // Stage Buttons
        string[] stageLabels = new string[] { "STAGE 1: ARMORY", "STAGE 2: ENERGY LAB", "STAGE 3: TECH VAULT" };
        Button[] btns = new Button[3];
        Image[] highlights = new Image[3];
        float[] xPositions = new float[] { -295f, 0f, 295f };

        for (int i = 0; i < 3; i++)
        {
            int stageNum = i + 1;
            Transform btnTr = stageBarRoot.transform.Find($"Stage{stageNum}Button");
            GameObject btnGO;
            if (btnTr == null)
            {
                btnGO = new GameObject($"Stage{stageNum}Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnGO.transform.SetParent(stageBarRoot.transform, false);
            }
            else
            {
                btnGO = btnTr.gameObject;
            }

            RectTransform bRect = btnGO.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 1f);
            bRect.anchorMax = new Vector2(0.5f, 1f);
            bRect.pivot = new Vector2(0.5f, 1f);
            bRect.anchoredPosition = new Vector2(xPositions[i], -30f);
            bRect.sizeDelta = new Vector2(275f, 38f);

            Image img = btnGO.GetComponent<Image>();
            img.color = buttonInactiveColor;
            highlights[i] = img;

            Button btn = btnGO.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectStage(stageNum));
            btns[i] = btn;

            // Button label
            Transform textTr = btnGO.transform.Find("Text");
            TextMeshProUGUI tmp;
            if (textTr == null)
            {
                GameObject tGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                tGO.transform.SetParent(btnGO.transform, false);
                tmp = tGO.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                tmp = textTr.GetComponent<TextMeshProUGUI>();
            }
            RectTransform tRect = tmp.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            tmp.text = $"<b>{stageLabels[i]}</b>";
            tmp.fontSize = 12f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        stage1Button = btns[0];
        stage2Button = btns[1];
        stage3Button = btns[2];
        stage1Highlight = highlights[0];
        stage2Highlight = highlights[1];
        stage3Highlight = highlights[2];

        // 7. Stage Briefing Box (Dedicated container below StageSelectorBar)
        Transform briefTr = transform.Find("StageBriefingBox");
        GameObject briefGO;
        if (briefTr == null)
        {
            briefGO = new GameObject("StageBriefingBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            briefGO.transform.SetParent(transform, false);
        }
        else
        {
            briefGO = briefTr.gameObject;
        }

        RectTransform briefRect = briefGO.GetComponent<RectTransform>();
        briefRect.anchorMin = new Vector2(0.5f, 1f);
        briefRect.anchorMax = new Vector2(0.5f, 1f);
        briefRect.pivot = new Vector2(0.5f, 1f);
        briefRect.anchoredPosition = new Vector2(0f, -390f);
        briefRect.sizeDelta = new Vector2(910f, 62f);

        Image briefImg = briefGO.GetComponent<Image>();
        if (briefImg != null) briefImg.color = new Color(0.09f, 0.12f, 0.17f, 0.95f);

        Transform infoTr = briefGO.transform.Find("StageInfoText");
        if (infoTr == null)
        {
            Transform oldInfo = stageBarRoot.transform.Find("StageInfoText");
            if (oldInfo != null)
            {
                oldInfo.SetParent(briefGO.transform, false);
                infoTr = oldInfo;
            }
            else
            {
                GameObject iGO = new GameObject("StageInfoText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                iGO.transform.SetParent(briefGO.transform, false);
                infoTr = iGO.transform;
            }
        }

        RectTransform infoRect = infoTr.GetComponent<RectTransform>();
        infoRect.anchorMin = Vector2.zero;
        infoRect.anchorMax = Vector2.one;
        infoRect.offsetMin = new Vector2(16f, 4f);
        infoRect.offsetMax = new Vector2(-16f, -4f);

        stageInfoText = infoTr.GetComponent<TextMeshProUGUI>();
        stageInfoText.fontSize = 12.5f;
        stageInfoText.alignment = TextAlignmentOptions.Center;
        stageInfoText.color = Color.white;
    }
}
