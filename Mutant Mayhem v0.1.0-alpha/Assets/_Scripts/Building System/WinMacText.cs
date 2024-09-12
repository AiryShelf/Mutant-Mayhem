using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinMacText : MonoBehaviour
{
    [TextArea(5,10)]
    [SerializeField] string replacementText;

    [SerializeField] TextMeshProUGUI TMPText;

    void Start()
    {
        // Check for MacOS
        if (Application.platform == RuntimePlatform.OSXPlayer || 
            Application.platform == RuntimePlatform.OSXEditor)
        {
            // Display controls for macOS users
            TMPText.text = replacementText;
        }
    }
}
