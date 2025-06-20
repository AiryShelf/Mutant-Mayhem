using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class VideoPlayerManager : MonoBehaviour
{
    public static VideoPlayerManager Instance;

    public VideoPlayer videoPlayer;
    [SerializeField] GameObject rawImage;

    int sceneToLoad = 0;
    List<CanvasGroup> hideGroups;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        videoPlayer.loopPointReached += VideoEnd;
    }

    public void PlayTutorialVideo(int sceneToLoad, List<CanvasGroup> hideGroups)
    {
        this.hideGroups = hideGroups;
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
        OnVideoEnd(videoPlayer);
        UI_MusicPlayerPanel.Instance.ShowPanel(true);
    }

    void VideoEnd(VideoPlayer vp)
    {
        StopVideo();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    System.Collections.IEnumerator PlayPreparedVideo()
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