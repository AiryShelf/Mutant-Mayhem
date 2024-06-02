using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class QCubeController : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] DeathCanvas deathCanvas;
    [SerializeField] TextMeshProUGUI deathTitleText;
    [SerializeField] TextMeshProUGUI deathSubtitleText;
    [SerializeField] List<string> deathTitles;
    [SerializeField] List<string> deathSubtitles;
    [SerializeField] float interactRadius = 1.5f;
    
    public static bool IsDead;
    public bool isOpen;

    InputActionMap playerActionMap;
    InputAction qCubeAction;

    void Awake()
    {
        IsDead = false;
        playerActionMap = player.inputAsset.FindActionMap("Player");
        qCubeAction = playerActionMap.FindAction("QCube");
    }

    void OnEnable()
    {  
        qCubeAction.performed += OnQCubeInteract;
    }

    void OnDisable()
    {
        qCubeAction.performed -= OnQCubeInteract;
    }

    void FixedUpdate()
    {
        if (IsDead && !player.isDead)
        {
            player.isDead = true;
            // Implement different ending, animation of cube exploding or the like
            RandomizeDeathMessages();
            deathCanvas.TransitionToDeathPanel();
        }

        if (isOpen)
        {
            Collider2D col = Physics2D.OverlapCircle(
                transform.position, interactRadius, LayerMask.NameToLayer("Player"));
            if (!col)
            {

            }
        }
    }

    void OnQCubeInteract(InputAction.CallbackContext context)
    {
        Collider2D col = Physics2D.OverlapCircle(
            transform.position, interactRadius, LayerMask.NameToLayer("Player"));
        if (col)
        {

        }
    }

    void OpenQCube()
    {

    }

    void RandomizeDeathMessages()
    {
        int randomIndex = Random.Range(0, deathTitles.Count);
        deathTitleText.text = deathTitles[randomIndex];

        randomIndex = Random.Range(0, deathSubtitles.Count);
        deathSubtitleText.text = deathSubtitles[randomIndex];
    }   
}
