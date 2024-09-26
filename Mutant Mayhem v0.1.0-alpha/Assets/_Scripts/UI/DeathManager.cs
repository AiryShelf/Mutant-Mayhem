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
    [SerializeField] UIStatsListBuilder statsListBuilder;
    [SerializeField] PauseMenuController pauseMenuController;
    [SerializeField] TextMeshProUGUI deathTitleText;
    [SerializeField] TextMeshProUGUI deathSubtitleText;
    [SerializeField] List<string> cubeDeathTitles;
    [SerializeField] List<string> cubeDeathSubtitles;
    [SerializeField] List<string> playerDeathTitles;
    [SerializeField] List<string> playerDeathSubtitles;

    [Header("Research Points")]
    [SerializeField] int basePoints = 0;
    [SerializeField] int incrementPerWave = 20;
    [SerializeField] int difficultyAdjustHard = 20;
    [SerializeField] int difficultyAdjustEasy = -10;

    int adjustedIncPerWave;

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

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void BackToShip()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
            Application.Quit();
    }

    public void TransitionToPlayerDeath(bool destroyed)
    {
        if (destroyed && !isTriggered)
        {
            TransitionToPanel();   
            RandomizeDeathMessages(playerDeathTitles, playerDeathSubtitles); 
            ApplyDeathPoints();       
        }
    }

    public void TransitionToCubeDeath(bool destroyed)
    {
        if (destroyed && !isTriggered)
        {
            player.IsDead = true;
            TransitionToPanel();
            RandomizeDeathMessages(cubeDeathTitles, cubeDeathSubtitles);
            ApplyDeathPoints();
        }
    }

    void TransitionToPanel()
    {
        myCanvasGroup.blocksRaycasts = true;
        pauseMenuController.isPaused = true;
        isTriggered = true;
        statsListBuilder.BuildListandText(waveController, this);
        fadeCanvasGroupsWave.isTriggered = true;
    }

    public void ApplyDifficultyToRPGain(DifficultyLevel difficultyLevel)
    {
        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                adjustedIncPerWave = incrementPerWave + difficultyAdjustEasy;
            break;
            case DifficultyLevel.Normal:
                adjustedIncPerWave = incrementPerWave;
            break;
            case DifficultyLevel.Hard:
                adjustedIncPerWave = incrementPerWave + difficultyAdjustHard;
            break;
        }

        Debug.Log("RP per wave set to " + adjustedIncPerWave);
    }

    void ApplyDeathPoints()
    {
        PlayerProfile currentProfile = ProfileManager.Instance.currentProfile;

        // Apply research points to profile
        int points = GetResearchPointsGain();
        currentProfile.researchPoints += points;
        Debug.Log("Added " + points + " research points to current profile");

        // Apply max wave reached to profile
        if (currentProfile.maxWaveReached < waveController.currentWaveIndex)
        {
            currentProfile.maxWaveReached = waveController.currentWaveIndex;
        }

        currentProfile.playthroughs++;

        // Save changes to profile
        ProfileManager.Instance.SaveCurrentProfile();
    }

    public int GetResearchPointsGain()
    {
        int pointsToGive = basePoints + Mathf.CeilToInt(adjustedIncPerWave * waveController.currentWaveIndex * 1.5f);
        return pointsToGive;
    }
}
