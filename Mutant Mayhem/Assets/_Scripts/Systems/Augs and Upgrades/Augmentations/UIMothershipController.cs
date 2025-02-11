using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMothershipController : MonoBehaviour
{
    [SerializeField] FadeCanvasGroupsWave areYouSurePanel;

    UIAugPanel augPanel;
    

    void Start()
    {
        AugManager.Instance.Initialize();
        
        augPanel = FindObjectOfType<UIAugPanel>();
        augPanel.Initialize();
        
        StartCoroutine(DelayInit());

        //AugManager.Instance.Initialize();
    }

    IEnumerator DelayInit()
    {
        yield return new WaitForFixedUpdate();

        augPanel.RefreshRPCosts();
        augPanel.UpdatePanelTextandButtons();
        augPanel.UpdateUIAugs();
    }

    public void OnLaunch()
    {
        if (AugManager.Instance.selectedAugsWithLvls.Count < 1)
        {
            areYouSurePanel.isTriggered = true;
            return;
        }

        augPanel.TrackRPCosts();

        SceneManager.LoadScene(2);
    }

    public void OnConfirmLaunch()
    {
        augPanel.TrackRPCosts();

        SceneManager.LoadScene(2);
    }

    public void OnReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
