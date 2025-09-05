using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{   
    public void StartSinglePlayer()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
