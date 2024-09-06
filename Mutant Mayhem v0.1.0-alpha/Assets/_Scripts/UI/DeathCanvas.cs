using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathCanvas : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] StatsCounterPlayer statsCounterPlayer;
    [SerializeField] CanvasGroup myCanvasGroup;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroupsWave;
    [SerializeField] StatsListBuilder statsListBuilder;
    [SerializeField] PauseMenuController pauseMenuController;
    [SerializeField] TextMeshProUGUI deathTitleText;
    [SerializeField] TextMeshProUGUI deathSubtitleText;
    [SerializeField] List<string> deathTitles;
    [SerializeField] List<string> deathSubtitles;

    bool isTriggered;

    void OnEnable()
    {
        Player.OnPlayerDestroyed += TransitionToDeathPanel;
    }

    void OnDisable()
    {
        Player.OnPlayerDestroyed -= TransitionToDeathPanel;
    }

    void Start()
    {
        myCanvasGroup.blocksRaycasts = false;
        RandomizeDeathMessages();
    }

    void RandomizeDeathMessages()
    {
        int randomIndex = Random.Range(0, deathTitles.Count);
        deathTitleText.text = deathTitles[randomIndex];

        randomIndex = Random.Range(0, deathSubtitles.Count);
        deathSubtitleText.text = deathSubtitles[randomIndex];
    }

    public void QuitGame()
    {
        //  If the editor is running, else if compiled quit.
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
            Application.Quit();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TransitionToDeathPanel(bool destroyed)
    {
        if (destroyed && !isTriggered)
        {
            myCanvasGroup.blocksRaycasts = true;
            pauseMenuController.isPaused = true;
            isTriggered = true;
            statsListBuilder.RebuildList();
            fadeCanvasGroupsWave.isTriggered = true;           
        }
    }
}
