using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QCubeController : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] DeathCanvas deathCanvas;
    [SerializeField] TextMeshProUGUI deathTitleText;
    [SerializeField] TextMeshProUGUI deathSubtitleText;
    [SerializeField] List<string> deathTitles;
    [SerializeField] List<string> deathSubtitles;
    
    public static bool IsDead;

    void Awake()
    {
        IsDead = false;
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
    }

    void RandomizeDeathMessages()
    {
        int randomIndex = Random.Range(0, deathTitles.Count);
        deathTitleText.text = deathTitles[randomIndex];

        randomIndex = Random.Range(0, deathSubtitles.Count);
        deathSubtitleText.text = deathSubtitles[randomIndex];
    }
}
