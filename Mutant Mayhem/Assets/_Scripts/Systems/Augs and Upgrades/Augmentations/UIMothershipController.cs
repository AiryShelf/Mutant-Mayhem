using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMothershipController : MonoBehaviour
{
    [SerializeField] FadeCanvasGroupsWave areYouSurePanel;
    [SerializeField] List<GraphicRaycaster> graphicRaycasters;

    UIAugPanel augPanel;
    int qualityLevelStart;

    void Start()
    {
        Application.targetFrameRate = 60;
        qualityLevelStart = QualitySettings.GetQualityLevel();
        //QualitySettings.SetQualityLevel(3); // High quality
        
        CursorManager.Instance.inMenu = true;
        TouchManager.Instance.SetVirtualJoysticksActive(false);
        AugManager.Instance.Initialize();

        //InputController.SetLastUsedDevice(null);
        InputManager.SetJoystickMouseControl(true);
        CursorManager.Instance.SetGraphicRaycasters(graphicRaycasters);
        // Hide custom cursor
        if (InputManager.LastUsedDevice == Touchscreen.current)
            CursorManager.Instance.MoveCustomCursorTo(new Vector2(0, 0), CursorRangeType.Bounds, Vector2.zero, 0, new Rect(0, 0, 1, 1));
        
        
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

        //QualitySettings.SetQualityLevel(qualityLevelStart); // High quality
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
