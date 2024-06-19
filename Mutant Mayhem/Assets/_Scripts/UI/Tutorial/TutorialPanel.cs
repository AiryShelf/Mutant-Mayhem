using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPanel : MonoBehaviour
{
    Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();

        if (SettingsManager.TutorialDisabled == true)
        {
            Destroy(gameObject);
        }
        else
        {
            Time.timeScale = 0;
            player.inputAsset.FindActionMap("Player").Disable();
        }
    }

    public virtual void OnOKButtonClick()
    {
        Time.timeScale = 1;
        player.inputAsset.FindActionMap("Player").Enable();
        Destroy(gameObject);
    }

    public virtual void OnDisableButtonClick()
    {
        Time.timeScale = 1;
        SettingsManager.TutorialDisabled = true;
        player.inputAsset.FindActionMap("Player").Enable();
        Destroy(gameObject);
    }
}
