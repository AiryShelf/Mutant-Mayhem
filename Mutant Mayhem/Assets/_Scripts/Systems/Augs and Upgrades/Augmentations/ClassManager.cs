using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerClass
{
    Fighter,
    Neutral,
    Builder
}

public class ClassManager : MonoBehaviour
{
    public static ClassManager Instance { get; private set; }

    public PlayerClass selectedClass = PlayerClass.Neutral;

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


    public void ApplyClassEffects(Player player)
    {
        BuildingSystem buildingSystem = FindObjectOfType<BuildingSystem>();

        switch (selectedClass)
        {
            case PlayerClass.Fighter:
                UpgradeManager.Instance.playerStatsCostMult -= 0.2f;
                UpgradeManager.Instance.gunStatsCostMult -= 0.2f;
                UpgradeManager.Instance.structureStatsCostMult += 0.2f;
                buildingSystem.structureCostMult += 0.2f;
                break;

            case PlayerClass.Neutral:
                break;

            case PlayerClass.Builder:
                UpgradeManager.Instance.playerStatsCostMult += 0.2f;
                UpgradeManager.Instance.gunStatsCostMult += 0.2f;
                UpgradeManager.Instance.structureStatsCostMult -= 0.2f;
                buildingSystem.structureCostMult -= 0.2f;
                player.playerShooter.gunsUnlocked[4] = true; // Repair gun
                player.playerShooter.gunList[4].damage *= 2;
                break;
        }

        Debug.Log("Class manager applied it's effects for " + selectedClass + " class");
    }
}