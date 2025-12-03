using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class QCubeController : MonoBehaviour
{
    public static QCubeController Instance;

    public QCubeHealth cubeHealth;    

    [Header("Death")]
    [SerializeField] TextMeshProUGUI deathTitleText;
    [SerializeField] TextMeshProUGUI deathSubtitleText;
    [SerializeField] List<string> cubeDeathTitles;
    [SerializeField] List<string> cubeDeathSubtitles;
    static bool _isCubeDestroyed; // Backing field
    public static event Action<bool> OnCubeDestroyed;
    public static bool IsCubeDestroyed
    {
        get { return _isCubeDestroyed; }
        set
        {
            if (_isCubeDestroyed != value)
            {
                _isCubeDestroyed = value;
                OnCubeDestroyed?.Invoke(_isCubeDestroyed);
            }
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else
        {
            Destroy(gameObject);
            return;
        }

        IsCubeDestroyed = false;
        
    }

    public void RandomizeDeathMessages()
    {
        int randomIndex = UnityEngine.Random.Range(0, cubeDeathTitles.Count);
        deathTitleText.text = cubeDeathTitles[randomIndex];

        randomIndex = UnityEngine.Random.Range(0, cubeDeathSubtitles.Count);
        deathSubtitleText.text = cubeDeathSubtitles[randomIndex];
    }
}
