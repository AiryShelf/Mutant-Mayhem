using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SystemsInitializer : MonoBehaviour
{
    public void InitializeLevelStart(Player player)
    {
        StartCoroutine(ForceCanvasUpdate());
        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        SettingsManager.Instance.ApplyGameplaySettings();

        StartCoroutine(DelayScreenBoundReset());
        TouchManager.Instance.player = player;
        TouchManager.Instance.buildMenuController = player.stats.structureStats.buildingSystem.buildMenuController;
        TouchManager.Instance.buildPanelRect = TouchManager.Instance.buildMenuController.transform as RectTransform;
        TouchManager.Instance.upgradePanelSwitcher = player.stats.structureStats.cubeController.panelSwitcher;
        TouchManager.Instance.upgradePanelRect = player.stats.structureStats.cubeController.backPanel;
        CursorManager.Instance.Initialize();
        CursorManager.Instance.inMenu = false;
        CursorManager.Instance.MoveCustomCursorWorldToUi(transform.position);
        CursorManager.Instance.SetGraphicRaycasters(player.graphicRaycasters);
        if (InputManager.LastUsedDevice == Touchscreen.current)
            TouchManager.Instance.SetVirtualJoysticksActive(true);
        InputManager.SetJoystickMouseControl(!SettingsManager.Instance.useFastJoystickAim);
        LinkVirtualJoysticks(player);
        player.aimDistance = CursorManager.Instance.aimDistance;
        player.aimMinDist = CursorManager.Instance.aimMinDistance;

        TimeControl.Instance.SubscribePlayerTimeControl(player);
        TimeControl.Instance.ResetTimeScale();
        if (InputManager.IsMobile())
            Application.targetFrameRate = 60;
        else
            Application.targetFrameRate = 120;
        
        SFXManager.Instance.Initialize();
        StatsCounterPlayer.ResetStatsCounts();

        TurretManager.Instance.Initialize(player);
        UpgradeManager.Instance.Initialize();
        ClassManager.Instance.ApplyClassEffects(player);
        AugManager.Instance.ApplySelectedAugmentations();
        PlanetManager.Instance.ApplyPlanetProperties();
        
        FindObjectOfType<WaveControllerRandom>().Initialize();
        
        ScreenScaleChecker.InvokeAspectRatioChanged();
    }

    IEnumerator ForceCanvasUpdate()
    {
        // Wait for the end of the frame to ensure everything is initialized.
        yield return new WaitForSeconds(2);
        Canvas.ForceUpdateCanvases();
    }

    IEnumerator DelayScreenBoundReset()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        TouchManager.Instance.RefreshScreenBounds();
    }

    void LinkVirtualJoysticks(Player player)
    {
        VirtualJoystick[] allVirtualJoysticks = FindObjectsOfType<VirtualJoystick>(true);

        foreach (var stick in allVirtualJoysticks)
        {
            stick.animControllerPlayer = player.animControllerPlayer;
            stick.player = player;
        }
    }
}
