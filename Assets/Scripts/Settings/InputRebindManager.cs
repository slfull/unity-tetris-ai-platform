using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mirror.Examples.Basic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InputRebindManager : MonoBehaviour
{
    public static InputRebindManager Instance { get; private set; }

    public PlayerInput Input { get; private set; }
    private const string RebindSaveFile = "rebind.json";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Input = new PlayerInput();
        LoadRebinds();
        Input.Enable();
    }

    public void StartRebind(string actionMapName, string actionName, int bindingIndex = 0, Action onComplete = null)
    {
        InputActionMap map = Input.asset.FindActionMap(actionMapName);
        if (map == null)
        {
            Debug.LogWarning($"[Input Rebind] no action map {actionName}");
            return;
        }

        InputAction action = map.FindAction(actionName);
        if (action == null)
        {
            Debug.LogWarning($"[Input Rebind] no action{actionName}");
            return;
        }

        Debug.Log($"[Input Rebind] start rebind {actionName}");
        map.Disable();

        action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                op.Dispose();
                map.Enable();
                SaveRebinds();
                Debug.Log($"[Input Rebind] {actionName} rebind complete {action.bindings[bindingIndex].effectivePath}");
                onComplete?.Invoke();
            })
            .Start();
    }

    public void SaveRebinds()
    {
        string json = Input.asset.SaveBindingOverridesAsJson();
        string path = Path.Combine(Application.persistentDataPath, RebindSaveFile);
        File.WriteAllText(path, json);
        Debug.Log($"[Input Rebind] save rebind");
    }

    public void LoadRebinds()
    {
        string path = Path.Combine(Application.persistentDataPath, RebindSaveFile);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Input.asset.LoadBindingOverridesFromJson(json);
            Debug.Log("[Input Rebind] load rebind");
        }
    }
    
    public void ResetAllBindings()
    {
        Input.asset.RemoveAllBindingOverrides();
        SaveRebinds();
        Debug.Log("[Input Rebind] reset rebind");
    }
}
