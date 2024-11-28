using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDStatsPanel : MonoBehaviour
{
    [SerializeField] float textFlyAlphaMax;

    [Header("Player Stats")]
    [SerializeField] TextMeshProUGUI creditsText;
    [SerializeField] Slider healthSlider;
    [SerializeField] TextMeshProUGUI healthValueText;
    [SerializeField] Slider staminaSlider;
    [SerializeField] TextMeshProUGUI staminaValueText;
    [SerializeField] Color textFlyCreditsGainColor;
    [SerializeField] Color textFlyCreditsLossColor;
    [SerializeField] Color textFlyHealthGainColor;
    [SerializeField] Color textFlyHealthLossColor;

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

    Player player;
    PlayerStats playerStats;
    Health playerHealthScript;
    Stamina playerStaminaScript;
    float playerHealth;
    float playerMaxHealth;
    float playerStamina;
    float playerMaxStamina;

    float qCubePevHealth;
    float qCubeCurrHealth;
    float qCubeMaxHealth;
    int previousCredits;
    int previousHealth;
    
    QCubeHealth qCubeHealth;
    Color qCubeStartColor;
    Vector2 qCubeStartLocalPos;
    Coroutine shakeEffect;
    Coroutine flashEffect;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        playerStats = player.stats;
        playerHealthScript = player.GetComponent<Health>();
        playerStaminaScript = player.GetComponent<Stamina>();

        qCubeStartColor = qCubeImage.color;
        qCubeStartLocalPos = qCubeImage.transform.localPosition;
    }

    void OnEnable()
    {
        BuildingSystem.OnPlayerCreditsChanged += UpdateCreditsText;
        qCubeHealthScript.OnCubeHealthChanged += UpdateQCubeStatsUI;
        player.stats.playerHealthScript.OnPlayerHealthChanged += UpdatePlayerStatsUI;
        player.stats.playerHealthScript.OnPlayerMaxHealthChanged += UpdatePlayerStatsUI;
    }

    void OnDisable()
    {
        BuildingSystem.OnPlayerCreditsChanged -= UpdateCreditsText;
        qCubeHealthScript.OnCubeHealthChanged -= UpdateQCubeStatsUI;
        player.stats.playerHealthScript.OnPlayerHealthChanged -= UpdatePlayerStatsUI;
        player.stats.playerHealthScript.OnPlayerMaxHealthChanged -= UpdatePlayerStatsUI;
    }

    void Start()
    {
        UpdatePlayerStatsUI(player.stats.playerHealthScript.GetHealth());
        UpdateStaminaStats();

        qCubeHealth = FindObjectOfType<QCubeHealth>();
        UpdateQCubeStatsUI(qCubeHealth.GetHealth());
    }

    void FixedUpdate()
    {
        UpdateStaminaStats();
    }

    void UpdateStaminaStats()
    {
        playerMaxStamina = playerStats.staminaMax;
        playerStamina = playerStaminaScript.GetStamina();
        staminaSlider.value = playerStamina / playerMaxStamina;
        staminaValueText.text = "Energy: " + Mathf.CeilToInt(playerStamina).ToString();
    }

    void UpdatePlayerStatsUI(float playerHealth)
    {
        playerMaxHealth = playerHealthScript.GetMaxHealth();

        healthSlider.value = playerHealth / playerMaxHealth;
        healthValueText.text = "Health: " + Mathf.CeilToInt(playerHealth).ToString();

        int healthChange = Mathf.CeilToInt(playerHealth - previousHealth);
        if (healthChange != 0)
        {
            Color textColor;
            // Gain health effect
            TextFly textFly = PoolManager.Instance.GetFromPool("TextFlyUI_Health").GetComponent<TextFly>();
            if (healthChange >= 0)
                textColor = textFlyHealthGainColor;
            else
                textColor = textFlyHealthLossColor;

            float angle = (Random.Range(-45f, 45f) + 75) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

            textFly.transform.position = healthValueText.transform.position;
            textFly.transform.SetParent(transform);
            textFly.Initialize("+ " + healthChange + " HP", textColor, textFlyAlphaMax, dir, false);
        }
        previousHealth = Mathf.CeilToInt(playerHealth);
    }

    void UpdateCreditsText(float playerCredits)
    {
        int creditsChange =  Mathf.FloorToInt(playerCredits - previousCredits);
        if (creditsChange != 0)
        {
            Color textColor;
            if (creditsChange >= 0)
                textColor = textFlyCreditsGainColor;
            else
                textColor = textFlyCreditsLossColor;

            // Gain credits effect
            TextFly textFly = PoolManager.Instance.GetFromPool("TextFlyUI_Credits").GetComponent<TextFly>();

            float angle = (Random.Range(-45f, 45f) + 75) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
            textFly.transform.position = creditsText.transform.position;
            textFly.transform.SetParent(transform);

            textFly.Initialize("+ " + creditsChange + " C", textColor, textFlyAlphaMax, dir, false);
        }

        int credits = (int)BuildingSystem.PlayerCredits;
        creditsText.text = "Credits: " + credits.ToString("#0");
        previousCredits = credits;
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
            flashEffect = StartCoroutine(GameTools.FlashSpriteOrImage(
                                        null, qCubeImage, qCubeShakeTime, 
                                        qCubeShakeTime/2, qCubeDamageColor, qCubeStartColor));
        }
        else
        {
            // Add cube repair effect
        }

        qCubePevHealth = qCubeCurrHealth;
    }

    

}
