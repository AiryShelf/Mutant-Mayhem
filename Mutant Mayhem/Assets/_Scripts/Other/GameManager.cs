using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// I'm just testing this out, not sure if i need it yet.  
// could be useful for options selcted in the main menu carrying into next scene
// load game functionality or as a "service locator" connecting other manager scripts

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set;}
    
    void Start()
    {
        
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }
}
