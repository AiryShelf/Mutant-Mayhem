using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TurretGunSO", fileName = "NewTurretGun")]
public class TurretGunSO : GunSO
{
    [Header("Turret Stats")]
    public float rotationSpeed;
    public float rotSpeedUpgAmt;
    public float detectRange;
    public float detectRangeUpgAmt;
    public float expansionDelay;
    public float expansionDelayUpgNegAmt;
    public float reloadSpeed;
    public float reloadSpeedUpgNegAmt;
}
