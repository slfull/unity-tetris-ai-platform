using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void AIvsAI()
    {
        SceneManager.LoadScene();
    }
    public void PlaySinglePlayer()
    {
        SceneManager.LoadScene();
    }

    public void PlayWithAI()
    {
        SceneManager.LoadScene();
    }

    public void PlayMultiplayer()
    {
        SceneManager.LoadScene();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
