using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TurretGunSO", fileName = "NewTurretGun")]
public class TurretGunSO : GunSO
{
    [Header("Turret Stats")]
    public float rotationSpeed;
    public float detectionRange;
    public float reloadTime;
}
