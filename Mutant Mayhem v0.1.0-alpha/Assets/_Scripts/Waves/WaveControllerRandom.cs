using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaveControllerRandom : MonoBehaviour
{
    [SerializeField] WaveSpawnerRandom waveSpawner;

    [Header("UI Wave Info")]
    [SerializeField] FadeCanvasGroupsWave nextWaveFadeGroup;
    [SerializeField] FadeCanvasGroupsWave waveInfoFadeGroup;
    [SerializeField] TextMeshProUGUI enemyCountText;
    [SerializeField] TextMeshProUGUI currentNightText;
    [SerializeField] TextMeshProUGUI nextWaveText;

    [Header("Wave Properties")]
    public int currentWaveCount = 0;
    public float timeBetweenWavesBase = 90; // Base amount of day-time
    public float _timeBetweenWaves; // Amount of day-time after difficulty setting
    public float spawnRadius = 60; // Radius around the cube that enemies spawn
    public int wavesPerBase = 2; // Affects batch multiplier and max index to choose subwaves from

    [Header("Enemy Multipliers")]
    public int batchMultiplierStart = 5; // Starting batch multiplier for each Subwave
    public int batchMultiplier = 5; // Current batch multiplier
    public int multiplierStart = 1; // Applies it's multiplier to enemy stats multipliers below
    public float damageMultiplier = 1;
    public float healthMultiplier = 1;
    public float speedMultiplier = 1;
    public float sizeMultiplier = 1;
    public float spawnSpeedMult = 1;

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
        // Start the daytime counter
        if (nextWaveTimer != null)
            StopCoroutine(nextWaveTimer);
        nextWaveTimer = StartCoroutine(NextWaveTimer());

        // Testing MaserWave and dynamically building currentWave
        //waveSpawner.currentWaveSource = allWaveBases[0];
        currentWaveCount = 0;
    }

    void OnNextWaveInput(InputAction.CallbackContext context)
    {
        // When Enter is pressed to start next wave
        if (nextWaveText.enabled)
        {
            StopAllCoroutines();
            StartCoroutine(StartWave());
        }
    }

    IEnumerator NextWaveTimer()
    {
        waveInfoFadeGroup.isTriggered = false;
        nextWaveText.enabled = true;
        nextWaveFadeGroup.isTriggered = true;

        float countdown = _timeBetweenWaves;
        while (countdown >= 0)
        {
            nextWaveText.text = "Time until night " + (currentWaveCount + 1) + 
                ":  " + countdown.ToString("#") + "s.  Press 'Enter' to skip";
            
            yield return new WaitForEndOfFrame();
            countdown -= Time.deltaTime;        
        }

        StopAllCoroutines();
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {
        daylight.StartCoroutine(daylight.PlaySunsetEffect());
        isNight = true;
        ApplyDifficultyToMult();

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

    void ApplyDifficultyToMult()
    {
        batchMultiplier = Mathf.FloorToInt(batchMultiplierStart + currentWaveCount / 
                          wavesPerBase / 2 * SettingsManager.Instance.WaveDifficultyMult);
        damageMultiplier = multiplierStart + currentWaveCount / 20f * 
                           SettingsManager.Instance.WaveDifficultyMult;
        healthMultiplier = multiplierStart + currentWaveCount / 20f * 
                           SettingsManager.Instance.WaveDifficultyMult;
        speedMultiplier = multiplierStart + currentWaveCount / 20f *  
                           SettingsManager.Instance.WaveDifficultyMult;
        sizeMultiplier = multiplierStart + currentWaveCount / 20f *   
                           SettingsManager.Instance.WaveDifficultyMult;
        spawnSpeedMult = Mathf.Clamp(multiplierStart - currentWaveCount / 20f * 
                           SettingsManager.Instance.WaveDifficultyMult, 0.1f, 20);
    }
}
