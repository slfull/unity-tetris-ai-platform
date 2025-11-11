using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel; // Drag your SettingsPanel here
    private Animator settingsAnimator;

    void Start()
    {
        // Get the Animator from the panel
        if (settingsPanel != null)
        {
            settingsAnimator = settingsPanel.GetComponent<Animator>();
        }
    }

    // This is called by your main "Options" button's OnClick() event
    public void OpenSettings()
    {
        if (settingsAnimator != null)
        {
            // 1. Enable the panel object
            settingsPanel.SetActive(true);
            
            // 2. Trigger the "SlideIn" animation
            settingsAnimator.SetTrigger("SlideIn");
            
            // 3. Pause the game
            Time.timeScale = 0f;
        }
    }

    // This is called by the "Back" button INSIDE the SettingsPanel
    public void CloseSettings()
    {
        if (settingsAnimator != null)
        {
            // 1. Un-pause the game
            Time.timeScale = 1f;
            
            // 2. Trigger the "SlideOut" animation
            settingsAnimator.SetTrigger("SlideOut");

            // Optional: You could add a small delay (e.g., 0.5s)
            // and then set settingsPanel.SetActive(false)
            // to disable the object after it's off-screen.
        }
    }
}
