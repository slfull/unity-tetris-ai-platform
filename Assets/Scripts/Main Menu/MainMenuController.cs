using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void AIvsAI()
    {
        SceneManager.LoadScene("TetrisLocalColdClearAndHAgent");
    }

    public void PlayervsCC()
    {
        SceneManager.LoadScene("TetrisLocalPlayerAndColdClear");
    }
    public void PlaySinglePlayer()
    {
        SceneManager.LoadScene("TetrisSingle");
    }

    public void PlayWithAI()
    {
        SceneManager.LoadScene("TetrisLocalPlayerAndHAgent");
    }

    public void PlayMultiplayer()
    {
        SceneManager.LoadScene("TetrisLocalTwoPlayer");
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
