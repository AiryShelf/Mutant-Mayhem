using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaveController : MonoBehaviour
{
    public static WaveController Instance = null;

    public WaveSpawnerRandom waveSpawner;
    public static event Action<int> OnWaveStarted;
    public static event Action<int> OnWaveEnded;
    public string textFlyPoolName = "TextFlyPool";
    public float textFlyMaxScale = 2f;
    public Canvas gameplayCanvas;

    [Header("UI Wave Info")]
    public FadeCanvasGroupsWave nextWaveFadeGroup;
    [SerializeField] FadeCanvasGroupsWave waveInfoFadeGroup;
    [SerializeField] TextMeshProUGUI enemyCountText;
    [SerializeField] TextMeshProUGUI currentNightText;
    [SerializeField] TextMeshProUGUI nextWaveText;
    public TextMeshProUGUI nextWaveButtonName; 

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
    public float batchMultStart = 5; // Starting batch multiplier for each Subwave
    public float batchMultiplier = 5; // Current batch multiplier
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

    bool isPlanetCompletedOnStart = false;

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

    public void StartWaveSequence()
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
        isPlanetCompletedOnStart = ProfileManager.Instance.IsPlanetCompleted(PlanetManager.Instance.currentPlanet);
    }

    #region Next Wave Input

    public void NextWaveButtonPressed()
    {
        if (InputManager.LastUsedDevice == Touchscreen.current)
            OnNextWaveInput(new InputAction.CallbackContext());
    }

    void OnNextWaveInput(InputAction.CallbackContext context)
    {
        //Debug.Log("NextWave Input detected");
        if (!isNight)
        {
            // Regen for remaining time
            int healthGain = Mathf.FloorToInt(player.stats.playerHealthScript.healthRegenPerSec * countdown);
            player.stats.playerHealthScript.ModifyHealth(healthGain, 1, Vector2.one, gameObject);
            int staminaGain = Mathf.FloorToInt(player.stats.staminaRegen * countdown);
            player.myStamina.ModifyStamina(staminaGain);

            // Drone hangar regen for remaining time
            foreach (DroneContainer dc in DroneManager.Instance.droneContainers)
            {
                int energyGain = Mathf.FloorToInt(DroneManager.Instance.droneHangarRechargeSpeed * countdown);
                dc.RechargeDockedDronesEnergy(energyGain);
                int repairGain = Mathf.FloorToInt(DroneManager.Instance.droneHangarRepairSpeed * countdown);
                dc.RepairDockedDrones(repairGain); 
            }

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
            nextWaveText.text = $"Night {currentWaveIndex + 1} starts in {countdown}s. {nextWaveButtonName.text} to skip";

            yield return new WaitForSeconds(1);
            countdown--;

            if (countdown == 15)
            {
                MessageBanner.PulseMessage(Mathf.CeilToInt(countdown) + $" seconds until night {currentWaveIndex + 1}!", Color.red);
            }
            else if (countdown == 10)
            {
                MessageBanner.PulseMessage(Mathf.CeilToInt(countdown) + $" seconds until night {currentWaveIndex + 1}!", Color.red);
            }
            else if (countdown == 5)
            {
                MessageBanner.PulseMessage(Mathf.CeilToInt(countdown) + $" seconds until night {currentWaveIndex + 1}!", Color.red);
            }
        }

        StopAllCoroutines();
        StartCoroutine(StartWave());
    }

    #endregion

    #region Start / End Wave

    IEnumerator StartWave()
    {
        MessageBanner.PulseMessage("Night " + (currentWaveIndex + 1) + " started!", Color.red);

        daylight.StartCoroutine(daylight.PlaySunsetEffect());
        isNight = true;
        MusicManager.Instance.SwitchScenePlaylists(MusicManager.Instance.nightPlaylists);
        MusicManager.Instance.PlayNextSong(3f);
        IncrementWaveDifficulty();

        if (nextWaveTimer != null)
            StopCoroutine(nextWaveTimer);

        waveSpawner.StartWave();
        OnWaveStarted?.Invoke(currentWaveIndex);

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

        OnWaveEnded?.Invoke(currentWaveIndex);

        ApplyResearchPoints();

        // Change music to day
        MusicManager.Instance.SwitchScenePlaylists(MusicManager.Instance.dayPlaylists);
        MusicManager.Instance.PlayNextSong(3f);

        isNight = false;
        currentWaveIndex++;
        BuildingSystem.PlayerCredits += currentWaveIndex * creditsPerWave;

        AnalyticsManager.Instance.TrackNightCompleted(currentWaveIndex);

        waveSpawner.CalculateMaxIndex();

        if (currentWaveIndex >= PlanetManager.Instance.currentPlanet.nightToSurvive)
            ProfileManager.Instance.SetPlanetCompleted(PlanetManager.Instance.currentPlanet.bodyName);

        StartCoroutine(NextWaveTimer());

        endWave = null;
        //Debug.Log("End Wave");

        daylight.StartCoroutine(daylight.PlaySunriseEffect());
    }

    public int GetResearchPointsForWave(int waveIndex)
    {
        PlanetSO p = PlanetManager.Instance.currentPlanet;
        float points = p.pointsPerWave;
        float growth = p.growthControlFactor;

        // reward(i) = A * (1 + C * (2i))
        float wavePts = points * (1f + growth * (2 * waveIndex + 1));
        int pts = Mathf.CeilToInt(wavePts);

        if (isPlanetCompletedOnStart)
        {
            pts = Mathf.CeilToInt(pts * 0.5f);
        }

        return pts;
    }

    public int GetResearchPointsTotal(int nightsSurvived)
    {
        nightsSurvived--; // Adjust since nightsSurvived is 1-based
        if (nightsSurvived < 0)
            return 0;

        int total = 0;
        for (int wave = 0; wave <= nightsSurvived; wave++)
        {
            total += GetResearchPointsForWave(wave);
        }

        return total;
    } 

    void ApplyResearchPoints()
    {
        // Apply research points for wave survived
        int researchPointsGained = GetResearchPointsForWave(currentWaveIndex);
        ProfileManager.Instance.currentProfile.totalNightsSurvived++;
        ProfileManager.Instance.currentProfile.researchPoints += researchPointsGained;
        ProfileManager.Instance.SaveCurrentProfile();

        // Set textFly
        GameObject textFly = PoolManager.Instance.GetFromPool(textFlyPoolName);
        TextFly textFlyComp = textFly.GetComponent<TextFly>();
        // Center screen pos
        textFly.transform.SetParent(gameplayCanvas.transform, false);
        RectTransform rectTransform = textFly.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;

        // Set textFly and MessageBanner text
        string rpGainedCommas = researchPointsGained.ToString("N0");
        string rpTotalCommas = ProfileManager.Instance.currentProfile.researchPoints.ToString("N0");
        if (PlanetManager.Instance.currentPlanet.isTutorialPlanet)
        {
            textFlyComp.Initialize($"Survived Night {currentWaveIndex + 1}!", Color.cyan, 1, Vector2.up, false, textFlyMaxScale);
            MessageBanner.PulseMessageLong($"You survived Night {currentWaveIndex + 1}!", Color.cyan);
            return;
        }
        textFlyComp.Initialize($"Survived Night {currentWaveIndex + 1}! +{researchPointsGained} RP", 
                               Color.cyan, 1, Vector2.up, false, textFlyMaxScale);
        MessageBanner.PulseMessageLong($"You survived Night {currentWaveIndex + 1}! \n" +
            $"Gained {rpGainedCommas} Research Points! You now have {rpTotalCommas} RP!", Color.cyan);

        Debug.Log($"Player gained {researchPointsGained} research points for waveIndex {currentWaveIndex}.");
    }

    #endregion

    #region Wave Difficulty

    public void ApplyDifficultyToTimeBetweenWaves()
    {
        switch (SettingsManager.Instance.difficultyLevel)
        {
            case DifficultyLevel.Easy:
                timeBetweenWaves = timeBetweenWavesBase + 60;
                break;
            case DifficultyLevel.Normal:
                timeBetweenWaves = timeBetweenWavesBase;
                break;
            case DifficultyLevel.Hard:
                timeBetweenWaves = timeBetweenWavesBase - 30;
                break;
            default:
                Debug.LogError("Unknown difficulty level set in SettingsManager");
                return;
        }
    }

    void IncrementWaveDifficulty()
    {
        batchMultiplier = batchMultStart + (currentWaveIndex / batchMultGrowthTime *
                          SettingsManager.Instance.WaveDifficultyMult);
        damageMultiplier = damageMultStart + (currentWaveIndex / damageMultGrowthTime *
                           SettingsManager.Instance.WaveDifficultyMult);
        // Doubles the attack speed in 30 waves
        attackDelayMult = attackDelayStart - currentWaveIndex / attackDelayMultGrowthTime /
                           SettingsManager.Instance.WaveDifficultyMult;
        attackDelayMult = Mathf.Clamp(attackDelayMult, 0.2f, float.MaxValue);
        healthMultiplier = healthMultStart + currentWaveIndex / healthMultGrowthTime *
                           SettingsManager.Instance.WaveDifficultyMult;
        speedMultiplier = speedMultStart + currentWaveIndex / speedMultGrowthTime *
                          SettingsManager.Instance.WaveDifficultyMult;
        sizeMultiplier = sizeMultStart + currentWaveIndex / sizeMultGrowthTime *
                         SettingsManager.Instance.WaveDifficultyMult;
        subwaveDelayMult = Mathf.Clamp(subwaveDelayMultStart - currentWaveIndex / subwaveDelayMultGrowthTime *
                           SettingsManager.Instance.WaveDifficultyMult, 0.1f, 20);
    }
    
    #endregion
}
