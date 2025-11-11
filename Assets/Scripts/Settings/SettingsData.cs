using UnityEngine;

[CreateAssetMenu(fileName = "SettingsData", menuName = "Settings/SettingsData")]
public class SettingsData : ScriptableObject
{
    //設定架構，等下加更多設定就複製下面的做
    [Header("AgentSpeed")]
    [Range(0.01f, 2f)]
    public float agentSpeed = 0.3f;

    // 調整輸入鍵
    // code here
}
