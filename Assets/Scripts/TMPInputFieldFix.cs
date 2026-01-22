using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TMPInputFieldFix : MonoBehaviour
{
    public TMP_InputField inputField;

    void Start()
    {
        inputField.onSelect.AddListener(OnSelectInput);
    }

    void OnSelectInput(string text)
    {
        TouchScreenKeyboard.hideInput = true;
    }
}
