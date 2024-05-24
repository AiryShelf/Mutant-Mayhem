using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] UIBuildMenuController uIBuildMenuController;
    [SerializeField] FadeCanvasGroups fadeCanvasGroups;

    Player player;
    InputActionMap playerActionMap;
    public bool isPaused;
    bool wasBuildPanelOpen;

    void Start()
    {
        player = FindObjectOfType<Player>();
        playerActionMap = player.inputAsset.FindActionMap("Player");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                OpenPanel(true);
            else
                OpenPanel(false);
        }
    }

    public void OpenPanel(bool active)
    {
        if (active)
        {
            if (uIBuildMenuController.fadeCanvasGroups.triggered == true)
            {
                wasBuildPanelOpen = true;
                uIBuildMenuController.fadeCanvasGroups.triggered = false;
            }

            fadeCanvasGroups.triggered = true;
            Pause(true);
        }
        else
        {
            if (wasBuildPanelOpen)
            {
                wasBuildPanelOpen = false;
                uIBuildMenuController.fadeCanvasGroups.triggered = true;
            }

            fadeCanvasGroups.triggered = false;
            Pause(false);
        }
    }



    public void Continue()
    {
        OpenPanel(false);
    }

    public void Restart()
    {
        Pause(false);
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        //  If the editor is running, else if compiled quit.
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
            Application.Quit();
    }

    public void Pause(bool pause)
    {
        if (pause)
        {
            playerActionMap.Disable();
            Time.timeScale = 0;
            isPaused = true;
        }
        else
        {
            playerActionMap.Enable();
            Time.timeScale = 1;
            isPaused = false;
        }
    }
}
