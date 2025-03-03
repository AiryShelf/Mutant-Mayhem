using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    [SerializeField] QCubeController qCube;
    [SerializeField] Player player;
    [SerializeField] StatsCounterPlayer statsCounterPlayer;
    [SerializeField] CanvasGroup myCanvasGroup;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroupsWave;
    [SerializeField] UI_DeathStatsListBuilder statsListBuilder;
    [SerializeField] PauseMenuController pauseMenuController;
    [SerializeField] TextMeshProUGUI deathTitleText;
    [SerializeField] TextMeshProUGUI deathSubtitleText;
    [SerializeField] List<string> cubeDeathTitles;
    [SerializeField] List<string> cubeDeathSubtitles;
    [SerializeField] List<string> playerDeathTitles;
    [SerializeField] List<string> playerDeathSubtitles;
    [SerializeField] AudioMixer sfxMixer;
    [SerializeField] float deathSFXFadeTime = 3f;
    [SerializeField] float deathSFXFadeAmount = -4f;
    [SerializeField] FollowScreenToWorld worldCustomCursor;

    int adjustedPointsPerWave;
    bool isTriggered;
    float storedSFXVolume;

    WaveControllerRandom waveController;
    PlanetSO currentPlanet;

    void OnEnable()
    {
        waveController = FindObjectOfType<WaveControllerRandom>();
        currentPlanet = PlanetManager.Instance.currentPlanet;

        Player.OnPlayerDestroyed += TransitionToPlayerDeath;
        QCubeController.OnCubeDestroyed += TransitionToCubeDeath;
    }

    void OnDisable()
    {
        Player.OnPlayerDestroyed -= TransitionToPlayerDeath;
        QCubeController.OnCubeDestroyed -= TransitionToCubeDeath;

        worldCustomCursor.ResetUiTransToStart();
    }

    void Start()
    {
        worldCustomCursor = SettingsManager.Instance.GetComponentInChildren<FollowScreenToWorld>();
        myCanvasGroup.blocksRaycasts = false;
    }

    void RandomizeDeathMessages(List<string> titlesList, List<string> subtitlesList)
    {
        int randomIndex = Random.Range(0, titlesList.Count);
        deathTitleText.text = titlesList[randomIndex];

        randomIndex = Random.Range(0, subtitlesList.Count);
        deathSubtitleText.text = subtitlesList[randomIndex];
    }

    public void ReloadScene()
    {
        GameTools.StartCoroutine(LerpSFXVolume(storedSFXVolume, deathSFXFadeTime));
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        GameTools.StartCoroutine(LerpSFXVolume(storedSFXVolume, deathSFXFadeTime));
        SceneManager.LoadScene(0);
    }

    public void BackToShip()
    {
        GameTools.StartCoroutine(LerpSFXVolume(storedSFXVolume, deathSFXFadeTime));
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        MusicManager.Instance.mainMixer.SetFloat("sfxVolume", storedSFXVolume);
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
            Application.Quit();
    }

    public void TransitionToPlayerDeath(bool destroyed)
    {
        if (destroyed && !isTriggered)
        {
            worldCustomCursor.useUiTrans = false;
            worldCustomCursor.worldTrans = player.transform;
            ApplyDeathPoints();
            TransitionToPanel();
            RandomizeDeathMessages(playerDeathTitles, playerDeathSubtitles);
        }
    }

    public void TransitionToCubeDeath(bool destroyed)
    {
        if (destroyed && !isTriggered)
        {
            worldCustomCursor.useUiTrans = false;
            worldCustomCursor.worldTrans = qCube.transform;
            player.IsDead = true;
            ApplyDeathPoints();
            TransitionToPanel();
            RandomizeDeathMessages(cubeDeathTitles, cubeDeathSubtitles);
        }
    }

    void TransitionToPanel()
    {
        CursorManager.Instance.inMenu = true;
        TouchManager.Instance.SetVirtualJoysticksActive(false);
        InputManager.SetJoystickMouseControl(true);
        //SFXManager.Instance.FadeToDeathSnapshot();
        MusicManager.Instance.mainMixer.GetFloat("sfxVolume", out storedSFXVolume);
        StartCoroutine(LerpSFXVolume(Mathf.Clamp(storedSFXVolume + deathSFXFadeAmount, -80, float.MaxValue), 2f));
        
        myCanvasGroup.blocksRaycasts = true;
        pauseMenuController.isPauseMenuOpen = true;
        isTriggered = true;
        
        statsListBuilder.BuildListAndText();
        fadeCanvasGroupsWave.isTriggered = true;
    }

    IEnumerator LerpSFXVolume(float targetDb, float fadeTime)
    {
        float timeElapsed = 0;
        float startVolume;
        MusicManager.Instance.mainMixer.GetFloat("sfxVolume", out startVolume);

        while (timeElapsed <= fadeTime)
        {
            float t = timeElapsed / fadeTime;
            float volume = Mathf.Lerp(startVolume, targetDb, t);
            MusicManager.Instance.mainMixer.SetFloat("sfxVolume", volume);

            yield return null;
            timeElapsed += Time.deltaTime;
        }
    }

    public void ApplyDifficultyToRPGain(DifficultyLevel difficultyLevel)
    {
        /*
        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                adjustedPointsPerWave = currentPlanet.pointsPerWave + currentPlanet.difficultyAdjustEasy;
            break;
            case DifficultyLevel.Normal:
                adjustedPointsPerWave = currentPlanet.pointsPerWave;
            break;
            case DifficultyLevel.Hard:
                adjustedPointsPerWave = currentPlanet.pointsPerWave + currentPlanet.difficultyAdjustHard;
            break;
        }
        */

        adjustedPointsPerWave = currentPlanet.pointsPerWave;
        Debug.Log("RP per wave set to " + adjustedPointsPerWave + " for planet " + currentPlanet);
    }

    void ApplyDeathPoints()
    {
        PlayerProfile currentProfile = ProfileManager.Instance.currentProfile;
        currentProfile.playthroughs++;

        // Apply research points to profile
        int points = GetResearchPointsGain();
        currentProfile.researchPoints += points;
        Debug.Log("DeathManager: Added " + points + " research points to current profile");

        // Apply max wave survived to profile
        if (currentProfile.maxWaveSurvived < waveController.currentWaveIndex)
        {
            currentProfile.maxWaveSurvived = waveController.currentWaveIndex;
        }

        // Save changes to profile
        ProfileManager.Instance.SaveCurrentProfile();
    }

    public int GetResearchPointsGain()
    {
        if (waveController.currentWaveIndex < 1)
        {
            Debug.Log("Player died without passing Night 1");
            return 0;
        }

        int pointsToGive = currentPlanet.basePoints + Mathf.CeilToInt(adjustedPointsPerWave * waveController.currentWaveIndex * 
                                                        (1 + waveController.currentWaveIndex * currentPlanet.growthControlFactor));
        return pointsToGive;
    }
}
