using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Planet_New", menuName = "Game/Planet")]
public class Planet : ScriptableObject
{
    public string planetName;
    public string description;
    public Sprite planetIcon;

    [Header("Planet Effects")]
    public float playerMoveSpeedMultiplier = 1f;
    public float buildCostMultiplier = 1f;
    public float laserDamageMultiplier = 1f;
    public float bulletDamageMultiplier = 1f;

    [Header("Other Properties")]
    public bool isSwampy = false;
    public bool hasElectromagneticField = false;
}
