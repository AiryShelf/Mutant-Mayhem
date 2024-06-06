using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaveController : MonoBehaviour
{
    [SerializeField] List<WaveSOBase> allWaveBases;
    [SerializeField] WaveSpawner waveSpawner;

    [Header("UI Wave Info")]
    [SerializeField] FadeCanvasGroupsWave nextWaveFadeGroup;
    [SerializeField] FadeCanvasGroupsWave waveInfoFadeGroup;
    [SerializeField] TextMeshProUGUI enemyCountText;
    [SerializeField] TextMeshProUGUI currentNightText;
    [SerializeField] TextMeshProUGUI nextWaveText;

    [Header("Wave Properties")]
    public int currentWave = 0;
    public float timeBetweenWaves = 60f;
    public float spawnRadius = 60;
    public int wavesPerBase = 3;
    public int wavesTillExtraWaveAdd = 2;

    [Header("Enemy Multipliers")]
    public int MultiplierStart = 1;
    public int batchMultiplier = 1;
    public float damageMultiplier = 1;
    public float healthMultiplier = 1;
    public float speedMultiplier = 1;
    public float sizeMultiplier = 1;
    public float spawnSpeedMultiplier = 1;

    InputActionMap playerActionMap;
    InputAction nextWaveAction;
    Player player;

    Coroutine nextWaveTimer;
    Coroutine endWave;
    public bool isNight;


    void Awake()
    {
        player = FindObjectOfType<Player>();
        playerActionMap = player.inputAsset.FindActionMap("Player");
        nextWaveAction = playerActionMap.FindAction("NextWave");
    }

    void OnEnable()
    {  
        nextWaveAction.performed += OnNextWaveInput;
    }

    void OnDisable()
    {
        nextWaveAction.performed -= OnNextWaveInput;
    }

    void Start()
    {
        if (nextWaveTimer != null)
            StopCoroutine(nextWaveTimer);
        nextWaveTimer = StartCoroutine(NextWaveTimer());

        waveSpawner.currentWave = allWaveBases[0];
    }

    void OnNextWaveInput(InputAction.CallbackContext context)
    {
        if (nextWaveText.enabled)
        {
            StopAllCoroutines();
            TriggerWaves();
        }
    }

    void TriggerWaves()
    {
        int wavesToTrigger = 1 + (int)Mathf.Floor(currentWave / wavesTillExtraWaveAdd);

        // ** NEED TO FINISHT THIS FOR MULTI-WAVE SPAWNING ** //
        for (int w = 0; w < wavesToTrigger; w++)
        {
            StartCoroutine(StartWave());
        }
    }

    IEnumerator NextWaveTimer()
    {
        waveInfoFadeGroup.isTriggered = false;
        nextWaveText.enabled = true;
        nextWaveFadeGroup.isTriggered = true;
        float countdown = timeBetweenWaves;

        while (countdown >= 0)
        {
            nextWaveText.text = "Time until night " 
                + currentWave + ":  " + countdown.ToString("#") + "s.  Press 'Enter' to skip";
            
            yield return new WaitForEndOfFrame();
            countdown -= Time.deltaTime;        
        }

        TriggerWaves();
    }

    IEnumerator StartWave()
    {
        isNight = true;
        UpdateWaveMultipliers();

        // Switch Spawner to new WaveBase
        int index = (int)Mathf.Floor(currentWave / wavesPerBase);
        index = Mathf.Clamp(index, 0, allWaveBases.Count - 1);
        waveSpawner.currentWave = allWaveBases[index];
        // Need to add logic to handle the end of the list.
        // Maybe restart with larger multipliers?  Or a random new list?

        if (nextWaveTimer != null)
            StopCoroutine(nextWaveTimer);
        
        waveSpawner.StartWave();

        // Set wave UI text
        waveInfoFadeGroup.isTriggered = true;
        nextWaveFadeGroup.isTriggered = false;
        nextWaveText.enabled = false;
        currentNightText.text = "Night " + currentWave;

        // Wait 5 seconds before checking wave complete
        float timeElapsed = 0;
        while (timeElapsed < 5)
        {
            enemyCountText.text = "Enemies Detected: " + WaveSpawner.EnemyCount;
            

            yield return new WaitForEndOfFrame();
            timeElapsed += Time.deltaTime; 
        }

        // Check for end of wave
        while (!waveSpawner.waveComplete)
        {      
            enemyCountText.text = "Enemies Detected: " + WaveSpawner.EnemyCount;

            yield return new WaitForEndOfFrame(); 
        }
        
        // Let one wave initiate ending
        if (endWave == null)
            endWave = StartCoroutine(EndWave());
    }

    IEnumerator EndWave()
    {
        yield return new WaitForSeconds(1);

        isNight = false;
        currentWave++;

        StartCoroutine(NextWaveTimer());

        endWave = null;
        Debug.Log("End Wave");
    }

    void UpdateWaveMultipliers()
    {
        batchMultiplier = MultiplierStart 
                          + (int)Mathf.Floor(currentWave / 5);
        damageMultiplier = MultiplierStart + currentWave / 10;
        healthMultiplier = MultiplierStart + currentWave / 10;
        speedMultiplier = MultiplierStart + currentWave / 10;
        sizeMultiplier = MultiplierStart + currentWave / 20;
        spawnSpeedMultiplier = Mathf.Clamp(MultiplierStart 
                               - currentWave / 100, 0.1f, 100);
    }
}
