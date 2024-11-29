using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionText : MonoBehaviour
{
    string version;
    TextMeshProUGUI versionText;

    void Start()
    {
        version = Application.version;
        versionText = GetComponent<TextMeshProUGUI>();
        versionText.text = version;
    }
}
