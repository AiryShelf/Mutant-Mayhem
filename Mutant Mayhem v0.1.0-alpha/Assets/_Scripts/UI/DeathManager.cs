using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] StatsCounterPlayer statsCounterPlayer;
    [SerializeField] CanvasGroup myCanvasGroup;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroupsWave;
    [SerializeField] StatsListBuilder statsListBuilder;
    [SerializeField] PauseMenuController pauseMenuController;
    [SerializeField] TextMeshProUGUI deathTitleText;
    [SerializeField] TextMeshProUGUI deathSubtitleText;
    [SerializeField] List<string> cubeDeathTitles;
    [SerializeField] List<string> cubeDeathSubtitles;
    [SerializeField] List<string> playerDeathTitles;
    [SerializeField] List<string> playerDeathSubtitles;
    [Header("Research Points")]
    [SerializeField] int basePoints = 10;
    [SerializeField] int incrementPerWave = 10;


    WaveControllerRandom waveController;

    bool isTriggered;

    void OnEnable()
    {
        Player.OnPlayerDestroyed += TransitionToPlayerDeath;
        QCubeController.OnCubeDestroyed += TransitionToCubeDeath;
        waveController = FindObjectOfType<WaveControllerRandom>();
    }

    void OnDisable()
    {
        Player.OnPlayerDestroyed -= TransitionToPlayerDeath;
        QCubeController.OnCubeDestroyed-= TransitionToCubeDeath;
    }

    void Start()
    {
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToShip()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        SceneManager.LoadScene(0);
    }

    public void TransitionToPlayerDeath(bool destroyed)
    {
        if (destroyed && !isTriggered)
        {
            ApplyDeathPoints();
            myCanvasGroup.blocksRaycasts = true;
            pauseMenuController.isPaused = true;
            isTriggered = true;
            statsListBuilder.RebuildList();
            fadeCanvasGroupsWave.isTriggered = true;   
            RandomizeDeathMessages(playerDeathTitles, playerDeathSubtitles);        
        }
    }

    public void TransitionToCubeDeath(bool destroyed)
    {
        if (destroyed && !isTriggered)
        {
            ApplyDeathPoints();
            player.IsDead = true;
            myCanvasGroup.blocksRaycasts = true;
            pauseMenuController.isPaused = true;
            isTriggered = true;
            statsListBuilder.RebuildList();
            fadeCanvasGroupsWave.isTriggered = true;
            RandomizeDeathMessages(cubeDeathTitles, cubeDeathSubtitles);
        }
    }

    void ApplyDeathPoints()
    {
        PlayerProfile currentProfile = ProfileManager.Instance.currentProfile;

        // Apply research points to profile
        int pointsToGive = basePoints + (incrementPerWave * waveController.currentWaveCount) + 
                           (waveController.currentWaveCount * 2);
        currentProfile.researchPoints += pointsToGive;

        // Apply max wave reached to profile
        if (currentProfile.maxWaveReached < waveController.currentWaveCount)
        {
            currentProfile.maxWaveReached = waveController.currentWaveCount;
        }

        currentProfile.playthroughs++;

        // Save changes to profile
        ProfileManager.Instance.SaveCurrentProfile();
    }
}
