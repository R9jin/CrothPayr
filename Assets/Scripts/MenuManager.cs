using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Tooltip("Must exactly match the scene name in File > Build Settings")]
    [SerializeField] private string gameSceneName = "Level1";

    [Header("Character Selection UI")]
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private GameObject mainButtonsPanel;

    [Header("Audio")]
    [SerializeField] private AudioClip themeSongClip;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("Level / Stage Progression")]
    [SerializeField] private int selectedStage = 1;

    public int SelectedStage => selectedStage;
    public string GameSceneName => gameSceneName;

    private void Awake()
    {
        Instance = this;

        // Restore last selected stage or default to Stage 1
        selectedStage = PlayerPrefs.GetInt("SelectedStage", 1);
        SetSelectedStage(selectedStage);
    }

    private void Start()
    {
        // Setup and play theme song (2D menu music, looping)
        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (musicAudioSource != null && themeSongClip != null)
        {
            musicAudioSource.clip = themeSongClip;
            musicAudioSource.loop = true;
            musicAudioSource.playOnAwake = false;
            musicAudioSource.spatialBlend = 0f; // 2D non-diegetic background music
            musicAudioSource.volume = musicVolume;
            if (!musicAudioSource.isPlaying)
            {
                musicAudioSource.Play();
            }
        }

        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(false);
        }
        if (mainButtonsPanel != null)
        {
            mainButtonsPanel.SetActive(true);
        }
    }

    public void SetSelectedStage(int stageNumber)
    {
        selectedStage = Mathf.Clamp(stageNumber, 1, 3);
        gameSceneName = "Level" + selectedStage;
        PlayerPrefs.SetInt("SelectedStage", selectedStage);
        PlayerPrefs.Save();
    }

    // Wired to the START button's OnClick()
    public void PlayGame()
    {
        if (characterSelectPanel != null)
        {
            OpenCharacterSelect();
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void OpenCharacterSelect()
    {
        if (characterSelectPanel != null) characterSelectPanel.SetActive(true);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
    }

    public void CloseCharacterSelect()
    {
        if (characterSelectPanel != null) characterSelectPanel.SetActive(false);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
    }

    public void SelectCharacter(int characterIndex)
    {
        CharacterSelection.SelectedCharacter = (CharacterType)characterIndex;
    }

    public void ConfirmAndStartGame()
    {
        // Ensure scene name is up to date with selected stage
        gameSceneName = "Level" + selectedStage;
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        Debug.Log("Settings not implemented yet.");
    }

    // Wired to the EXIT button's OnClick()
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}