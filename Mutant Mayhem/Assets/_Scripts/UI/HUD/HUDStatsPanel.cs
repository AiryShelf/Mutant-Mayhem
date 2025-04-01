using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements.Experimental;

public class HUDStatsPanel : MonoBehaviour
{
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

    [Header("Power ")]
    [SerializeField] Image powerImage;
    [SerializeField] TextMeshProUGUI powerText;

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
        player.stats.playerHealthScript.OnPlayerHealthChanged += UpdateHealthStats;
        player.stats.playerHealthScript.OnPlayerMaxHealthChanged += UpdateHealthStats;
        PowerManager.Instance.OnPowerChanged += UpdatePowerText;
    }

    void OnDisable()
    {
        BuildingSystem.OnPlayerCreditsChanged -= UpdateCreditsText;
        qCubeHealthScript.OnCubeHealthChanged -= UpdateQCubeStatsUI;
        player.stats.playerHealthScript.OnPlayerHealthChanged -= UpdateHealthStats;
        player.stats.playerHealthScript.OnPlayerMaxHealthChanged -= UpdateHealthStats;
        PowerManager.Instance.OnPowerChanged -= UpdatePowerText;
    }

    void Start()
    {
        StartCoroutine(DelayStatsUpdate());
    }

    void FixedUpdate()
    {
        UpdateStaminaStats();
    }

    void UpdatePowerText(int powerBalance)
    {
        if (powerBalance >= 0)
            powerImage.color = Color.white;
        else 
            powerImage.color = Color.red;

        powerText.text = powerBalance.ToString();
    }

    IEnumerator DelayStatsUpdate()
    {
        yield return new WaitForEndOfFrame();

        UpdateHealthStats(player.stats.playerHealthScript.GetHealth());
        UpdateCreditsText(BuildingSystem.PlayerCredits);

        qCubeHealth = FindObjectOfType<QCubeHealth>();
        UpdateQCubeStatsUI(qCubeHealth.GetHealth());

        Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreen);

        /*
        var rt = creditsText.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        var parentRT = creditsText.transform.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
        var grandparentRT = creditsText.transform.parent.parent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(grandparentRT);

        yield return new WaitForEndOfFrame(); 

        

        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame(); 
        creditsText.ForceMeshUpdate(true, true);
        yield return new WaitForEndOfFrame(); 
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame(); 

        creditsText.gameObject.SetActive(false);
        yield return new WaitForEndOfFrame(); 
        creditsText.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame(); 
        
        Screen.SetResolution(Screen.width / 2, Screen.height / 2, Screen.fullScreen);
        yield return new WaitForFixedUpdate(); 
        yield return new WaitForFixedUpdate(); 
        yield return new WaitForFixedUpdate(); 
        yield return new WaitForFixedUpdate(); 
        yield return new WaitForFixedUpdate(); 
        yield return new WaitForFixedUpdate();
        Screen.SetResolution(Screen.width * 2, Screen.height * 2, Screen.fullScreen);
        */
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

        // TextFly setup
        TextFly textFly = PoolManager.Instance.GetFromPool("TextFlyUI_Health").GetComponent<TextFly>();
        textFly.transform.SetParent(transform);
        Color textColor;
        Vector3 newPos = healthValueText.transform.position;
        //RectTransform rectTrans = (RectTransform)transform;
        newPos.x += 2;
        textFly.transform.position = newPos;

        if (healthChange < 0)
        {
            textColor = textFlyHealthLossColor;
            float angle = Random.Range(-45f, -15f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
            textFly.Initialize(healthChange + " HP", textColor, textFlyAlphaMax, dir, false, textPulseScaleMax);
        }
        else
        {
            textColor = textFlyHealthGainColor;
            float angle = Random.Range(15, 45f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
            textFly.Initialize("+" + healthChange + " HP", textColor, textFlyAlphaMax, dir, false, textPulseScaleMax);
        }
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
