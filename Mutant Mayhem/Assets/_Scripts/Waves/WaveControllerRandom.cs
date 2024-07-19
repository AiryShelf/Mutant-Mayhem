using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaveControllerRandom : MonoBehaviour
{
    [SerializeField] List<WaveSOBase> allWaveBases;
    [SerializeField] WaveSpawnerRandom waveSpawner;

    [Header("UI Wave Info")]
    [SerializeField] FadeCanvasGroupsWave nextWaveFadeGroup;
    [SerializeField] FadeCanvasGroupsWave waveInfoFadeGroup;
    [SerializeField] TextMeshProUGUI enemyCountText;
    [SerializeField] TextMeshProUGUI currentNightText;
    [SerializeField] TextMeshProUGUI nextWaveText;

    [Header("Wave Properties")]
    public int currentWaveCount = 0;
    public float timeBetweenWavesBase = 90;
    public float timeBetweenWaves = 90f;
    public float spawnRadius = 60;
    public int wavesPerBase = 2;

    [Header("Enemy Multipliers")]
    public int batchMultiplierStart = 5;
    public int batchMultiplier = 5;
    public int multiplierStart = 1;
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
    Daylight daylight;


    void Awake()
    {
        player = FindObjectOfType<Player>();
        playerActionMap = player.inputAsset.FindActionMap("Player");
        nextWaveAction = playerActionMap.FindAction("NextWave");

        daylight = FindObjectOfType<Daylight>();
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

        // Testing MaserWave and dynamically building currentWave
        //waveSpawner.currentWaveSource = allWaveBases[0];
        currentWaveCount = 0;
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
        StartCoroutine(StartWave());
    }

    IEnumerator NextWaveTimer()
    {
        waveInfoFadeGroup.isTriggered = false;
        nextWaveText.enabled = true;
        nextWaveFadeGroup.isTriggered = true;
        float countdown = timeBetweenWaves;

        while (countdown >= 0)
        {
            nextWaveText.text = "Time until night " + (currentWaveCount + 1) + 
                ":  " + countdown.ToString("#") + "s.  Press 'Enter' to skip";
            
            yield return new WaitForEndOfFrame();
            countdown -= Time.deltaTime;        
        }

        TriggerWaves();
    }

    IEnumerator StartWave()
    {
        daylight.StartCoroutine(daylight.PlaySunsetEffect());
        isNight = true;
        UpdateWaveMultipliers();

        // Switch Spawner to new WaveBase
        //int index = (int)Mathf.Floor(currentWave / wavesPerBase);
        //index = Mathf.Clamp(index, 0, allWaveBases.Count - 1);
        //waveSpawner.currentWaveSource = allWaveBases[index];
        // Need to add logic to handle the end of the list.
        // Maybe restart with larger multipliers?  Or a random new list?

        if (nextWaveTimer != null)
            StopCoroutine(nextWaveTimer);
        
        waveSpawner.StartWave();

        // Set wave UI text
        waveInfoFadeGroup.isTriggered = true;
        nextWaveFadeGroup.isTriggered = false;
        nextWaveText.enabled = false;
        currentNightText.text = "Night " + (currentWaveCount + 1);

        // Wait 5 seconds before checking wave complete
        float timeElapsed = 0;
        while (timeElapsed < 5)
        {
            enemyCountText.text = "Enemies Detected: " + EnemyCounter.EnemyCount;

            yield return new WaitForEndOfFrame();
            timeElapsed += Time.deltaTime; 
        }

        // Check for end of wave
        while (!waveSpawner.waveComplete)
        {      
            enemyCountText.text = "Enemies Detected: " + EnemyCounter.EnemyCount;

            yield return new WaitForEndOfFrame(); 
        }
        
        // Let one wave initiate ending, if multiple
        if (endWave == null)
            endWave = StartCoroutine(EndWave());
    }

    IEnumerator EndWave()
    {
        yield return new WaitForSeconds(1);

        isNight = false;
        currentWaveCount++;

        StartCoroutine(NextWaveTimer());

        endWave = null;
        //Debug.Log("End Wave");

        daylight.StartCoroutine(daylight.PlaySunriseEffect());
    }

    void UpdateWaveMultipliers()
    {
        batchMultiplier = Mathf.FloorToInt(batchMultiplierStart + currentWaveCount / 
                          wavesPerBase / 2 * SettingsManager.Instance.WaveDifficultyMult);
        damageMultiplier = multiplierStart + currentWaveCount / 40f * 
                           SettingsManager.Instance.WaveDifficultyMult;
        healthMultiplier = multiplierStart + currentWaveCount / 40f * 
                           SettingsManager.Instance.WaveDifficultyMult;
        speedMultiplier = multiplierStart + currentWaveCount / 40f *  
                           SettingsManager.Instance.WaveDifficultyMult;
        sizeMultiplier = multiplierStart + currentWaveCount / 40f *   
                           SettingsManager.Instance.WaveDifficultyMult;
        spawnSpeedMultiplier = Mathf.Clamp(multiplierStart - currentWaveCount / 100f * 
                           SettingsManager.Instance.WaveDifficultyMult, 0.1f, 100);
    }
}
