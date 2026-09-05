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
            dummyProgressText.text = $"Dummies: <color=#55FF55>{current}</color> / {required}";
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
                statsDummiesKilledText.text = $"Dummies Killed: {dummiesKilled}";
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
}
