using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaveControllerRandom : MonoBehaviour
{
    [SerializeField] WaveSpawnerRandom waveSpawner;

    [Header("UI Wave Info")]
    public FadeCanvasGroupsWave nextWaveFadeGroup;
    [SerializeField] FadeCanvasGroupsWave waveInfoFadeGroup;
    [SerializeField] TextMeshProUGUI enemyCountText;
    [SerializeField] TextMeshProUGUI currentNightText;
    [SerializeField] TextMeshProUGUI nextWaveText;

    [Header("Wave Properties")]
    public int currentWaveIndex = 0;
    public float timeBetweenWavesBase = 90; // Base amount of day-time
    public float timeBetweenWaves = 0; // Amount of day-time after difficulty setting
    public float spawnRadius = 60; // Radius around the cube that enemies spawn
    public float wavesTillAddBase = 1; // Affects batch multiplier and max index to choose subwaves from
    public float spawnSpeedStart = 1f;
    public float subwaveDelayMult = 1;

    [Header("Enemy Multipliers")]
    public int batchMultiplierStart = 5; // Starting batch multiplier for each Subwave
    public int batchMultiplier = 5; // Current batch multiplier
    public int enemyStatMultStart = 1; // Applies it's multiplier to enemy stats multipliers below
    public float damageMultiplier = 1; // Current multipliers
    public float healthMultiplier = 1;
    public float speedMultiplier = 1;
    public float sizeMultiplier = 1;
    
    InputActionMap playerActionMap;
    InputAction nextWaveAction;
    Player player;

    Coroutine nextWaveTimer;
    Coroutine endWave;
    public bool isNight;
    Daylight daylight;

    void OnDisable()
    {
        if (nextWaveAction != null)
            nextWaveAction.performed -= OnNextWaveInput;
    }

    public void Initialize()
    {
        player = FindObjectOfType<Player>();
        playerActionMap = player.inputAsset.FindActionMap("Player");
        nextWaveAction = playerActionMap.FindAction("NextWave");
        nextWaveAction.performed += OnNextWaveInput;

        daylight = FindObjectOfType<Daylight>();
        currentWaveIndex = 0;

        // Start the daytime counter
        if (nextWaveTimer != null)
            StopCoroutine(nextWaveTimer);
        nextWaveTimer = StartCoroutine(NextWaveTimer());

        // Testing MaserWave and dynamically building currentWave
        //waveSpawner.currentWaveSource = allWaveBases[0];
        
    }

    void OnNextWaveInput(InputAction.CallbackContext context)
    {
        // When Enter is pressed to start next wave
        if (nextWaveFadeGroup.isTriggered)
        {
            StopAllCoroutines();
            StartCoroutine(StartWave());
        }
    }

    void UpdateWaveTimer(bool isNight)
    {
        if (isNight)
        {
            //Debug.Log("Wave info turned on, next wave info turned off");
            waveInfoFadeGroup.isTriggered = true;
            //nextWaveText.enabled = false;
            nextWaveFadeGroup.isTriggered = false;
            
            currentNightText.text = "Night " + (currentWaveIndex + 1);
        }
        else
        {
            //Debug.Log("Wave info turned off, next wave info turned on with time: " + timeBetweenWaves);
            waveInfoFadeGroup.isTriggered = false;
            //nextWaveText.enabled = true;
            nextWaveFadeGroup.isTriggered = true;
        }
    }

    IEnumerator NextWaveTimer()
    {
        yield return new WaitForFixedUpdate();

        UpdateWaveTimer(false);

        float countdown = timeBetweenWaves;
        while (countdown > 0)
        {
            nextWaveText.text = "Time until night " + (currentWaveIndex + 1) + 
                ":  " + countdown.ToString("#") + "s.  Press 'Enter' to skip";
            
            yield return new WaitForSeconds(1);
            countdown--;

            if (countdown <= 10)
            {
                MessagePanel.PulseMessage(Mathf.CeilToInt(countdown) + " seconds to the next night!", Color.red);
            }
            
        }

        StopAllCoroutines();
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {
        MessagePanel.PulseMessage("Night " + (currentWaveIndex + 1) + " started!", Color.red);

        daylight.StartCoroutine(daylight.PlaySunsetEffect());
        isNight = true;
        IncrementWaveDifficulty();

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
        UpdateWaveTimer(true);

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
        MessagePanel.PulseMessage("You survived night " + (currentWaveIndex + 1) + "!", Color.cyan);
        currentWaveIndex++;

        StartCoroutine(NextWaveTimer());

        endWave = null;
        //Debug.Log("End Wave");

        daylight.StartCoroutine(daylight.PlaySunriseEffect());
    }

    void IncrementWaveDifficulty()
    {
        batchMultiplier = Mathf.FloorToInt(batchMultiplierStart + currentWaveIndex / 2 * SettingsManager.Instance.WaveDifficultyMult);
        damageMultiplier = enemyStatMultStart + currentWaveIndex / 20f * 
                           SettingsManager.Instance.WaveDifficultyMult;
        healthMultiplier = enemyStatMultStart + currentWaveIndex / 20f * 
                           SettingsManager.Instance.WaveDifficultyMult;
        speedMultiplier = enemyStatMultStart + currentWaveIndex / 20f *  
                           SettingsManager.Instance.WaveDifficultyMult;
        sizeMultiplier = enemyStatMultStart + currentWaveIndex / 20f *   
                           SettingsManager.Instance.WaveDifficultyMult;
        subwaveDelayMult = Mathf.Clamp(spawnSpeedStart - currentWaveIndex / 40f * 
                           SettingsManager.Instance.WaveDifficultyMult, 0.1f, 20);
    }
}
