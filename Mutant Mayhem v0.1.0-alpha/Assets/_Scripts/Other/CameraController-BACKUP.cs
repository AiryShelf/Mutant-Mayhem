using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Cinemachine;
using UnityEditor;
using UnityEngine;
/*
public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera playerCamera;
    [SerializeField] CinemachineVirtualCamera mouseLookerCamera;
    [SerializeField] CinemachineMixingCamera mixingCamera;
    [SerializeField] Player player;
    [SerializeField] Transform qCubeTrans;
    [SerializeField] MouseLooker mouseLooker;
    [SerializeField] float DeathZoomLerpTime = 2f;
    [SerializeField] float playerDeathZoomOut = -2f;
    [SerializeField] float cubeDeathZooomOut = 2f;
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

    bool deathSequenceStarted;
    Coroutine zoomCoroutine;

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
        // Subscribe to the OnCubeDestroyed event
        QCubeController.OnCubeDestroyed += HandleCubeDeath;
        Player.OnPlayerDestroyed += HandlePlayerDeath;
    }

    void OnDisable()
    {
        // Reset to defaults
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

        QCubeController.OnCubeDestroyed -= HandleCubeDeath;
        Player.OnPlayerDestroyed -= HandlePlayerDeath;

        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }
    }

    private void HandleCubeDeath(bool isDestroyed)
    {
        if (isDestroyed && !deathSequenceStarted)
        {
            //Debug.Log("Cube was destroyed! Adjusting camera...");
            
            // Focus mixing camera on mouseLooker
            GameTools.StartCoroutine(LerpCameraWeight(0f, 0, 2f));
            GameTools.StartCoroutine(LerpCameraWeight(1f, 1, 2f));
            LockCamerasToTargets(false, true);
            SetMixingCamera(false, true);

            // Lerp mouseLooker to QCube
            mouseLooker.deathTriggered = true;
            GameTools.StartCoroutine(GameTools.LerpPosition(mouseLooker.transform, mouseLooker.transform.position, 
                                   qCubeTrans.position, DeathZoomLerpTime));

            // Apply Cube zoom
            float orthoSize = playerCamera.m_Lens.OrthographicSize;
            GameTools.StartCoroutine(GameTools.LerpFloat(orthoSize, orthoSize + playerDeathZoomOut, 
                                                         DeathZoomLerpTime, UpdateOrthoSize));

            deathSequenceStarted = true;
            player.IsDead = true;
        }
    }

    void HandlePlayerDeath(bool isDestroyed)
    {
        if (isDestroyed && !deathSequenceStarted)
        {
            //Debug.Log("Player was destroyed! Adjusting camera...");

            // Focus mixing camera on Player
            GameTools.StartCoroutine(LerpCameraWeight(1f, 0, 2f));
            GameTools.StartCoroutine(LerpCameraWeight(0f, 1, 2f));
            LockCamerasToTargets(true, false);
            SetMixingCamera(true, false);

            // Lerp mouseLooker to Player
            mouseLooker.deathTriggered = true;
            GameTools.StartCoroutine(GameTools.LerpPosition(mouseLooker.transform, mouseLooker.transform.position, 
                                   player.transform.position, DeathZoomLerpTime));

            // Apply player zoom
            float orthoSize = playerCamera.m_Lens.OrthographicSize;
            GameTools.StartCoroutine(GameTools.LerpFloat(orthoSize, orthoSize + playerDeathZoomOut, 
                                                         DeathZoomLerpTime, UpdateOrthoSize));

            deathSequenceStarted = true;
        } 
    }

    void UpdateOrthoSize(float newSize)
    {
        playerCamera.m_Lens.OrthographicSize = newSize;
    }

    public void SetCameraDamping(float dampingValue, bool lockCameras)
    {
        if (playerFramingTransposer == null || mouseFramingTransposer == null)
        {
            Debug.Log("Could not find reference to framing transposer(s)");
            return;
        }

        if (lockCameras)
        {
            // Set damping values for the player camera
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_XDamping, 
                dampingValue, 1f, value => UpdateFloat(ref playerFramingTransposer.m_XDamping, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_YDamping, 
                dampingValue, 1f, value => UpdateFloat(ref playerFramingTransposer.m_YDamping, value)));

            // Set damping values for the mouse looker camera
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_XDamping, 
                dampingValue, 1f, value => UpdateFloat(ref mouseFramingTransposer.m_XDamping, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_YDamping, 
                dampingValue, 1f, value => UpdateFloat(ref mouseFramingTransposer.m_YDamping, value)));
        }
        else
        {
            // Reset damping values
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_XDamping, 
                playerXDamping, 0.1f, value => UpdateFloat(ref playerFramingTransposer.m_XDamping, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_YDamping, 
                playerYDamping, 0.1f, value => UpdateFloat(ref playerFramingTransposer.m_YDamping, value)));

            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_XDamping, 
                mouseXDamping, 0.1f, value => UpdateFloat(ref mouseFramingTransposer.m_XDamping, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_YDamping, 
                mouseYDamping, 0.1f, value => UpdateFloat(ref mouseFramingTransposer.m_YDamping, value)));
        }
    }

    public void LockCamerasToTargets(bool playerLock, bool mouseLookerLock)
    {
        if (playerFramingTransposer == null || mouseFramingTransposer == null) return;

        if (playerLock)
        {
            // Lerp player dead zones to 0
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneWidth, 
                0, 1f, value => UpdateFloat(ref playerFramingTransposer.m_DeadZoneWidth, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneHeight, 
                0, 1f, value => UpdateFloat(ref playerFramingTransposer.m_DeadZoneHeight, value)));

            // Same for soft zones
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneWidth, 
                0, 1f, value => UpdateFloat(ref playerFramingTransposer.m_SoftZoneWidth, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneHeight, 
                0, 1f, value => UpdateFloat(ref playerFramingTransposer.m_SoftZoneHeight, value)));
        }
        else if (mouseLookerLock)
        {
            // Lerp mouseLooker dead zones to 0
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneWidth, 
                0, 1f, value => UpdateFloat(ref mouseFramingTransposer.m_DeadZoneWidth, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneHeight, 
                0, 1f, value => UpdateFloat(ref mouseFramingTransposer.m_DeadZoneHeight, value)));
            
            // Same for soft zones
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneWidth, 
                0, 1f, value => UpdateFloat(ref mouseFramingTransposer.m_SoftZoneWidth, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneHeight, 
                0, 1f, value => UpdateFloat(ref mouseFramingTransposer.m_SoftZoneHeight, value)));
        }
        else
        {
            // Lerp all dead zones back to defaults
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneWidth, 
                playerDZWidth, 1f, value => UpdateFloat(ref playerFramingTransposer.m_DeadZoneWidth, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_DeadZoneHeight, 
                playerDZHeight, 1f, value => UpdateFloat(ref playerFramingTransposer.m_DeadZoneHeight, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneWidth, 
                mouseDZWidth, 1f, value => UpdateFloat(ref mouseFramingTransposer.m_DeadZoneWidth, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_DeadZoneHeight, 
                mouseDZHeight, 1f, value => UpdateFloat(ref mouseFramingTransposer.m_DeadZoneHeight, value)));

            // Same for soft zones
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneWidth, 
                playerSZWidth, 1f, value => UpdateFloat(ref playerFramingTransposer.m_SoftZoneWidth, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(playerFramingTransposer.m_SoftZoneHeight, 
                playerSZHeight, 1f, value => UpdateFloat(ref playerFramingTransposer.m_SoftZoneHeight, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneWidth, 
                mouseSZWidth, 1f, value => UpdateFloat(ref mouseFramingTransposer.m_SoftZoneWidth, value)));
            GameTools.StartCoroutine(GameTools.LerpFloat(mouseFramingTransposer.m_SoftZoneHeight, 
                mouseSZHeight, 1f, value => UpdateFloat(ref mouseFramingTransposer.m_SoftZoneHeight, value)));
        }
    }

    public void SetMixingCamera(bool playerLock, bool mouseLookerLock)
    {
        if (mixingCamera == null)
        {
            Debug.LogError("CinemachineMixingCamera component is not assigned.");
            return;
        }

        if (playerLock)
        {
            // Lerp weight to 0 for mouse looker
            GameTools.StartCoroutine(LerpCameraWeight(0f, 1, 1f));
        }
        else if (mouseLookerLock)
        {
            // Lerp weight to 0 for player and 1 for mouse looker
            GameTools.StartCoroutine(LerpCameraWeight(0f, 0, 1f));
            GameTools.StartCoroutine(LerpCameraWeight(1f, 1, 1f));
        }
        else
        {
            // Lerp weights back to saved values
            GameTools.StartCoroutine(LerpCameraWeight(playerMixWeight, 0, 1f));
            GameTools.StartCoroutine(LerpCameraWeight(mouseMixWeight, 1, 1f));
        }
    }

    IEnumerator LerpCameraWeight(float targetWeight, int cameraIndex, float duration)
    {
        if (mixingCamera == null || cameraIndex < 0 || cameraIndex >= mixingCamera.ChildCameras.Length)
        {
            Debug.LogWarning("Invalid camera index or Mixing Camera not set.");
            yield break;
        }

        // Get the initial weight of the camera
        float initialWeight = mixingCamera.GetWeight(cameraIndex);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float newWeight = Mathf.Lerp(initialWeight, targetWeight, t);

            // Set the new weight for the camera
            mixingCamera.SetWeight(cameraIndex, newWeight);

            yield return null;
        }

        // Ensure the final weight is set accurately
        mixingCamera.SetWeight(cameraIndex, targetWeight);
    }

    void UpdateFloat(ref float property, float newValue)
    {
        property = newValue;
    }
}
*/