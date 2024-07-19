using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBuildPanel : TutorialPanel
{
    void Awake()
    {
        if (TutorialManager.tutorialShowedBuild)
        {
            Destroy(gameObject);
        }
    }

    public override void OnOKButtonClick()
    {
        TutorialManager.tutorialShowedBuild = true;
        base.OnOKButtonClick();
    }
}
