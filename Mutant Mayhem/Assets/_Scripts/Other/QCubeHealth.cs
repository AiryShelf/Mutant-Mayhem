using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class QCubeHealth : Health
{
    [SerializeField] SpriteRenderer mySr;
    public List<Sprite> damageSprites;

    [Header("Cube Damage Effects")]
    [SerializeField] Light2D cubeLight;
    [SerializeField] float cubeLightMinIntensity;
    [SerializeField] float cubeLightFlashIntensity;
    [SerializeField] Color cubeDamageColor;
    [SerializeField] Color cubeDamageLightColor;
    [SerializeField] float cubeDamageShakeAmount;
    [SerializeField] float cubeDamageShakeTime;
    [SerializeField] float cubeDamageShakeSpeed;

    Coroutine shakeEffect;
    Coroutine flashCubeLightEffect;
    Coroutine flashCubeEffect;
    public event Action<float> OnCubeHealthChanged;

    float lightStartIntensity;
    Color lightStartColor;
    Color cubeStartColor;
    Vector2 cubeStartLocalPos;

    void Start()
    {
        lightStartColor = cubeLight.color;
        cubeStartColor = mySr.color;
        cubeStartLocalPos = transform.localPosition;
        lightStartIntensity = cubeLight.intensity;
    }

    void FixedUpdate()
    {
        UpdateDamageSprite();        
    }

    public override void SetMaxHealth(float value)
    {
        maxHealth = value;
        OnCubeHealthChanged.Invoke(health);
    }

    public override void ModifyHealth(float amount, float textPulseScaleMax, Vector2 textDir, GameObject damageDealer)
    {
        base.ModifyHealth(amount, textPulseScaleMax, textDir, damageDealer);
        
        UpdateDamageSprite();
        UpdateCubeLight(0);
        if (amount < 0)
        {
            PlayCubeDamageEffect();
            StatsCounterPlayer.DamageToCube -= healthChange;
        }
        else
        {
            // Play cube repair effect, currently handled by UpgradeManager
        }

        if (health <= 0 && !hasDied)
        {
            AnalyticsManager.Instance.TrackCubeDestroyed(damageDealer.name);
            Die();
            return;
        }

        OnCubeHealthChanged.Invoke(health);
    }

    void UpdateDamageSprite()
    {
        float healthRatio = 1 - (health / maxHealth);
        int sprIndex = Mathf.FloorToInt(healthRatio * damageSprites.Count);
        sprIndex = Mathf.Clamp(sprIndex, 0, damageSprites.Count - 1);

        mySr.sprite = damageSprites[sprIndex];
    }

    void UpdateCubeLight(float addValue)
    {
        cubeLight.intensity = Mathf.Clamp(lightStartIntensity * (health / maxHealth) + addValue,
                                          cubeLightMinIntensity, float.MaxValue);
    }

    void PlayCubeDamageEffect()
    {
        // Flash Cube
        if (flashCubeEffect != null)
            StopCoroutine(flashCubeEffect);
        flashCubeEffect = StartCoroutine(GameTools.FlashSprite(
                                mySr, cubeDamageShakeTime, 
                                cubeDamageShakeTime/2, cubeDamageColor, cubeStartColor));

        // Flash Cube light
        if (flashCubeLightEffect != null)
            StopCoroutine(flashCubeLightEffect);
        flashCubeLightEffect = StartCoroutine(FlashCubeLight(cubeDamageLightColor));

        // Shake effect
        if (shakeEffect != null)
            StopCoroutine(shakeEffect);
        shakeEffect = StartCoroutine(GameTools.ShakeTransform(
                                transform, cubeStartLocalPos, cubeDamageShakeTime, 
                                cubeDamageShakeAmount, cubeDamageShakeSpeed));
        
        //if (flashRoutine != null)
        //    StopCoroutine(flashRoutine);
        //flashRoutine = StartCoroutine(FlashColors(cubeDamageColor));
    }

    IEnumerator FlashCubeLight(Color color)
    {
        cubeLight.color = color;
        UpdateCubeLight(cubeLightFlashIntensity);

        yield return new WaitForSeconds(cubeDamageShakeTime/2);

        cubeLight.color = lightStartColor;
        UpdateCubeLight(0);
    }

    public override void Die()
    {
        QCubeController.IsCubeDestroyed = true;
        hasDied = true;
    } 
}