using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Tooltip("Must exactly match the scene name in File > Build Settings")]
    [SerializeField] private string gameSceneName = "Game";

    // Wired to the START button's OnClick()
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Not wired yet — placeholder for when you build the settings panel
    public void OpenSettings()
    {
        Debug.Log("Settings not implemented yet.");
    }

    // Wired to the EXIT button's OnClick()
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Application.Quit() does nothing while testing inside the Editor,
        // so this stops Play Mode instead so you can actually see it work.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}