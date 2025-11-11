using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements.Experimental;
using Unity.VisualScripting;

public class HUDStatsPanel : MonoBehaviour
{
    public static HUDStatsPanel Instance { get; private set; }

    [SerializeField] float textFlyAlphaMax;
    [SerializeField] float textPulseScaleMax = 1.5f;

    [Header("Player Stats")]
    [SerializeField] TextMeshProUGUI creditsText;
    [SerializeField] Slider healthSlider;
    [SerializeField] TextMeshProUGUI healthValueText;
    [SerializeField] Slider staminaSlider;
    [SerializeField] TextMeshProUGUI staminaValueText;
    [SerializeField] Color textFlyCreditsGainColor;
    [SerializeField] Color textFlyCreditsLossColor;

    [Header("QCube Stats")]
    [SerializeField] Image qCubeImage;
    [SerializeField] QCubeHealth qCubeHealthScript;
    [SerializeField] Slider qCubeHealthSlider;
    [SerializeField] TextMeshProUGUI qCubeHealthValueText;

    [Header("QCube Damage Effect")]
    [SerializeField] Color qCubeDamageColor;
    [SerializeField] float qCubeShakeSpeed;
    [SerializeField] float qCubeShakeAmount;
    [SerializeField] float qCubeShakeTime;

    [Header("Power ")]
    [SerializeField] Image powerImage;
    [SerializeField] TextMeshProUGUI powerText;

    [Header("Supplies")]
    [SerializeField] Image supplyImage;
    [SerializeField] TextMeshProUGUI supplyText;

    Player player;
    PlayerStats playerStats;
    Health playerHealthScript;
    Stamina playerStaminaScript;
    float playerStamina;

    float qCubePevHealth;
    float qCubeCurrHealth;
    float qCubeMaxHealth;
    int previousCredits = 0;
    int previousHealth;
    
    QCubeHealth qCubeHealth;
    Color qCubeStartColor;
    Vector2 qCubeStartLocalPos;
    Coroutine shakeEffect;
    Coroutine flashEffect;
    bool initialized = false;
    Color powerIconColor;
    Color supplyIconColor;

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

        player = FindObjectOfType<Player>();
        playerStats = player.stats;
        playerHealthScript = player.GetComponent<Health>();
        playerStaminaScript = player.GetComponent<Stamina>();

