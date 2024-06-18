using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBuildPanel : TutorialPanel
{
    void Awake()
    {
        if (SettingsManager.tutorialShowedBuild)
        {
            Destroy(gameObject);
        }
    }

    public override void OnOKButtonClick()
    {
        SettingsManager.tutorialShowedBuild = true;
        base.OnOKButtonClick();
    }
}
