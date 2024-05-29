
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(menuName = "Gun", fileName = "NewGun")]
public class GunSO : ScriptableObject
{
    public Sprite sprite;
    public Sprite uiSprite;
    public string animatorHasString;
    public GameObject bulletPrefab;
    public GameObject emptyClipPrefab;
    public int clipSize;
    public GameObject muzzleFlashPrefab;
    public Vector2 muzzleLocalPos;
    public GameObject bulletCasingPrefab;
    public Vector2 casingLocalPos;
    public float muzzleFlashTime;
    public float shootSpeed;
    public float accuracy;
    public float bulletSpeed;
    public float bulletLifeTime;
    public float kickback;
}
