
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GunType
{
    LaserPistol,
    SMG,
    RepairGun = 10,
}

[CreateAssetMenu(menuName = "GunSO", fileName = "NewGun")]
public class GunSO : ScriptableObject
{
    public GunType gunType;
    public string uiName;
    public Sprite sprite;
    public Sprite uiSprite;
    public string animatorHasString;
    public GameObject bulletPrefab;
    public bool isOneHanded;

    [Header("Sounds")]
    public List<SoundSO> reloadSounds;
    public SoundSO selectedSound;

    [Header("Effects")]
    public GameObject emptyClipPrefab;
    public GameObject muzzleFlashPrefab;
    public GameObject laserSight;
    public Vector2 muzzleLocalPos;
    public GameObject bulletCasingPrefab;
    public Vector2 casingLocalPos;
    public float muzzleFlashTime;
    public Vector2 leftHandPos;
    public Vector2 rightHandPos;
    public Vector2 recoilAmount;
    
    [Header("Gun Stats")]
    public float damage;
    public float damageUpgAmt;
    public float knockback;
    public float knockbackUpgAmt;
    public int clipSize;
    public int clipSizeUpgAmt;
    public float chargeDelay;
    public float chargeDelayUpgNegAmt;
    public float shootSpeed;
    public float shootSpeedUpgNegAmt;
    public float accuracy;
    public float accuracyUpgNegAmt;
    public float bulletSpeed;
    public float bulletLifeTime;
    public float bulletRangeUpgAmt;
    public float kickback;
    public float kickbackUpgNegAmt;
}