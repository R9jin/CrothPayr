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

    private Color activeHighlightColor = new Color(0.2f, 0.9f, 0.3f, 1f);
    private Color inactiveHighlightColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    private int currentStage = 1;

    private void Awake()
    {
        if (charAButton != null) charAButton.onClick.AddListener(() => Select(CharacterType.CharacterA_Roll));
        if (charBButton != null) charBButton.onClick.AddListener(() => Select(CharacterType.CharacterB_DoubleJump));
        if (charCButton != null) charCButton.onClick.AddListener(() => Select(CharacterType.CharacterC_Teleport));

        if (menuManager == null) menuManager = FindAnyObjectByType<MenuManager>();

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

        BuildStageSelectorUIIfNeeded();
    }

    private void OnEnable()
    {
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
            selectedTitleText.text = CharacterSelection.GetCharacterName(type);

        if (selectedSkillText != null)
            selectedSkillText.text = $"Skill: {CharacterSelection.GetSkillName(type)}";

        if (selectedDescriptionText != null)
            selectedDescriptionText.text = CharacterSelection.GetSkillDescription(type);
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
        if (stage1Highlight != null) stage1Highlight.color = (currentStage == 1) ? activeHighlightColor : inactiveHighlightColor;
        if (stage2Highlight != null) stage2Highlight.color = (currentStage == 2) ? activeHighlightColor : inactiveHighlightColor;
        if (stage3Highlight != null) stage3Highlight.color = (currentStage == 3) ? activeHighlightColor : inactiveHighlightColor;

        // Update Stage Description
        if (stageInfoText != null)
        {
            switch (currentStage)
            {
                case 1:
                    stageInfoText.text = "<b>STAGE 1: ARMORY</b>\n<color=#BBBBBB>Objective: Eliminate 5 Targets in 90s\nThreats: Line of Sight Snipers & Vision Cone Rushers</color>";
                    break;
                case 2:
                    stageInfoText.text = "<b>STAGE 2: ENERGY LAB</b>\n<color=#BBBBBB>Objective: Eliminate 8 Targets in 90s\nThreats: Proximity Acoustic Sensors (Use Crouch [C/Ctrl] to sneak!)</color>";
                    break;
                case 3:
                    stageInfoText.text = "<b>STAGE 3: TECH VAULT (FINAL)</b>\n<color=#BBBBBB>Objective: Eliminate 10 Targets in 120s\nThreats: Coordinated Sector with all 3 AI archetypes</color>";
                    break;
            }
        }
    }

    private void BuildStageSelectorUIIfNeeded()
    {
        if (stage1Button != null) return;

        // Create container for Stage Selector below character cards
        GameObject stageBarRoot = new GameObject("StageSelectorBar", typeof(RectTransform));
        stageBarRoot.transform.SetParent(transform, false);
        RectTransform barRect = stageBarRoot.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0.18f);
        barRect.anchorMax = new Vector2(0.5f, 0.18f);
        barRect.pivot = new Vector2(0.5f, 0.5f);
        barRect.anchoredPosition = new Vector2(0f, 0f);
        barRect.sizeDelta = new Vector2(620f, 75f);

        // Stage Buttons Row
        string[] stageNames = new string[] { "STAGE 1\n(Armory)", "STAGE 2\n(Energy Lab)", "STAGE 3\n(Tech Vault)" };
        Button[] btns = new Button[3];
        Image[] highlights = new Image[3];

        float startX = -200f;
        float spacingX = 200f;

        for (int i = 0; i < 3; i++)
        {
            int stageNum = i + 1;
            GameObject btnGO = new GameObject($"Stage{stageNum}Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(stageBarRoot.transform, false);

            RectTransform bRect = btnGO.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0.5f);
            bRect.anchorMax = new Vector2(0.5f, 0.5f);
            bRect.pivot = new Vector2(0.5f, 0.5f);
            bRect.anchoredPosition = new Vector2(startX + i * spacingX, 10f);
            bRect.sizeDelta = new Vector2(170f, 40f);

            Image img = btnGO.GetComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.28f, 0.95f);
            highlights[i] = img;

            Button btn = btnGO.GetComponent<Button>();
            btn.onClick.AddListener(() => SelectStage(stageNum));
            btns[i] = btn;

            // Label
            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(btnGO.transform, false);
            RectTransform tRect = textGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = stageNames[i];
            tmp.fontSize = 11f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        stage1Button = btns[0];
        stage2Button = btns[1];
        stage3Button = btns[2];
        stage1Highlight = highlights[0];
        stage2Highlight = highlights[1];
        stage3Highlight = highlights[2];

        // Stage Info Text below buttons
        GameObject infoGO = new GameObject("StageInfoText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        infoGO.transform.SetParent(stageBarRoot.transform, false);
        RectTransform infoRect = infoGO.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.5f, 0.5f);
        infoRect.anchorMax = new Vector2(0.5f, 0.5f);
        infoRect.pivot = new Vector2(0.5f, 0.5f);
        infoRect.anchoredPosition = new Vector2(0f, -28f);
        infoRect.sizeDelta = new Vector2(600f, 32f);

        stageInfoText = infoGO.GetComponent<TextMeshProUGUI>();
        stageInfoText.fontSize = 11f;
        stageInfoText.alignment = TextAlignmentOptions.Center;
        stageInfoText.color = Color.white;
    }
}
