using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        // Zapisz nazwê sceny do za³adowania w PlayerPrefs
        PlayerPrefs.SetString("SceneToLoad", "SampleScene");
        SceneManager.LoadScene("LoadingBar");
        // lub: SceneManager.LoadScene(2); // index z Build Settings
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("3_Settings");
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
