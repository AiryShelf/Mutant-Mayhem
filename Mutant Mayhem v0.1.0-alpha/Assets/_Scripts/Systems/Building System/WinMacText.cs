using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinMacText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textToReplace;
    [TextArea(5,10)]
    [SerializeField] string replacementText;

    void Start()
    {
        // Check for MacOS
        if (Application.platform == RuntimePlatform.OSXPlayer || 
            Application.platform == RuntimePlatform.OSXEditor)
        {
            // Display controls for macOS users
            textToReplace.text = replacementText;
        }
    }
}
