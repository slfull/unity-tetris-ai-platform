using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class RebindButton : MonoBehaviour
{
    [SerializeField] private string actionMap = "Player";
    [SerializeField] private string actionName = "P1Move";
    [SerializeField] private int bindingIndex = 0;
    [SerializeField] private TextMeshProUGUI displayText;
    private Button rebindButton;

    private InputRebindManager rebindManager;

    private void Start()
    {
        rebindManager = InputRebindManager.Instance;
        rebindButton = gameObject.GetComponent<Button>();
        rebindButton.onClick.AddListener(OnRebindClicked);
        UpdateLabel();
    }

    private void OnRebindClicked()
    {
        displayText.text = "Press Button...";
        rebindManager.StartRebind(actionMap, actionName, bindingIndex, UpdateLabel);
    }

    private void UpdateLabel()
    {
        var action = rebindManager.Input.asset.FindActionMap(actionMap).FindAction(actionName);
        if (action != null)
        {
            if (actionName == "P1Move")
            {
                string dirText = "";
                if (bindingIndex == 1)
                {
                    dirText = "P1Up";
                }
                else if (bindingIndex == 2)
                {
                    dirText = "P1Down";
                }
                else if (bindingIndex == 3)
                {
                    dirText = "P1Left";
                }
                else
                {
                    dirText = "P1Right";
                }
                displayText.text = $"{dirText}: {action.GetBindingDisplayString(bindingIndex)}";
            }
            else if (actionName == "P2Move")
            {
                string dirText = "";
                if (bindingIndex == 1)
                {
                    dirText = "P2Up";
                }
                else if (bindingIndex == 2)
                {
                    dirText = "P2Down";
                }
                else if (bindingIndex == 3)
                {
                    dirText = "P2Left";
                }
                else
                {
                    dirText = "P2Right";
                }
                displayText.text = $"{dirText}: {action.GetBindingDisplayString(bindingIndex)}";
            }
            else if (actionName == "P1Rotate")
            {
                string dirText = "";
                if (bindingIndex == 1)
                {
                    dirText = "P1CCW";
                }
                else
                {
                    dirText = "P1CW";
                }
                displayText.text = $"{dirText}: {action.GetBindingDisplayString(bindingIndex)}";
            }
            else if(actionName == "P2Rotate")
            {
                string dirText = "";
                if (bindingIndex == 1)
                {
                    dirText = "P2CCW";
                }
                else
                {
                    dirText = "P2CW";
                }
                displayText.text = $"{dirText}: {action.GetBindingDisplayString(bindingIndex)}";
            }
            else
            {
                displayText.text = $"{actionName}: {action.GetBindingDisplayString(bindingIndex)}";
            }
        }            
    }
}
