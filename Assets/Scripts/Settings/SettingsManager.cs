using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.InputSystem;
public class SettingsManager : MonoBehaviour
{
    // 讀Settings檔的Data
    [Header("Data")]
    public SettingsData settings;

    // 調整輸入鍵
    // code here

    // 宣告UI滑條(AgentSpeed)
    [Header("AgentSpeed")]
    public Slider agentSpeedSlider;

    //設定檔的路徑
    private string settingsFilePath;

    void Awake()
    {
        settingsFilePath = Path.Combine(Application.persistentDataPath, "gamesettings.json");
        LoadSettings();
    }
    void Start()
    {
        if (agentSpeedSlider != null)
        {
            agentSpeedSlider.value = settings.agentSpeed;
            agentSpeedSlider.onValueChanged.AddListener(SetAgentSpeed);
        }
    }

    // 這邊會被滑條呼叫
    public void SetAgentSpeed(float value)
    {
        settings.agentSpeed = value;
        ApplyAgentSpeed();
    }

    private void ApplyAgentSpeed()
    {
        // 先確保有沒有這個Script
        HeuristicAgent heuristicAgent = FindFirstObjectByType<HeuristicAgent>();
        if (heuristicAgent != null)
        {
            heuristicAgent.stepInterval = settings.agentSpeed;
        }
        else
        {
            // do nothing.
        }
    }

    private void ApplyInput()
    {
        // code here
    }
    public void SaveSettings()
    {
        // save Input code here
        // 轉換設定成 JSON 檔
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(settingsFilePath, json);
        Debug.Log("Settings saved!");
    }

    public void LoadSettings()
    {
        if (File.Exists(settingsFilePath))
        {
            string json = File.ReadAllText(settingsFilePath);

            JsonUtility.FromJsonOverwrite(json, settings);
            Debug.Log("Settings loaded!");
        }
        else
        {
            Debug.Log("No settings file found. Using default settings.");
        }


        ApplyAllSettings();
    }


    private void ApplyAllSettings()
    {
        ApplyAgentSpeed();
        ApplyInput();
    }
}