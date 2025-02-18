using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMothershipController : MonoBehaviour
{
    [SerializeField] FadeCanvasGroupsWave areYouSurePanel;
    [SerializeField] List<GraphicRaycaster> graphicRaycasters;

    UIAugPanel augPanel;
    

    void Start()
    {
        Application.targetFrameRate = 60;
        
        AugManager.Instance.Initialize();
        //InputController.SetLastUsedDevice(null);
        InputController.SetJoystickMouseControl(true);
        CursorManager.Instance.SetGraphicRaycasters(graphicRaycasters);
        
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
        if (AugManager.selectedAugsWithLvls.Count < 1)
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
