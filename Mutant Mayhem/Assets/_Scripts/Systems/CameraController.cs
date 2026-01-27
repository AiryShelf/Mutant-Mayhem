using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] CinemachineVirtualCamera mouseLookerCamera;
    [SerializeField] CinemachineMixingCamera mixingCamera;
    [SerializeField] MouseLooker mouseLooker;
    [SerializeField] Player player;
    [SerializeField] Transform qCubeTrans;
    [SerializeField] float deathLerpTime = 2f;
    [SerializeField] float touchscreenXOffset = 3;
    [SerializeField] float wideTouchscreenZoomBias = -2f;

    CinemachineFramingTransposer playerFramingTransposer;
    CinemachineFramingTransposer mouseFramingTransposer;

    float playerCamOrthoSizeStart;
    float mouseLookerCamOrthoSizeStart;
    float playerDZWidth;
    float playerDZHeight;
    float mouseDZWidth;
    float mouseDZHeight;
    float playerSZWidth;
    float playerSZHeight;
    float mouseSZWidth;
    float mouseSZHeight;
    float playerXDamping;
    float playerYDamping;
    float mouseXDamping;
    float mouseYDamping;
    float playerMixWeight;

    [Header("Dynamic, don't set here")]
    public float mouseMixWeight;
    public float mouseMixWeightStart;
    public bool alwaysLockToPlayer;

    Coroutine weightLerpCoroutine1;
    Coroutine weightLerpCoroutine2;
    Coroutine positionLerpCoroutine;
    Coroutine orthoSizeLerpCoroutine;
    Coroutine orthoSizeLerpCoroutine2;
    Coroutine playerDampingCoroutine1;
    Coroutine playerDampingCoroutine2;
    Coroutine mouseDampingCoroutine1;
    Coroutine mouseDampingCoroutine2;
    Coroutine playerDzCoroutine1;
    Coroutine playerDzCoroutine2;
    Coroutine mouseDzCoroutine1;
    Coroutine mouseDzCoroutine2;
    Coroutine playerSzCoroutine1;
    Coroutine playerSzCoroutine2;
    Coroutine mouseSzCoroutine1;
    Coroutine mouseSzCoroutine2;

    bool deathZoomStarted;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        playerFramingTransposer = playerCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        mouseFramingTransposer = mouseLookerCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        // Store defaults
        playerCamOrthoSizeStart = playerCamera.m_Lens.OrthographicSize;
        mouseLookerCamOrthoSizeStart = mouseLookerCamera.m_Lens.OrthographicSize;

        playerDZWidth = playerFramingTransposer.m_DeadZoneWidth;
        playerDZHeight = playerFramingTransposer.m_DeadZoneHeight;
        mouseDZWidth = mouseFramingTransposer.m_DeadZoneWidth;
        mouseDZHeight = mouseFramingTransposer.m_DeadZoneHeight;

        playerSZWidth = playerFramingTransposer.m_SoftZoneWidth;
        playerSZHeight = playerFramingTransposer.m_SoftZoneHeight;
        mouseSZWidth = mouseFramingTransposer.m_SoftZoneWidth;
        mouseSZHeight = mouseFramingTransposer.m_SoftZoneHeight;

        playerXDamping = playerFramingTransposer.m_XDamping;
        playerYDamping = playerFramingTransposer.m_YDamping;
        mouseXDamping = mouseFramingTransposer.m_XDamping;
        mouseYDamping = mouseFramingTransposer.m_YDamping;

        playerMixWeight = mixingCamera.GetWeight(0);
        mouseMixWeight = mixingCamera.GetWeight(1);
        mouseMixWeightStart = mouseMixWeight;
    }

    void OnEnable()
    {
        Player.OnPlayerDestroyed += HandlePlayerDeath;
        QCubeController.OnCubeDestroyed += HandleCubeDeath;
    }

    void OnDisable()
    {
        StopAllTrackedCoroutines();
        ResetCameraSettings();
        Player.OnPlayerDestroyed -= HandlePlayerDeath;
        QCubeController.OnCubeDestroyed -= HandleCubeDeath;
    }

    public void Initialize()
    {
        TouchscreenAdjustments();
    }

    void TouchscreenAdjustments()
    {
        if (InputManager.LastUsedDevice != Touchscreen.current) return;

        SetTouchscreenOffset(true);

        // Adjust zoom bias based on aspect ratio
        float aspectRatio = (float)Screen.width / Screen.height;

        if (aspectRatio >= 2.0f)
            SettingsManager.Instance.zoomBiasTouchscreen = wideTouchscreenZoomBias;
        else if (aspectRatio > 1.85f)
            SettingsManager.Instance.zoomBiasTouchscreen = wideTouchscreenZoomBias;
        else if (aspectRatio >= 1.77f)
            SettingsManager.Instance.zoomBiasTouchscreen = wideTouchscreenZoomBias;
        else if (aspectRatio >= 1.5f) 
            SettingsManager.Instance.zoomBiasTouchscreen = wideTouchscreenZoomBias / 2f;
        else
            SettingsManager.Instance.zoomBiasTouchscreen = 0;
    }

    public void SetTouchscreenOffset(bool isOffset)
    {
        var framingTransposer = playerCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer != null)
        {
            if (isOffset)
            {
                // Only set offset if using touchscreen and virtual aim joystick is disabled
                if (InputManager.LastUsedDevice == Touchscreen.current && SettingsManager.Instance.isVirtualAimJoystickVisible == false)
                    framingTransposer.m_TrackedObjectOffset.x = touchscreenXOffset;
                else
                    framingTransposer.m_TrackedObjectOffset.x = 0;
            }
            else
            {
                framingTransposer.m_TrackedObjectOffset.x = 0;
            }
        }
    }

    #region Settings

    void ResetCameraSettings()
    {
        playerFramingTransposer.m_DeadZoneWidth = playerDZWidth;
        playerFramingTransposer.m_DeadZoneHeight = playerDZHeight;
        mouseFramingTransposer.m_DeadZoneWidth = mouseDZWidth;
        mouseFramingTransposer.m_DeadZoneHeight = mouseDZHeight;

        playerFramingTransposer.m_SoftZoneWidth = playerSZWidth;
        playerFramingTransposer.m_SoftZoneHeight = playerSZHeight;
        mouseFramingTransposer.m_SoftZoneWidth = mouseSZWidth;
        mouseFramingTransposer.m_SoftZoneHeight = mouseSZHeight;

        playerFramingTransposer.m_XDamping = playerXDamping;
        playerFramingTransposer.m_YDamping = playerYDamping;
        mouseFramingTransposer.m_XDamping = mouseXDamping;
        mouseFramingTransposer.m_YDamping = mouseYDamping;

        mixingCamera.SetWeight(0, playerMixWeight);
        mixingCamera.SetWeight(1, mouseMixWeight);
    }

    void HandlePlayerDeath(bool isDestroyed)
    {
        if (isDestroyed && !deathZoomStarted)
        {
            ZoomAndFocus(player.transform, 0.8f, 1f, deathLerpTime, true, false);
            deathZoomStarted = true;
            mouseLooker.deathTriggered = true;
        }
    }

    void HandleCubeDeath(bool isDestroyed)
    {
        if (isDestroyed && !deathZoomStarted)
        {
            ZoomAndFocus(qCubeTrans, 0.8f, 1f, deathLerpTime, false, true);
            deathZoomStarted = true;
            mouseLooker.deathTriggered = true;
        }
    }

    #endregion

    #region Zoom & Focus

    public void ZoomAndFocus(Transform targetTrans, float orthoZoomFactor, float dampingAmount, 
                                              float duration, bool focusPlayer, bool focusMouseLooker)
    {
        StopAllTrackedCoroutines();

        if (alwaysLockToPlayer)
        {
            focusPlayer = true;
            dampingAmount = 0;
        }

        // Adjust weights based on focus parameters
        if (focusPlayer)
        {
            weightLerpCoroutine1 = StartCoroutine(LerpCameraWeight(1f, 0, duration)); // Focus on player camera
            weightLerpCoroutine2 = StartCoroutine(LerpCameraWeight(0f, 1, duration)); // Unfocus mouseLooker camera
        }
        else if (focusMouseLooker)
        {
            weightLerpCoroutine1 = StartCoroutine(LerpCameraWeight(0f, 0, duration)); // Unfocus player camera
            weightLerpCoroutine2 = StartCoroutine(LerpCameraWeight(1f, 1, duration)); // Focus on mouseLooker camera
        }
        else
        {
            // If both are false, restore default weights
            weightLerpCoroutine1 = StartCoroutine(LerpCameraWeight(playerMixWeight, 0, duration));
            weightLerpCoroutine2 = StartCoroutine(LerpCameraWeight(mouseMixWeight, 1, duration));
        }

        // Lock camera to target, if a focus is chosen.
        LockCamerasToTarget(focusPlayer, focusMouseLooker);
        bool lockCameras = focusPlayer || focusMouseLooker;
        if (focusMouseLooker)
        {
            // Lerp mouseLooker to target
            positionLerpCoroutine = StartCoroutine(GameTools.LerpPosition(mouseLooker.transform,
            mouseLooker.transform.position, targetTrans.position, duration));
        }
        
        SetCameraDamping(dampingAmount, lockCameras);
 
        // Lerp ortho size (Zoom)
        if (orthoSizeLerpCoroutine != null)
            StopCoroutine(orthoSizeLerpCoroutine);
        if (orthoSizeLerpCoroutine2 != null)
            StopCoroutine(orthoSizeLerpCoroutine2);
        float currentOrthoPlayer = playerCamera.m_Lens.OrthographicSize;
        float currentOrthoMouse = mouseLookerCamera.m_Lens.OrthographicSize;
        orthoSizeLerpCoroutine = StartCoroutine(GameTools.LerpFloat(currentOrthoPlayer,
                                (playerCamOrthoSizeStart + SettingsManager.Instance.zoomBias) * 
                                orthoZoomFactor, duration, UpdateOrthoSizePlayer));
        orthoSizeLerpCoroutine2 = StartCoroutine(GameTools.LerpFloat(currentOrthoMouse,
                                (mouseLookerCamOrthoSizeStart + SettingsManager.Instance.zoomBias) * 
                                orthoZoomFactor, duration, UpdateOrthoSizeMouse));
    }

    void UpdateOrthoSizePlayer(float newSize)
    {
        playerCamera.m_Lens.OrthographicSize = newSize;
    }

    void UpdateOrthoSizeMouse(float newSize)
    {
        mouseLookerCamera.m_Lens.OrthographicSize = newSize;
    }

    #endregion

    #region Damping

    void SetCameraDamping(float dampingValue, bool lockCameras)
    {
        if (playerFramingTransposer == null || mouseFramingTransposer == null) return;

        if (lockCameras)
        {
            playerDampingCoroutine1 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_XDamping,
                dampingValue, 1f, value => playerFramingTransposer.m_XDamping = value));
            playerDampingCoroutine2 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_YDamping,
                dampingValue, 1f, value => playerFramingTransposer.m_YDamping = value));
            mouseDampingCoroutine1 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_XDamping,
                dampingValue, 1f, value => mouseFramingTransposer.m_XDamping = value));
            mouseDampingCoroutine2 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_YDamping,
                dampingValue, 1f, value => mouseFramingTransposer.m_YDamping = value));
        }
        else
        {
            // Return to default
            playerDampingCoroutine1 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_XDamping,
                playerXDamping, 1f, value => playerFramingTransposer.m_XDamping = value));
            playerDampingCoroutine2 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_YDamping,
                playerYDamping, 1f, value => playerFramingTransposer.m_YDamping = value));
            mouseDampingCoroutine1 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_XDamping,
                mouseXDamping, 1f, value => mouseFramingTransposer.m_XDamping = value));
            mouseDampingCoroutine2 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_YDamping,
                mouseYDamping, 1f, value => mouseFramingTransposer.m_YDamping = value));
        }
    }

    #endregion

    #region Lock Cameras

    void LockCamerasToTarget(bool playerLock, bool mouseLookerLock)
    {
        if (playerFramingTransposer == null || mouseFramingTransposer == null) 
        {
            Debug.Log("Player or MouseLooker's framing transposer not assigned");
            return;
        }

        // Apply Dead Zone and Soft Zone changes
        if (playerLock)
        {
            playerDzCoroutine1 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneWidth,
                0, 1f, value => playerFramingTransposer.m_DeadZoneWidth = value));
            playerDzCoroutine2 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneHeight,
                0, 1f, value => playerFramingTransposer.m_DeadZoneHeight = value));
            playerSzCoroutine1 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneWidth,
                0, 1f, value => playerFramingTransposer.m_SoftZoneWidth = value));
            playerSzCoroutine2 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneHeight,
                0, 1f, value => playerFramingTransposer.m_SoftZoneHeight = value));
        }
        else if (mouseLookerLock)
        {
            mouseDzCoroutine1 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneWidth,
                0, 1f, value => mouseFramingTransposer.m_DeadZoneWidth = value));
            mouseDzCoroutine2 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneHeight,
                0, 1f, value => mouseFramingTransposer.m_DeadZoneHeight = value));
            mouseSzCoroutine1 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneWidth,
                0, 1f, value => mouseFramingTransposer.m_SoftZoneWidth = value));
            mouseSzCoroutine2 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneHeight,
                0, 1f, value => mouseFramingTransposer.m_SoftZoneHeight = value));
        }
        else
        {
            // Return to default
            // Player zones
            playerDzCoroutine1 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneWidth,
                playerDZWidth, 1f, value => playerFramingTransposer.m_DeadZoneWidth = value));
            playerDzCoroutine2 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneHeight,
                playerDZHeight, 1f, value => playerFramingTransposer.m_DeadZoneHeight = value));
            playerSzCoroutine1 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneWidth,
                playerSZWidth, 1f, value => playerFramingTransposer.m_SoftZoneWidth = value));
            playerSzCoroutine2 = StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneHeight,
                playerSZHeight, 1f, value => playerFramingTransposer.m_SoftZoneHeight = value));

            // MouseLooker zones
            mouseDzCoroutine1 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneWidth,
                mouseDZWidth, 1f, value => mouseFramingTransposer.m_DeadZoneWidth = value));
            mouseDzCoroutine2 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneHeight,
                mouseDZHeight, 1f, value => mouseFramingTransposer.m_DeadZoneHeight = value));
            mouseSzCoroutine1 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneWidth,
                mouseSZWidth, 1f, value => mouseFramingTransposer.m_SoftZoneWidth = value));
            mouseSzCoroutine2 = StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneHeight,
                mouseSZHeight, 1f, value => mouseFramingTransposer.m_SoftZoneHeight = value));
        }
    }

    #endregion

    #region Coroutines

    IEnumerator LerpCameraWeight(float targetWeight, int cameraIndex, float duration)
    {
        if (mixingCamera == null || cameraIndex < 0 || cameraIndex >= mixingCamera.ChildCameras.Length)
        {
            Debug.LogWarning("Invalid camera index or Mixing Camera not set.");
            yield break;
        }

        float initialWeight = mixingCamera.GetWeight(cameraIndex);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float newWeight = Mathf.Lerp(initialWeight, targetWeight, t);
            mixingCamera.SetWeight(cameraIndex, newWeight);
            yield return null;
        }

        mixingCamera.SetWeight(cameraIndex, targetWeight);
    }

    public void StopAllTrackedCoroutines()
    {
        if (weightLerpCoroutine1 != null) 
        {
            StopCoroutine(weightLerpCoroutine1);
            weightLerpCoroutine1 = null;
        }
        if (weightLerpCoroutine2 != null) 
        {
            StopCoroutine(weightLerpCoroutine2);
            weightLerpCoroutine2 = null;
        }
        if (positionLerpCoroutine != null) 
        {
            StopCoroutine(positionLerpCoroutine);
            positionLerpCoroutine = null;
        }
        if (orthoSizeLerpCoroutine != null) 
        {
            StopCoroutine(orthoSizeLerpCoroutine);
            orthoSizeLerpCoroutine = null;
        }
        if (playerDampingCoroutine1 != null) 
        {
            StopCoroutine(playerDampingCoroutine1);
            playerDampingCoroutine1 = null;
        }
        if (playerDampingCoroutine2 != null) 
        {
            StopCoroutine(playerDampingCoroutine2);
            playerDampingCoroutine2 = null;
        }
        if (mouseDampingCoroutine1 != null) 
        {
            StopCoroutine(mouseDampingCoroutine1);
            mouseDampingCoroutine1 = null;
        }
        if (mouseDampingCoroutine2 != null) 
        {
            StopCoroutine(mouseDampingCoroutine2);
            mouseDampingCoroutine2 = null;
        }
        if (playerDzCoroutine1 != null) 
        {
            StopCoroutine(playerDzCoroutine1);
            playerDzCoroutine1 = null;
        }
        if (playerDzCoroutine2 != null) 
        {
            StopCoroutine(playerDzCoroutine2);
            playerDzCoroutine2 = null;
        }
        if (playerSzCoroutine1 != null) 
        {
            StopCoroutine(playerSzCoroutine1);
            playerSzCoroutine1 = null;
        }
        if (playerSzCoroutine2 != null) 
        {
            StopCoroutine(playerSzCoroutine2);
            playerSzCoroutine2 = null;
        }
        if (mouseDzCoroutine1 != null) 
        {
            StopCoroutine(mouseDzCoroutine1);
            mouseDzCoroutine1 = null;
        }
        if (mouseDzCoroutine2 != null) 
        {
            StopCoroutine(mouseDzCoroutine2);
            mouseDzCoroutine2 = null;
        }
        if (mouseSzCoroutine1 != null) 
        {
            StopCoroutine(mouseSzCoroutine1);
            mouseSzCoroutine1 = null;
        }
        if (mouseSzCoroutine2 != null) 
        {
            StopCoroutine(mouseSzCoroutine2);
            mouseSzCoroutine2 = null;
        }
    }

    #endregion
}