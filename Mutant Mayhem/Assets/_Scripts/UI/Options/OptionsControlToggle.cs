using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsControlToggle : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    [SerializeField] KeyCode keyCode;
    bool isKeyActive;

    public void OnValueChanged(bool isOn)
    {
        isKeyActive = isOn;
    }
}
