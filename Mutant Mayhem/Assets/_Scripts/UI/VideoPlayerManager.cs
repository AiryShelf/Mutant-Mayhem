using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class VideoPlayerManager : MonoBehaviour
{
    public static VideoPlayerManager Instance;

    public VideoPlayer videoPlayer;
    [SerializeField] GameObject rawImage;
    [SerializeField] InputActionAsset inputActions;

    int sceneToLoad = 0;
    InputActionMap uiActionMap = null;
    InputActionMap playerActionMap = null;
    InputAction pauseAction = null;
    InputAction buildAction = null;
    InputAction selectAction = null;
    public MainMenuController mainMenuController;

    void OnSkipPerformed(InputAction.CallbackContext ctx)
    {
        StopVideo();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        AssignSkipActions();
    }

    void Start()
    {
        videoPlayer.loopPointReached += VideoEnd;
    }

    void OnDestroy()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnSkipPerformed; 
        }
        if (buildAction != null)
        {
            buildAction.performed -= OnSkipPerformed; 
        }
        if (selectAction != null)
        {
            selectAction.performed -= OnSkipPerformed; 
        }
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= VideoEnd; 
        }
    }

    // This is essential for controller support
    void AssignSkipActions()
    {
        uiActionMap = inputActions != null ? inputActions.FindActionMap("UI") : null;
        if (uiActionMap != null)
        {
            pauseAction = uiActionMap.FindAction("Pause");
            if (pauseAction != null)
            {
                pauseAction.performed += OnSkipPerformed; 
            }
        }

        playerActionMap = inputActions != null ? inputActions.FindActionMap("Player") : null;
        if (playerActionMap != null)
        {
            buildAction = playerActionMap.FindAction("Build");
            if (buildAction != null)
            {
                buildAction.performed += OnSkipPerformed; 
            }

            selectAction = playerActionMap.FindAction("Select");
            if (selectAction != null)
            {
                selectAction.performed += OnSkipPerformed; 
            }
        }
    }

    public void PlayTutorialVideo(int sceneToLoad, List<CanvasGroup> hideGroups)
    {
        foreach (var group in hideGroups)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        this.sceneToLoad = sceneToLoad;
        StartCoroutine(PlayPreparedVideo());
        UI_MusicPlayerPanel.Instance.ShowPanel(false);
    }

    public void StopVideo()
    {
        videoPlayer.Stop();
        rawImage.SetActive(false);
        UI_MusicPlayerPanel.Instance.ShowPanel(true);
        if (mainMenuController == null)
        {
            mainMenuController = FindObjectOfType<MainMenuController>();
        }
        if (mainMenuController != null)
        {
            mainMenuController.StartCoroutine(mainMenuController.LoadSceneCoroutine(sceneToLoad));
        }
    }

    void VideoEnd(VideoPlayer vp)
    {
        StopVideo();
    }

    IEnumerator PlayPreparedVideo()
    {
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        rawImage.SetActive(true);
        videoPlayer.Play();
    }
}