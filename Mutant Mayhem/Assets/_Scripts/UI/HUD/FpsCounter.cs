using System.Collections;
using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    public static FpsCounter Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float hudRefreshRate = 1f;

    [Header("Internal Tracking")]
    private float timer;
    private int frameCount;
    private float fpsSum;

    private float currentAvg;
    private float minFPS = float.MaxValue;
    private float maxFPS = float.MinValue;

    private int framesUnder20;
    private int framesUnder40;
    private int framesUnder60;

    private void Awake()
    {
        // Basic singleton enforcement
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(HudRefresh());
    }

    private void Update()
    {
        float fps = 1f / Time.unscaledDeltaTime;

        // Track data
        fpsSum += fps;
        frameCount++;

        if (fps < minFPS) minFPS = fps;
        if (fps > maxFPS) maxFPS = fps;

        if (fps < 20) framesUnder20++;
        if (fps < 40) framesUnder40++;
        if (fps < 60) framesUnder60++;

        // Update averages every second
        timer += Time.unscaledDeltaTime;
        if (timer >= 1f)
        {
            currentAvg = fpsSum / frameCount;
            fpsSum = 0;
            frameCount = 0;
            timer = 0;
        }
    }

    IEnumerator HudRefresh()
    {
        while (true)
        {
            int displayFps = (int)currentAvg;
            fpsText.text = "FPS: " + displayFps;
            yield return new WaitForSecondsRealtime(hudRefreshRate);
        }
    }

    // -------- ANALYTICS SNAPSHOT --------

    public FpsSnapshot GetSnapshot()
    {
        return new FpsSnapshot
        {
            avgFPS = Mathf.RoundToInt(currentAvg),
            minFPS = Mathf.RoundToInt(minFPS),
            maxFPS = Mathf.RoundToInt(maxFPS),
            framesUnder20 = framesUnder20,
            framesUnder40 = framesUnder40,
            framesUnder60 = framesUnder60,
            deviceRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value
        };
    }

    public void ResetSnapshot()
    {
        currentAvg = 0;
        minFPS = float.MaxValue;
        maxFPS = float.MinValue;

        framesUnder20 = 0;
        framesUnder40 = 0;
        framesUnder60 = 0;

        fpsSum = 0;
        frameCount = 0;
        timer = 0;
    }
}

// -------- STRUCT FOR ANALYTICS --------
public struct FpsSnapshot
{
    public int avgFPS;
    public int minFPS;
    public int maxFPS;
    public int framesUnder20;
    public int framesUnder40;
    public int framesUnder60;
    public float deviceRefreshRate;
}