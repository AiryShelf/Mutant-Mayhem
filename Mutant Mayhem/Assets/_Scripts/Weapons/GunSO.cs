
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(menuName = "Gun", fileName = "NewGun")]
public class GunSO : ScriptableObject
{
    public GunType gunType;
    public Sprite sprite;
    public Sprite uiSprite;
    public string animatorHasString;
    public GameObject bulletPrefab;

    [Header("Effects")]
    public GameObject emptyClipPrefab;
    public GameObject muzzleFlashPrefab;
    public Vector2 muzzleLocalPos;
    public GameObject bulletCasingPrefab;
    public Vector2 casingLocalPos;
    public float muzzleFlashTime;
    
    [Header("Gun Stats")]
    public int damage;
    public int damageUpgAmt;
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
    public float recoil;
    public float recoilUpgNegAmt;
}

public enum GunType
{
    LaserPistol,
    SMG,
}