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
    public float spawnRadius = 60; // DEPRECATED, now uses dynamic radius plus a buffer set by planetSO
    public float wavesTillAddBase = 1; // Affects max index to choose subwaves from
    public float subwaveDelayMultStart = 1f;
    public float subwaveDelayMult = 1;
    public int spawnRadiusBuffer = 16;

    [Header("Enemy Multipliers")]
    public int batchMultStart = 5; // Starting batch multiplier for each Subwave
    public int batchMultiplier = 5; // Current batch multiplier
    public float damageMultStart = 1;
    public float damageMultiplier = 1;
    public float attackDelayStart = 1;
    public float attackDelayMult = 1;
    public float healthMultStart = 1;
    public float healthMultiplier = 1;
    public float speedMultStart = 1;
    public float speedMultiplier = 1;
    public float sizeMultStart = 1;
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
            waveInfoFadeGroup.isTriggered = true;
            nextWaveFadeGroup.isTriggered = false;
            
            currentNightText.text = "Night " + (currentWaveIndex + 1);
        }
        else
        {
            waveInfoFadeGroup.isTriggered = false;
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
        batchMultiplier = Mathf.FloorToInt(batchMultStart + currentWaveIndex / 2 * 
                          SettingsManager.Instance.WaveDifficultyMult);
        damageMultiplier = damageMultStart + currentWaveIndex / 5f * 
                           SettingsManager.Instance.WaveDifficultyMult *
                           PlanetManager.Instance.statMultipliers[PlanetStatModifier.EnemyDamage];
        // Doubles the attack speed in 30 waves
        attackDelayMult = attackDelayStart - currentWaveIndex / 60f / 
                           SettingsManager.Instance.WaveDifficultyMult;
        attackDelayMult = Mathf.Clamp(attackDelayMult, 0.2f, float.MaxValue);
        healthMultiplier = healthMultStart + currentWaveIndex / 20f * 
                           SettingsManager.Instance.WaveDifficultyMult *
                           PlanetManager.Instance.statMultipliers[PlanetStatModifier.EnemyDamage];
        speedMultiplier = speedMultStart + currentWaveIndex / 20f *  
                          SettingsManager.Instance.WaveDifficultyMult *
                          PlanetManager.Instance.statMultipliers[PlanetStatModifier.EnemyMoveSpeed];
        sizeMultiplier = sizeMultStart + currentWaveIndex / 40f *   
                         SettingsManager.Instance.WaveDifficultyMult *
                         PlanetManager.Instance.statMultipliers[PlanetStatModifier.EnemySize];
        subwaveDelayMult = Mathf.Clamp(subwaveDelayMultStart - currentWaveIndex / 40f * 
                           SettingsManager.Instance.WaveDifficultyMult, 0.1f, 20);
    }
}