        qCubeStartColor = qCubeImage.color;
        qCubeStartLocalPos = qCubeImage.transform.localPosition;
    }

    void Start()
    {
        UpdatePowerText(PowerManager.Instance.powerBalance);
        UpdateSupplyBalance(SupplyManager.SupplyBalance);
        StartCoroutine(DelayStatsUpdate());
    }

    void OnEnable()
    {
        BuildingSystem.OnPlayerCreditsChanged += UpdateCreditsText;
        qCubeHealthScript.OnCubeHealthChanged += UpdateQCubeStatsUI;
        player.stats.playerHealthScript.OnPlayerHealthChanged += UpdateHealthStats;
        player.stats.playerHealthScript.OnPlayerMaxHealthChanged += UpdateHealthStats;
        PowerManager.Instance.OnPowerChanged += UpdatePowerText;
        SupplyManager.OnSupplyBalanceChanged += UpdateSupplyBalance;
    }

    void OnDisable()
    {
        BuildingSystem.OnPlayerCreditsChanged -= UpdateCreditsText;
        qCubeHealthScript.OnCubeHealthChanged -= UpdateQCubeStatsUI;
        player.stats.playerHealthScript.OnPlayerHealthChanged -= UpdateHealthStats;
        player.stats.playerHealthScript.OnPlayerMaxHealthChanged -= UpdateHealthStats;
        PowerManager.Instance.OnPowerChanged -= UpdatePowerText;
        SupplyManager.OnSupplyBalanceChanged -= UpdateSupplyBalance;
    }

    void FixedUpdate()
    {
        UpdateStaminaStats();
    }

    void UpdatePowerText(int powerBalance)
    {
        if (powerBalance >= 0)
            powerIconColor = Color.white;
        else
            powerIconColor = Color.red;

        powerImage.color = powerIconColor;
        powerText.text = powerBalance.ToString();
        powerText.color = powerIconColor;
    }

    void UpdateSupplyBalance(int supplyBalance)
    {
        Color textColor;
        if (supplyBalance >= 0)
        {
            textColor = Color.green;
            supplyIconColor = Color.white;
        }
        else
        {
            supplyIconColor = Color.red;
        }

        supplyImage.color = supplyIconColor;
        supplyText.text = $"{SupplyManager.SupplyBalance}";
        supplyText.color = supplyIconColor;
    }

    IEnumerator DelayStatsUpdate()
    {
        yield return new WaitForEndOfFrame();

        UpdateHealthStats(player.stats.playerHealthScript.GetHealth());
        UpdateCreditsText(BuildingSystem.PlayerCredits);

        qCubeHealth = FindObjectOfType<QCubeHealth>();
        UpdateQCubeStatsUI(qCubeHealth.GetHealth());

        Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreen);
    }

    void UpdateStaminaStats()
    {
        playerStamina = playerStaminaScript.GetStamina();
        staminaSlider.value = playerStamina / playerStats.staminaMax;;
        staminaValueText.text = "Energy: " + Mathf.CeilToInt(playerStamina).ToString();
    }

    void UpdateHealthStats(float playerHealth)
    {
        float playerMaxHealth = playerHealthScript.GetMaxHealth();

        healthSlider.value = playerHealth / playerMaxHealth;
        healthValueText.text = "Health: " + Mathf.CeilToInt(playerHealth).ToString();

        int healthChange = Mathf.CeilToInt(playerHealth - previousHealth);
        if (Mathf.Abs(healthChange) < 1)
            return;

        previousHealth = Mathf.CeilToInt(playerHealth);
    }

    void UpdateCreditsText(float playerCredits)
    {
        int creditsChange =  Mathf.FloorToInt(playerCredits - previousCredits);
        if (creditsChange == 0 && initialized)
            return;

        TextFly textFly = PoolManager.Instance.GetFromPool("TextFlyUI_Credits").GetComponent<TextFly>();
        textFly.transform.SetParent(transform);
        Color textColor;
        textFly.transform.position = creditsText.transform.position;
        
        if (creditsChange < 0)
        {
            textColor = textFlyCreditsLossColor;
            float angle = (Random.Range(-45f, 45f) - 75) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
            textFly.Initialize("-" + creditsChange + " C", textColor, textFlyAlphaMax, dir, false, textPulseScaleMax);
        }
        else
        {
            textColor = textFlyCreditsGainColor;
            float angle = (Random.Range(-45f, 45f) + 75) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
            textFly.Initialize("+" + creditsChange + " C", textColor, textFlyAlphaMax, dir, false, textPulseScaleMax);
        }

        int credits = (int)BuildingSystem.PlayerCredits;
        creditsText.text = "Credits: " + credits.ToString("#0");
        previousCredits = credits;

        initialized = true;
    }

    void UpdateQCubeStatsUI(float cubeHealth)
    {
        qCubeCurrHealth = qCubeHealthScript.GetHealth();
        qCubeMaxHealth = qCubeHealthScript.GetMaxHealth();

        qCubeHealthSlider.value = qCubeCurrHealth / qCubeMaxHealth;
        qCubeHealthValueText.text = Mathf.CeilToInt(qCubeCurrHealth).ToString();

        UpdateDamageImage();
    }

    void UpdateDamageImage()
    {
        float healthRatio = 1 - (qCubeCurrHealth / qCubeMaxHealth);
        int sprIndex = Mathf.FloorToInt(healthRatio * qCubeHealthScript.damageSprites.Count);
        sprIndex = Mathf.Clamp(sprIndex, 0, qCubeHealthScript.damageSprites.Count - 1);

        qCubeImage.sprite = qCubeHealthScript.damageSprites[sprIndex];

        if (qCubeCurrHealth < qCubePevHealth)
        {
            if (shakeEffect != null)
                StopCoroutine(shakeEffect);
            shakeEffect = StartCoroutine(GameTools.ShakeTransform(
                                        qCubeImage.transform, qCubeStartLocalPos,
                                        qCubeShakeTime, qCubeShakeAmount, qCubeShakeSpeed));

            if (flashEffect != null)
                StopCoroutine(flashEffect);
            flashEffect = StartCoroutine(GameTools.FlashImage(
                                        qCubeImage, qCubeShakeTime, 
                                        qCubeShakeTime/2, qCubeDamageColor, qCubeStartColor));
        }
        else
        {
            // Add cube repair effect
        }

        qCubePevHealth = qCubeCurrHealth;
    }

    

}
