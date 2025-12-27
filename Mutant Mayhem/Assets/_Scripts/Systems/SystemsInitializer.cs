using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SystemsInitializer : MonoBehaviour
{
    public void InitializeLevelStart(Player player)
    {
        // Camera and Settings
        CameraController.Instance.Initialize();
        StartCoroutine(ForceCanvasUpdate());
        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        SettingsManager.Instance.ApplyGameplaySettings();
        CameraController.Instance.ZoomAndFocus(player.transform, 1, 1, 2f, false, false);

        // Touch and Cursor
        StartCoroutine(DelayScreenBoundReset());
        TouchManager.Instance.player = player;
        TouchManager.Instance.buildMenuController = player.stats.structureStats.buildingSystem.buildMenuController;
        TouchManager.Instance.buildPanelRect = TouchManager.Instance.buildMenuController.transform as RectTransform;
        CursorManager.Instance.Initialize();
        CursorManager.Instance.inMenu = false;
        CursorManager.Instance.MoveCustomCursorWorldToUi(transform.position);
        CursorManager.Instance.SetGraphicRaycasters(player.graphicRaycasters);

        // Virtual Joysticks
        if (InputManager.LastUsedDevice == Touchscreen.current)
        {
            TouchManager.Instance.ShowVirtualAimJoysticks(true);

            if (PlanetManager.Instance.currentPlanet.isTutorialPlanet)
            {
                // Force virtual aim joystick visible on tutorial planet
                SettingsManager.Instance.isVirtualAimJoystickVisible = true;
                TouchManager.Instance.SetVirtualAimJoystickVisible(true);
                TouchManager.Instance.ShowVirtualAimJoysticks(true);
            }
        }

        // Joystick Control
        InputManager.SetJoystickMouseControl(!SettingsManager.Instance.useInstantJoystickAim);
        LinkVirtualJoysticks(player);
        player.aimDistance = CursorManager.Instance.aimDistance;
        player.aimMinDist = CursorManager.Instance.aimMinDistance;

        // Time Control and Frame Rate
        TimeControl.Instance.SubscribePlayerTimeControl(player);
        TimeControl.Instance.ResetTimeScale();
        if (InputManager.IsMobile())
            Application.targetFrameRate = 60;
        else
            Application.targetFrameRate = 120;

        

        // Systems Initialization
        AudioManager.Instance.Initialize();
        StatsCounterPlayer.ResetStatsCounts();
        
        TurretManager.Instance.Initialize(player);
        UpgradeManager.Instance.Initialize();
        ClassManager.Instance.ApplyClassEffects(player);
        AugManager.Instance.ApplySelectedAugmentations();
        PlanetManager.Instance.ApplyPlanetProperties();

        WaveController.Instance.StartWaveSequence();
        ScreenScaleChecker.InvokeAspectRatioChanged();
        MessageManager.Instance.StartPlanetDialogue();

        // Track session_start
        FpsCounter.Instance?.ResetSnapshot();
        AnalyticsManager.Instance.TrackSessionStart();
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
