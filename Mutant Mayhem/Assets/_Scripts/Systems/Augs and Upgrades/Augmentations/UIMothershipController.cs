using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMothershipController : MonoBehaviour
{
    [SerializeField] FadeCanvasGroupsWave areYouSurePanel;
    void Start()
    {
        //AugManager.Instance.Initialize();
    }

    public void OnLaunch()
    {
        if (AugManager.Instance.selectedAugsWithLvls.Count < 1)
        {
            areYouSurePanel.isTriggered = true;
            return;
        }

        SceneManager.LoadScene(2);
    }

    public void OnConfirmLaunch()
    {
        SceneManager.LoadScene(2);
    }

    public void OnReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
