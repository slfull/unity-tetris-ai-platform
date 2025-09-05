using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game"; // 改成你的遊戲場景名稱

    public void PlaySinglePlayer()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void PlayWithAI()
    {
        Debug.Log("Play With AI clicked");
    }

    public void PlayMultiplayer()
    {
        Debug.Log("Multiplayer clicked");
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
