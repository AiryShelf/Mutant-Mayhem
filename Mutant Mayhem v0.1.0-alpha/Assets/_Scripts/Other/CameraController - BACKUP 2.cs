using System.Collections;
using UnityEngine;
using Cinemachine;

/*
public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] CinemachineVirtualCamera mouseLookerCamera;
    [SerializeField] CinemachineMixingCamera mixingCamera;
    [SerializeField] MouseLooker mouseLooker;
    [SerializeField] Player player;
    [SerializeField] Transform qCubeTrans;
    [SerializeField] float deathLerpTime = 2f;

    CinemachineFramingTransposer playerFramingTransposer;
    CinemachineFramingTransposer mouseFramingTransposer;

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
    float mouseMixWeight;

    Coroutine zoomCoroutine;
    bool deathZoomStarted;

    void Awake()
    {
        playerFramingTransposer = playerCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        mouseFramingTransposer = mouseLookerCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        // Store defaults
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
    }

    void OnEnable()
    {
        Player.OnPlayerDestroyed += HandlePlayerDeath;
        QCubeController.OnCubeDestroyed += HandleCubeDeath;
    }

    void OnDisable()
    {
        ResetCameraSettings();
        Player.OnPlayerDestroyed -= HandlePlayerDeath;
        QCubeController.OnCubeDestroyed -= HandleCubeDeath;
    }

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

        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }
    }

    void HandlePlayerDeath(bool isDestroyed)
    {
        if (isDestroyed && !deathZoomStarted)
        {
            ZoomToTarget(player.transform, -2f, 2f, deathLerpTime, true, false);
            deathZoomStarted = true;
        }
    }

    void HandleCubeDeath(bool isDestroyed)
    {
        if (isDestroyed && !deathZoomStarted)
        {
            ZoomToTarget(qCubeTrans, 2f, 2f, deathLerpTime, false, true);
            deathZoomStarted = true;
        }
    }

    public void ZoomToTarget(Transform targetTrans, float orthoZoomAmount, float dampingAmount, 
                             float duration, bool focusPlayer, bool focusMouseLooker)
    {
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }

        zoomCoroutine = StartCoroutine(ZoomAndFocusCoroutine(targetTrans, orthoZoomAmount, 
            dampingAmount, duration, focusPlayer, focusMouseLooker));
    }

    IEnumerator ZoomAndFocusCoroutine(Transform targetTrans, float orthoZoomAmount, float dampingAmount, 
                                              float duration, bool focusPlayer, bool focusMouseLooker)
    {
        // Adjust weights based on focus parameters
        if (focusPlayer)
        {
            GameTools.StartCoroutine(LerpCameraWeight(1f, 0, duration)); // Focus on player camera
            GameTools.StartCoroutine(LerpCameraWeight(0f, 1, duration)); // Unfocus mouseLooker camera
        }
        else if (focusMouseLooker)
        {
            GameTools.StartCoroutine(LerpCameraWeight(0f, 0, duration)); // Unfocus player camera
            GameTools.StartCoroutine(LerpCameraWeight(1f, 1, duration)); // Focus on mouseLooker camera
        }
        else
        {
            // If both are false, restore default weights
            GameTools.StartCoroutine(LerpCameraWeight(playerMixWeight, 0, duration));
            GameTools.StartCoroutine(LerpCameraWeight(mouseMixWeight, 1, duration));
        }

        // Lock camera to target and set damping
        LockCamerasToTarget(focusPlayer, focusMouseLooker);
        bool lockCameras = focusPlayer || focusMouseLooker;
        SetCameraDamping(dampingAmount, lockCameras);

        // Lerp mouseLooker to target
        mouseLooker.deathTriggered = true;
        GameTools.StartCoroutine(GameTools.LerpPosition(mouseLooker.transform, 
            mouseLooker.transform.position, targetTrans.position, duration));

        // Lerp ortho size
        float initialOrthoSize = playerCamera.m_Lens.OrthographicSize;
        yield return GameTools.StartCoroutine(GameTools.LerpFloat(initialOrthoSize, 
            initialOrthoSize + orthoZoomAmount, duration, UpdateOrthoSize));
    }

    void UpdateOrthoSize(float newSize)
    {
        playerCamera.m_Lens.OrthographicSize = newSize;
    }

    void SetCameraDamping(float dampingValue, bool lockCameras)
    {
        if (playerFramingTransposer == null || mouseFramingTransposer == null) return;

        if (lockCameras)
        {
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_XDamping, 
                dampingValue, 1f, value => playerFramingTransposer.m_XDamping = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_YDamping, 
                dampingValue, 1f, value => playerFramingTransposer.m_YDamping = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_XDamping, 
                dampingValue, 1f, value => mouseFramingTransposer.m_XDamping = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_YDamping, 
                dampingValue, 1f, value => mouseFramingTransposer.m_YDamping = value));
        }
        else
        {
            // Return to default
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_XDamping, 
                playerXDamping, 1f, value => playerFramingTransposer.m_XDamping = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_YDamping, 
                playerYDamping, 1f, value => playerFramingTransposer.m_YDamping = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_XDamping, 
                mouseXDamping, 1f, value => mouseFramingTransposer.m_XDamping = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_YDamping, 
                mouseYDamping, 1f, value => mouseFramingTransposer.m_YDamping = value));
        }
    }

    void LockCamerasToTarget(bool playerLock, bool mouseLookerLock)
    {
        if (playerFramingTransposer == null || mouseFramingTransposer == null) return;

        // Apply Dead Zone and Soft Zone changes
        if (playerLock)
        {
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneWidth, 
                0, 1f, value => playerFramingTransposer.m_DeadZoneWidth = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneHeight, 
                0, 1f, value => playerFramingTransposer.m_DeadZoneHeight = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneWidth, 
                0, 1f, value => playerFramingTransposer.m_SoftZoneWidth = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneHeight, 
                0, 1f, value => playerFramingTransposer.m_SoftZoneHeight = value));
        }
        else if (mouseLookerLock)
        {
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneWidth, 
                0, 1f, value => mouseFramingTransposer.m_DeadZoneWidth = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneHeight, 
                0, 1f, value => mouseFramingTransposer.m_DeadZoneHeight = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneWidth, 
                0, 1f, value => mouseFramingTransposer.m_SoftZoneWidth = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneHeight, 
                0, 1f, value => mouseFramingTransposer.m_SoftZoneHeight = value));
        }
        else
        {
            // Return to default
            // Player zones
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneWidth, 
                playerDZWidth, 1f, value => playerFramingTransposer.m_DeadZoneWidth = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneHeight, 
                playerDZHeight, 1f, value => playerFramingTransposer.m_DeadZoneHeight = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneWidth, 
                playerSZWidth, 1f, value => playerFramingTransposer.m_SoftZoneWidth = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneHeight, 
                playerSZHeight, 1f, value => playerFramingTransposer.m_SoftZoneHeight = value));

            // MouseLooker zones
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneWidth, 
                0, 1f, value => mouseFramingTransposer.m_DeadZoneWidth = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneHeight, 
                0, 1f, value => mouseFramingTransposer.m_DeadZoneHeight = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneWidth, 
                0, 1f, value => mouseFramingTransposer.m_SoftZoneWidth = value));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneHeight, 
                0, 1f, value => mouseFramingTransposer.m_SoftZoneHeight = value));
        }
    }

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
}
*/