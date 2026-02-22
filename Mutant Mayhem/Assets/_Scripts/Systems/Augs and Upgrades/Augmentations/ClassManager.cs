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

                TileManager.Instance.blueprintBuildSpeedMultiplier = 0.75f;
                player.playerShooter.gunList[4].damage *= 0.75f;
                DroneManager.Instance.droneGunList[0].damage *= 0.75f;
                break;

            case PlayerClass.Neutral:
                break;

            case PlayerClass.Builder:
                UpgradeManager.Instance.playerStatsCostMult += 0.2f;
                UpgradeManager.Instance.gunStatsCostMult += 0.2f;
                UpgradeManager.Instance.structureStatsCostMult -= 0.2f;
                buildingSystem.structureCostMult -= 0.2f;
                
                TileManager.Instance.blueprintBuildSpeedMultiplier = 1.5f;
                player.playerShooter.gunList[4].damage *= 1.5f;
                DroneManager.Instance.droneGunList[0].damage *= 1.5f; // Drone repair gun
                break;
        }

        Debug.Log("Class manager applied it's effects for " + selectedClass + " class");
    }
}