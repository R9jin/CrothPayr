using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Tooltip("Must exactly match the scene name in File > Build Settings")]
    [SerializeField] private string gameSceneName = "Level1";

    [Header("Character Selection UI")]
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private GameObject mainButtonsPanel;

    [Header("Audio")]
    [SerializeField] private AudioClip themeSongClip;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

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
        SceneManager.LoadScene(gameSceneName);
    }

    // Not wired yet - placeholder for when you build the settings panel
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