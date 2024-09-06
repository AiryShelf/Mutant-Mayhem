using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDStatsPanel : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] TextMeshProUGUI creditsText;
    [SerializeField] Slider healthSlider;
    [SerializeField] Slider staminaSlider;

    [Header("Qcube Stats")]
    [SerializeField] Image qCubeImage;
    [SerializeField] QCubeHealth qCubeHealthScript;
    [SerializeField] Slider qCubeHealthSlider;

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
    [SerializeField] float qCubeShakeSpeed;
    [SerializeField] float qCubeShakeAmount;
    [SerializeField] float qCubeShakeTime;
    Vector2 qCubeStartPos;
    Color qCubeStartColor;
    Coroutine shakeEffect;

    void Start()
    {
        player = FindObjectOfType<Player>();
        playerStats = player.stats;
        playerHealthScript = player.GetComponent<Health>();
        playerStaminaScript = player.GetComponent<Stamina>();

        qCubeStartPos = qCubeImage.transform.localPosition;
        qCubeStartColor = qCubeImage.color;
    }

    void FixedUpdate()
    {
        UpdatePlayerStatsUI();

        UpdateQCubeStatsUI(); 
    }

    void UpdatePlayerStatsUI()
    {
        playerMaxHealth = playerHealthScript.GetMaxHealth();
        playerMaxStamina = playerStats.staminaMax;
        playerHealth = playerHealthScript.GetHealth();
        playerStamina = playerStaminaScript.GetStamina();

        healthSlider.value = playerHealth / playerMaxHealth;
        staminaSlider.value = playerStamina / playerMaxStamina;

        creditsText.text = "Credits: " + BuildingSystem.PlayerCredits.ToString("#0");
    }

    void UpdateQCubeStatsUI()
    {
        qCubeCurrHealth = qCubeHealthScript.GetHealth();
        qCubeMaxHealth = qCubeHealthScript.GetMaxHealth();

        qCubeHealthSlider.value = qCubeCurrHealth / qCubeMaxHealth;

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
            shakeEffect = StartCoroutine(ShakeEffect());
        }
        qCubePevHealth = qCubeCurrHealth;
    }

    IEnumerator ShakeEffect()
    {
        float timeElapsed = 0;
        Color targetColor = new Color(255, 0, 0, 0.75f);

        while (timeElapsed < qCubeShakeTime)
        {
            qCubeImage.transform.localPosition = qCubeStartPos + 
                                                 Random.insideUnitCircle * qCubeShakeAmount;

            // Calculate the interpolation factor
            float halfTime = qCubeShakeTime / 4;
            float t = timeElapsed < halfTime ? 
                    (timeElapsed / halfTime) : 
                    ((qCubeShakeTime - timeElapsed) / halfTime);

            // Interpolate the color
            qCubeImage.color = Color.Lerp(qCubeStartColor, targetColor, t);

            yield return new WaitForSeconds(qCubeShakeSpeed);
            timeElapsed += qCubeShakeSpeed;
        }

        qCubeImage.transform.localPosition = qCubeStartPos;
        qCubeImage.color = qCubeStartColor;
    }

}
