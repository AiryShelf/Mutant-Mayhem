using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPanel : MonoBehaviour
{
    Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();

        if (TutorialManager.TutorialDisabled == true)
        {
            Destroy(gameObject);
            return;
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
        TutorialManager.TutorialDisabled = true;
        player.inputAsset.FindActionMap("Player").Enable();
        Destroy(gameObject);
    }
}
