using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WelcomePanel : MonoBehaviour
{
    void Awake()
    {          
        if (SettingsManager.Welcomed == true)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Time.timeScale = 0;
    }

    public void OnButtonClick()
    {
        Time.timeScale = 1;
        SettingsManager.Welcomed = true;
        Destroy(gameObject);
    }
}
