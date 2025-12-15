
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GunType
{
    Laser,
    Bullet,
    RepairGun = 10,
    SpinningBlade
}

[CreateAssetMenu(menuName = "Game/Guns/GunSO", fileName = "NewGun")]
public class GunSO : ScriptableObject
{
    public GunType gunType;
    public bool isAutomatic;
    public string bulletPoolName;
    public string uiName;
    [Multiline()]
    public string uiDescription;

    [Header("For Player")]
    public Sprite sprite;
    public Sprite uiSprite;
    public string animatorHasString;
    

    [Header("Sounds")]
    public List<SoundSO> reloadSounds;
    public SoundSO selectedSound;

    [Header("Effects")]
    public string emptyClipPoolName;
    public GameObject muzzleFlashPrefab;
    public GameObject laserSight;
    public Vector2 muzzleLocalPos;
    public string FXManagerCasingMethodName;
    public Vector2 casingLocalPos;
    public float muzzleFlashTime;
    public Vector2 recoilAmount;
    
    [Header("Gun Stats")]
    public float damage;
    [Range(0, 1)]
    public float damageVariance = 0.15f;
    public float damageUpgFactor;
    public float knockback;
    public float knockbackUpgAmt;
    public int clipSize;
    public int clipSizeUpgAmt;
    public float chargeDelay;
    public float chargeSpeedUpgNegAmt;
    public float shootSpeed;
    public float shootSpeedUpgNegAmt;
    public float accuracy;
    public float accuracyUpgNegAmt;
    public float firingAccuracyLoss = 2;
    public float bulletSpeed;
    public float bulletLifeTime;
    public float bulletRangeUpgAmt;
    public float kickback;
    public float kickbackUpgNegAmt;
}