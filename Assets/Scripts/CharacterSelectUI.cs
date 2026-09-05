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

    [Header("Action Buttons")]
    [SerializeField] private Button deployButton;
    [SerializeField] private Button backButton;

    [Header("Menu Manager")]
    [SerializeField] private MenuManager menuManager;

    private Color activeHighlightColor = new Color(0.2f, 0.9f, 0.3f, 1f);
    private Color inactiveHighlightColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    private void Awake()
    {
        if (charAButton != null) charAButton.onClick.AddListener(() => Select(CharacterType.CharacterA_Roll));
        if (charBButton != null) charBButton.onClick.AddListener(() => Select(CharacterType.CharacterB_DoubleJump));
        if (charCButton != null) charCButton.onClick.AddListener(() => Select(CharacterType.CharacterC_Teleport));

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
    }

    private void OnEnable()
    {
        Select(CharacterSelection.SelectedCharacter);
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
}
