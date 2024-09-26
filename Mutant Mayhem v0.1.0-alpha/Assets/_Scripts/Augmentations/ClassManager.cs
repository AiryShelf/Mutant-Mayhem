using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassManager : MonoBehaviour
{
    public static ClassManager Instance { get; private set; }

    public PlayerClass selectedClass = PlayerClass.Neutral;

    public void ApplyClassEffects(Player player)
    {
        BuildingSystem buildingSystem = FindObjectOfType<BuildingSystem>();

        switch (selectedClass)
        {
            case PlayerClass.Fighter:
            UpgradeManager.Instance.playerStatsCostMult = 0.8f;
            UpgradeManager.Instance.gunStatsCostMult = 0.8f;
            UpgradeManager.Instance.structureStatsCostMult = 1.2f;
            buildingSystem.structureCostMult = 1.2f;
            break;

            case PlayerClass.Neutral:
            UpgradeManager.Instance.playerStatsCostMult = 1;
            UpgradeManager.Instance.gunStatsCostMult = 1;
            UpgradeManager.Instance.structureStatsCostMult = 1;
            buildingSystem.structureCostMult = 1;
            break;

            case PlayerClass.Builder:
            UpgradeManager.Instance.playerStatsCostMult = 1.2f;
            UpgradeManager.Instance.gunStatsCostMult = 1.2f;
            UpgradeManager.Instance.structureStatsCostMult = 0.8f;
            buildingSystem.structureCostMult = 0.8f;
            player.stats.structureStats.maxTurrets = 1;
            player.playerShooter.gunList[9].damage *= 2; // Repair gun
            break;
        }

        Debug.Log("Class manager applied it's effects for " + selectedClass + " class");
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
}

public enum PlayerClass
{
    Fighter,
    Neutral,
    Builder
}
