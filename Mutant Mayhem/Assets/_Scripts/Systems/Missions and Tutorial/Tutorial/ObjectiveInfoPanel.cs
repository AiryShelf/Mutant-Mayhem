using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveInfoPanel : InfoPanel
{
    void Start()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
    public override void OnOKButtonClick()
    {
        base.OnOKButtonClick();
    }
}