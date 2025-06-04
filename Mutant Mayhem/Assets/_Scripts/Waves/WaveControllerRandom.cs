using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaveControllerRandom : MonoBehaviour
{
    public static WaveControllerRandom Instance = null;

    public WaveSpawnerRandom waveSpawner;

    [Header("UI Wave Info")]
    public FadeCanvasGroupsWave nextWaveFadeGroup;
    [SerializeField] FadeCanvasGroupsWave waveInfoFadeGroup;
    [SerializeField] TextMeshProUGUI enemyCountText;
    [SerializeField] TextMeshProUGUI currentNightText;
    [SerializeField] TextMeshProUGUI nextWaveText;
    public TextMeshProUGUI nextWaveButtonName; // Used to store and universally access a 'string' 

    [Header("Wave Properties, mostly set by Planets")]
    public int creditsPerWave = 150; // Additive bonus (waveIndex*creditsPerWave)
    public int currentWaveIndex = 0;
    public float timeBetweenWavesBase = 90; // Base amount of day-time
    public float timeBetweenWaves = 0; // Amount of day-time after difficulty setting
    public float wavesTillAddIndex = 1; // Waves until raising max index to choose subwaves from
    public float subwaveDelayMultStart = 1f;
    public float subwaveDelayMult = 1;
    public int spawnRadiusBuffer = 16;

    [Header("Enemy Multipliers, mostly set by Planets")]
    public int batchMultStart = 5; // Starting batch multiplier for each Subwave
    public int batchMultiplier = 5; // Current batch multiplier
    public int batchMultGrowthTime; // Divisor for mult growth.  Value is the number of waves until mult is doubled
    public float damageMultStart = 1;
    public float damageMultiplier = 1;
    public int damageMultGrowthTime;
    public float attackDelayStart = 1;
    public float attackDelayMult = 1;
    public int attackDelayMultGrowthTime;
    public float healthMultStart = 1;
    public float healthMultiplier = 1;
    public int healthMultGrowthTime;
    public float speedMultStart = 1;
    public float speedMultiplier = 1;
    public int speedMultGrowthTime;
    public float sizeMultStart = 1;
    public float sizeMultiplier = 1;
    public int sizeMultGrowthTime;
    public int subwaveDelayMultGrowthTime;

    InputActionMap playerActionMap;
    InputAction nextWaveAction;
    Player player;

    Coroutine nextWaveTimer;
    Coroutine endWave;
    public bool isNight;
    Daylight daylight;
    float countdown;

    void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDisable()
    {
        if (nextWaveAction != null)
            nextWaveAction.performed -= OnNextWaveInput;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
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

    #region Next Wave Input

    public void NextWaveButtonPressed()
    {
        if (InputManager.LastUsedDevice == Touchscreen.current)
            OnNextWaveInput(new InputAction.CallbackContext());
    }

    void OnNextWaveInput(InputAction.CallbackContext context)
    {
        Debug.Log("NextWave Input detected");
        if (nextWaveFadeGroup.isTriggered)
        {
            int healthGain = Mathf.FloorToInt(player.stats.playerHealthScript.healthRegenPerSec * countdown);
            player.stats.playerHealthScript.ModifyHealth(healthGain, 1, Vector2.one, gameObject);

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

        countdown = timeBetweenWaves;
        while (countdown > 0)
        {
            nextWaveText.text = $"Time until night {currentWaveIndex + 1}: " + countdown.ToString("#") + $"s.  {nextWaveButtonName.text} to skip";

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

    #endregion

    #region Start / End Wave

    IEnumerator StartWave()
    {
        MessagePanel.PulseMessage("Night " + (currentWaveIndex + 1) + " started!", Color.red);

        daylight.StartCoroutine(daylight.PlaySunsetEffect());
        isNight = true;
        IncrementWaveDifficulty();

        if (nextWaveTimer != null)
            StopCoroutine(nextWaveTimer);

        waveSpawner.StartWave();
        EvolutionManager.Instance.SpawnWave();

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
        BuildingSystem.PlayerCredits += currentWaveIndex * creditsPerWave;

        EvolutionManager.Instance.EndWaveAndEvolve();

        if (currentWaveIndex >= PlanetManager.Instance.currentPlanet.nightToSurvive)
            ProfileManager.Instance.SetPlanetCompleted(PlanetManager.Instance.currentPlanet.bodyName);

        StartCoroutine(NextWaveTimer());

        endWave = null;
        //Debug.Log("End Wave");

        daylight.StartCoroutine(daylight.PlaySunriseEffect());
    }

    #endregion

    #region Wave Difficulty

    void IncrementWaveDifficulty()
    {
        batchMultiplier = Mathf.FloorToInt(batchMultStart + currentWaveIndex / batchMultGrowthTime *
                          SettingsManager.Instance.WaveDifficultyMult);
        damageMultiplier = damageMultStart + currentWaveIndex / damageMultGrowthTime *
                           SettingsManager.Instance.WaveDifficultyMult *
                           PlanetManager.Instance.statMultipliers[PlanetStatModifier.EnemyDamage];
        // Doubles the attack speed in 30 waves
        attackDelayMult = attackDelayStart - currentWaveIndex / attackDelayMultGrowthTime /
                           SettingsManager.Instance.WaveDifficultyMult;
        attackDelayMult = Mathf.Clamp(attackDelayMult, 0.2f, float.MaxValue);
        healthMultiplier = healthMultStart + currentWaveIndex / healthMultGrowthTime *
                           SettingsManager.Instance.WaveDifficultyMult *
                           PlanetManager.Instance.statMultipliers[PlanetStatModifier.EnemyDamage];
        speedMultiplier = speedMultStart + currentWaveIndex / speedMultGrowthTime *
                          SettingsManager.Instance.WaveDifficultyMult *
                          PlanetManager.Instance.statMultipliers[PlanetStatModifier.EnemyMoveSpeed];
        sizeMultiplier = sizeMultStart + currentWaveIndex / sizeMultGrowthTime *
                         SettingsManager.Instance.WaveDifficultyMult *
                         PlanetManager.Instance.statMultipliers[PlanetStatModifier.EnemySize];
        subwaveDelayMult = Mathf.Clamp(subwaveDelayMultStart - currentWaveIndex / subwaveDelayMultGrowthTime *
                           SettingsManager.Instance.WaveDifficultyMult, 0.1f, 20);
    }
    
    #endregion
}
