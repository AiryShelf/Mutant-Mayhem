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

    void Start()
    {
        PlanetManager.Instance.SetCurrentPlanet(ProfileManager.Instance.currentProfile.lastPlanetVisited);
        
        CursorManager.Instance.inMenu = true;
        TouchManager.Instance.SetVirtualJoysticksActive(false);
        AugManager.Instance.Initialize();
        AugManager.Instance.RefreshCurrentRP();

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

    IEnumerator LaunchGameCoroutine()
    {
        // 1) Start loading your game scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(3, LoadSceneMode.Single);
        
        // 2) Wait until scene load has finished
        while (!asyncLoad.isDone)
            yield return null;

        // 3) Now unload unused assets to free memory
        yield return Resources.UnloadUnusedAssets();

        // 4) Force garbage collection on the CPU side
        System.GC.Collect();
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
        StartCoroutine(LaunchGameCoroutine());
    }

    public void OnConfirmLaunch()
    {
        augPanel.TrackRPCosts();

        SceneManager.LoadScene(3);
    }

    public void OnReturnToMenu()
    {
        SceneManager.LoadScene(1);
    }
}
