using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void AIvsAI()
    {
        SceneManager.LoadScene("TetrisLocalColdClearAndHAgent");
    }
    public void AInormal()
    {
        SceneManager.LoadScene("TetrisLocalPlayerAndHAgent");
    }

    public void AIexpert()
    {
        SceneManager.LoadScene("TetrisLocalPlayerAndColdClear");
    }

    public void Multiplayer()
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
