using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class QCubeController : MonoBehaviour
{
    [SerializeField] Player player;
    [Header("Death")]
    [SerializeField] DeathCanvas deathCanvas;
    [SerializeField] TextMeshProUGUI deathTitleText;
    [SerializeField] TextMeshProUGUI deathSubtitleText;
    [SerializeField] List<string> deathTitles;
    [SerializeField] List<string> deathSubtitles;
    public static bool IsDestroyed;

    [Header("Upgrades")]
    [SerializeField] float interactRadius = 1.5f;
    [SerializeField] float leaveRadius = 2f;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroupsMainPanel;
    public bool isUpgradesOpen;
    [SerializeField] PauseMenuController pauseMenuController;

    InputActionMap playerActionMap;
    InputAction qCubeAction;
    InputAction fireAction;
    InputActionMap uIActionMap;
    InputAction escapeAction;


    void Awake()
    {
        IsDestroyed = false;
        playerActionMap = player.inputAsset.FindActionMap("Player");
        qCubeAction = playerActionMap.FindAction("QCube");
        fireAction = playerActionMap.FindAction("Fire");

        uIActionMap = player.inputAsset.FindActionMap("UI");
        escapeAction = uIActionMap.FindAction("Escape");

    }

    void OnEnable()
    {  
        qCubeAction.performed += OnQCubeInteract;
        escapeAction.started += OnEscapePressed;
    }

    void OnDisable()
    {
        qCubeAction.performed -= OnQCubeInteract;
        escapeAction.started -= OnEscapePressed;
    }

    void FixedUpdate()
    {
        if (IsDestroyed && !player.isDead)
        {
            player.isDead = true;
            // Implement different ending, animation of cube exploding or the like
            RandomizeDeathMessages();
            deathCanvas.TransitionToDeathPanel();
        }

        if (isUpgradesOpen)
        {
            Collider2D col = Physics2D.OverlapCircle(
                transform.position, leaveRadius, LayerMask.GetMask("Player"));
            if (!col)
            {
                CloseUpgradeWindow();
            }
        }
    }

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (isUpgradesOpen)
        {
            StartCoroutine(WaitToCheckForPause());
        }
    }

    IEnumerator WaitToCheckForPause()
    {
        yield return new WaitForSecondsRealtime(0.05f);

        CloseUpgradeWindow();
    }

    void OnQCubeInteract(InputAction.CallbackContext context)
    {
        Collider2D col = Physics2D.OverlapCircle(
            transform.position, interactRadius, LayerMask.GetMask("Player"));
        if (col != null)
        {
            //Debug.Log("Player in QCube Range");

            // Open or close menu
            if (!isUpgradesOpen)
            {
                OpenUpgradeWindow();
                //Pause(true);
            }
            else
            {
                CloseUpgradeWindow();
                //Pause(false);
            }
        }
        else
        {
            // Player is not in range, show UI message to the player
            //Debug.Log("Player NOT in QCube Range!");
        }
    }

    void OpenUpgradeWindow()
    {
        fireAction.Disable();
        fadeCanvasGroupsMainPanel.isTriggered = true;
        isUpgradesOpen = true;
    }

    void CloseUpgradeWindow()
    {
        fireAction.Enable();
        fadeCanvasGroupsMainPanel.isTriggered = false;
        isUpgradesOpen = false;
    }

    public void Pause(bool pause)
    {
        if (pause)
        {
            playerActionMap.Disable();
            Time.timeScale = 0;
            pauseMenuController.isPaused = true;
        }
        else
        {
            playerActionMap.Enable();
            Time.timeScale = 1;
            pauseMenuController.isPaused = false;
        }
    }

    void RandomizeDeathMessages()
    {
        int randomIndex = Random.Range(0, deathTitles.Count);
        deathTitleText.text = deathTitles[randomIndex];

        randomIndex = Random.Range(0, deathSubtitles.Count);
        deathSubtitleText.text = deathSubtitles[randomIndex];
    }   
}
