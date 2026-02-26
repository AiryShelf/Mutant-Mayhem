using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathManager : MonoBehaviour
{
    [SerializeField] QCubeController qCube;
    [SerializeField] Player player;
    [SerializeField] StatsCounterPlayer statsCounterPlayer;
    [SerializeField] CanvasGroup myCanvasGroup;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroupsWave;
    [SerializeField] UI_DeathStatsListBuilder statsListBuilder;
    [SerializeField] PauseMenuController pauseMenuController;
    [SerializeField] Button backToShipButton;
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

    bool isTriggered;
    float storedSFXVolume;

    WaveController waveController;
    PlanetSO currentPlanet;

    void OnEnable()
    {
        waveController = FindObjectOfType<WaveController>();
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
        TimeControl.Instance.ResetTimeScale();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        GameTools.StartCoroutine(LerpSFXVolume(storedSFXVolume, deathSFXFadeTime));
        TimeControl.Instance.ResetTimeScale();
        SceneManager.LoadScene(1);
    }

    public void BackToShip()
    {
        GameTools.StartCoroutine(LerpSFXVolume(storedSFXVolume, deathSFXFadeTime));
        TimeControl.Instance.ResetTimeScale();
        SceneManager.LoadScene(2);
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
            UpgradePanelManager.Instance.CloseAllPanels();
            worldCustomCursor.useUiTrans = false;
            worldCustomCursor.worldTrans = player.transform;
            TransitionToPanel();
            ApplyDeathStats();
            RandomizeDeathMessages(playerDeathTitles, playerDeathSubtitles);
        }
    }

    public void TransitionToCubeDeath(bool destroyed)
    {
        if (destroyed && !isTriggered)
        {
            UpgradePanelManager.Instance.CloseAllPanels();
            worldCustomCursor.useUiTrans = false;
            worldCustomCursor.worldTrans = qCube.transform;
            player.IsDead = true;
            TransitionToPanel();
            ApplyDeathStats();        
            RandomizeDeathMessages(cubeDeathTitles, cubeDeathSubtitles);
        }
    }

    void TransitionToPanel()
    {
        UI_MissionPanelController.Instance.EnableMissionPanel(false);
        UI_MissionPanelController.Instance.StopMissionPanel();
        
        // Hide 'Back to Ship' button if this was a tutorial mission
        if (PlanetManager.Instance.currentPlanet.mission.isTutorial)
        {
            backToShipButton.gameObject.SetActive(false);
            var canvasGroup = backToShipButton.GetComponent<CanvasGroup>();
            if (canvasGroup && fadeCanvasGroupsWave.individualElements.Contains(canvasGroup))
            {
                fadeCanvasGroupsWave.individualElements.Remove(canvasGroup);
            }
        }

        MessageManager.Instance.StopAllConversations();

        CursorManager.Instance.inMenu = true;
        TouchManager.Instance.ShowVirtualJoysticks(false);
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

    void ApplyDeathStats()
    {
        if (currentPlanet.isTutorialPlanet)
            return;

        PlayerProfile currentProfile = ProfileManager.Instance.currentProfile;

        // Ensure dictionary is built from the serialized list
        currentProfile.EnsurePlanetIndexLookup();

        string planetKey = currentPlanet.bodyName;
        int previousMax = currentProfile.GetPlanetMaxIndex(planetKey);

        // Only update if this is a new record
        if (waveController.currentWaveIndex > previousMax)
        {
            currentProfile.SetPlanetMaxIndex(planetKey, waveController.currentWaveIndex);
        }

        ProfileManager.Instance.SaveCurrentProfile();
    }
}
