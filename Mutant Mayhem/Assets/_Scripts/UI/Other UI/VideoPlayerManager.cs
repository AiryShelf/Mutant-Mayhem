using System.Collections;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class VideoPlayerManager : MonoBehaviour
{
    public static VideoPlayerManager Instance;

    public VideoPlayer videoPlayer;

    [Header("Video Source")]
    [Tooltip("Optional. If set, will be used on non-macOS platforms by default.")]
    [SerializeField] private VideoClip tutorialVideoClip;

    [Tooltip("Relative path inside StreamingAssets (case-sensitive on some systems). Example: Videos/In-Game Video.mp4")]
    [SerializeField] private string streamingAssetsRelativePath = "Videos/In-Game Video.mp4";

    [SerializeField] GameObject rawImage;
    [SerializeField] InputActionAsset inputActions;

    private bool _eventsHooked;
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
        HookVideoEvents();
    }

    void HookVideoEvents()
    {
        if (_eventsHooked || videoPlayer == null)
            return;

        // Avoid hangs on macOS if decode fails
        videoPlayer.waitForFirstFrame = false;

        videoPlayer.loopPointReached += VideoEnd;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.prepareCompleted += OnVideoPrepared;

        _eventsHooked = true;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        // Only show the RawImage once we have something prepared
        if (rawImage != null)
            rawImage.SetActive(true);
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"VideoPlayer error: {message} | source={vp.source} url={vp.url}");

        // Fail safe: don't leave player stuck on a black screen
        StopVideo();
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
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
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
        if (videoPlayer == null)
            return;

        videoPlayer.Stop();
        videoPlayer.clip = null;
        videoPlayer.url = null;

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
        HookVideoEvents();

        if (videoPlayer == null)
            yield break;

        // Always start clean
        videoPlayer.Stop();
        videoPlayer.clip = null;
        videoPlayer.url = null;

        // Prefer StreamingAssets URL on macOS (most reliable), otherwise prefer VideoClip if provided.
        bool useUrl = Application.platform == RuntimePlatform.OSXPlayer;

        if (!useUrl && tutorialVideoClip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = tutorialVideoClip;
        }
        else
        {
            videoPlayer.source = VideoSource.Url;

            string fullPath = Path.Combine(Application.streamingAssetsPath, streamingAssetsRelativePath);

            // Some platforms prefer a file:// URI
            string url = fullPath;
            try
            {
                url = new Uri(fullPath).AbsoluteUri;
            }
            catch { }

            // Helpful diagnostics
            bool exists = false;
            try { exists = File.Exists(fullPath); } catch { }
            Debug.Log($"Tutorial video URL mode. fullPath={fullPath} exists={exists} uri={url}");

            videoPlayer.url = url;
        }

        // Prepare + timeout so we never hang forever on black
        rawImage.SetActive(false);
        videoPlayer.Prepare();

        float timeout = 8f;
        float start = Time.realtimeSinceStartup;

        while (!videoPlayer.isPrepared)
        {
            if (Time.realtimeSinceStartup - start > timeout)
            {
                Debug.LogError("VideoPlayer Prepare() timed out. Skipping video.");
                StopVideo();
                yield break;
            }
            yield return null;
        }

        // RawImage is enabled in OnVideoPrepared; ensure it's on regardless
        rawImage.SetActive(true);
        videoPlayer.Play();
    }
}