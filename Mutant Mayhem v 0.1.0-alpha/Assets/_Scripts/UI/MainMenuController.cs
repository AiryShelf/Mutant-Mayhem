using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] FadeCanvasGroupsWave optionsFadeGroup;

    bool isOptionsOpen;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Esc pressed");
            if (isOptionsOpen)
            {
                ToggleOptions();
            }
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ToggleOptions()
    {
        if (!isOptionsOpen)
        {
            optionsFadeGroup.isTriggered = true;
            isOptionsOpen = true;
        }
        else
        {
            optionsFadeGroup.isTriggered = false;
            isOptionsOpen = false;
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


}
