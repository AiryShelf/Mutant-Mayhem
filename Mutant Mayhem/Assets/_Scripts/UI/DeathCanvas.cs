using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathCanvas : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] StatsCounterPlayer statsCounterPlayer;
    [SerializeField] FadeCanvasGroups fadeCanvasGroups;
    [SerializeField] TextMeshProUGUI statsText;


    void FixedUpdate()
    {
        if (player.isDead)
        {
            fadeCanvasGroups.triggered = true;
            statsText.text = statsCounterPlayer.PrintStats();
        }
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
}
