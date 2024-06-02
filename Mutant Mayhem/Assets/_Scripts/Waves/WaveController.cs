using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaveController : MonoBehaviour
{
    public List<WaveSOBase> waveBases;
    [SerializeField] WaveSpawner waveSpawner;
    [SerializeField] FadeCanvasGroupsWave nextWaveFadeGroup;
    [SerializeField] FadeCanvasGroupsWave waveInfoFadeGroup;
    [SerializeField] TextMeshProUGUI enemyCountText;
    [SerializeField] TextMeshProUGUI currentNightText;
    [SerializeField] TextMeshProUGUI nextWaveText;
    public int currentWave = 0;
    public float timeBetweenWaves = 60f;
    public float spawnRadius = 60;
    public int wavesPerBase = 3;


    public int MultiplierStart = 1;
    public static int batchMultiplier = 1;
    public static float damageMultiplier = 1;
    public static float healthMultiplier = 1;
    public static float speedMultiplier = 1;
    public static float sizeMultiplier = 1;
    public static float spawnSpeedMultiplier = 1;

    InputActionMap playerActionMap;
    InputAction nextWaveAction;
    Player player;

    Coroutine nextWaveTimer;
    public bool isNight;


    void Awake()
    {
        player = FindObjectOfType<Player>();
        playerActionMap = player.inputAsset.FindActionMap("Player");
        nextWaveAction = playerActionMap.FindAction("NextWave");

        batchMultiplier = MultiplierStart;
        damageMultiplier = MultiplierStart;
        healthMultiplier = MultiplierStart;
        speedMultiplier = MultiplierStart;
        sizeMultiplier = MultiplierStart;
        spawnSpeedMultiplier = MultiplierStart;
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

        waveSpawner.currentWave = waveBases[0];
    }

    void OnNextWaveInput(InputAction.CallbackContext context)
    {
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
        float countdown = timeBetweenWaves;

        while (countdown >= 0)
        {
            nextWaveText.text = "Time until night " 
                + currentWave + ":  " + countdown.ToString("#") + "s.  Press 'Enter' to skip";
            
            yield return new WaitForEndOfFrame();
            countdown -= Time.deltaTime;        
        }

        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {
        isNight = true;

        // Switch Spawner to new WaveBase
        int index = (int)Mathf.Floor(currentWave / wavesPerBase);
        index = Mathf.Clamp(index, 0, waveBases.Count - 1);
        waveSpawner.currentWave = waveBases[index];
        // Need to add logic to handle the end of the list.
        // Maybe restart with larger multipliers?  Or a random new list?

        if (nextWaveTimer != null)
            StopCoroutine(nextWaveTimer);
        
        Debug.Log("Start Wave");
        waveSpawner.StartWave();

        // Set wave UI text
        waveInfoFadeGroup.isTriggered = true;
        nextWaveFadeGroup.isTriggered = false;
        nextWaveText.enabled = false;

        // Wait 5 seconds before checking wave complete
        float timeElapsed = 0;
        while (timeElapsed < 5)
        {
            enemyCountText.text = "Enemies Detected: " + WaveSpawner.EnemyCount;
            currentNightText.text = "Night " + currentWave;

            yield return new WaitForEndOfFrame();
            timeElapsed += Time.deltaTime; 
        }

        // Check for end of wave
        while (!waveSpawner.waveComplete)
        {
            
            enemyCountText.text = "Enemies Detected: " + WaveSpawner.EnemyCount;
            currentNightText.text = "Night " + currentWave;

            yield return new WaitForEndOfFrame(); 
        }
        
        EndWave();
    }

    void EndWave()
    {
        isNight = false;

        Debug.Log("End Wave");
        currentWave++;

        UpdateWaveMultipliers();

        StartCoroutine(NextWaveTimer());
    }

    void UpdateWaveMultipliers()
    {
        batchMultiplier = MultiplierStart 
                          + (int)Mathf.Floor((currentWave - 1) / 10);
        damageMultiplier = MultiplierStart + (currentWave - 1) / 10;
        healthMultiplier = MultiplierStart + (currentWave - 1) / 10;
        sizeMultiplier = MultiplierStart + (currentWave - 1) / 20;
        spawnSpeedMultiplier = Mathf.Clamp(MultiplierStart 
                               - (currentWave - 1) / 100, 0.1f, 100);
    }
}
