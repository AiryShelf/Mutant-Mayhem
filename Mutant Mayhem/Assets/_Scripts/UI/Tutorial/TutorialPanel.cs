using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPanel : MonoBehaviour
{
    void Start()
    {
        if (SettingsManager.TutorialDisabled == true)
        {
            Destroy(gameObject);
        }

        Time.timeScale = 0;
    }

    public virtual void OnOKButtonClick()
    {
        Time.timeScale = 1;
        Destroy(gameObject);
    }

    public virtual void OnDisableButtonClick()
    {
        Time.timeScale = 1;
        SettingsManager.TutorialDisabled = true;
        Destroy(gameObject);
    }
}
