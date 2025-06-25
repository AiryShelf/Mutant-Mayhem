using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsCounterPlayer : MonoBehaviour
{
    [Header("Stat Counts:")]
    [Header("Misc")]
    public static float TotalPlayTime;
    public static int EnemiesKilledByPlayer;
    public static int EnemiesKilledByTurrets;
    public static float TimeSprintingPlayer;

    [Header("Projectiles")]
    public static int ShotsFiredByPlayer;
    public static int ShotsHitByPlayer;
    public static int ShotsFiredByTurrets;
    public static int ShotsFiredByEnemies;
    public static int GrenadesThrownByPlayer;

    [Header("Melee")]
    public static int MeleeAttacksByPlayer;
    public static int MeleeHitsByPlayer;
    public static float MeleeDamageByPlayer;
    public static int MeleeAttacksByEnemies;
    public static float MeleeDamageByEnemies;

    [Header("Damage")]
    public static float EnemyDamageByPlayerExplosions;
    public static float EnemyDamageByPlayerProjectiles;
    public static float EnemyDamageByTurrets;
    public static float DamageToPlayer;
    public static float DamageToStructures;
    public static float DamageToEnemies;
    public static float AmountRepairedByPlayer;
    public static float AmountRepairedByDrones;

    [Header("Structures")]
    public static int StructuresBuilt;
    public static int StructuresLost;
    public static int GatesBuilt;
    public static int WallsBuilt;
    public static int SolarPanelsBuilt;
    public static int TurretsBuilt;
    public static int EngineeringBaysBuilt;
    public static int PhotonicsBayBuilt;
    public static int BallisticsBayBuilt;
    public static int ExplosivesBayBuilt;
    public static int RepairBayBuilt;
    public static int DroneBayBuilt;

    public static int StructuresPlaced;
    public static int WallsPlaced;
    public static int GatesPlaced;
    public static int TurretsPlaced;
    public static int SolarPanelsPlaced;
    public static int EngineeringBaysPlaced;
    public static int PhotonicsBayPlaced;
    public static int BallisticsBayPlaced;
    public static int ExplosivesBayPlaced;
    public static int RepairBayPlaced;
    public static int DroneBayPlaced;

    static Dictionary<string, float> MiscStats = new Dictionary<string, float>();
    static Dictionary<string, float> ProjectilesStats = new Dictionary<string, float>();
    static Dictionary<string, float> MeleeStats = new Dictionary<string, float>();
    static Dictionary<string, float> DamageStats = new Dictionary<string, float>();
    static Dictionary<string, float> StructuresStats = new Dictionary<string, float>();

    void Start()
    {

        StartCoroutine(TimeCounter());
    }

    IEnumerator TimeCounter()
    {
        while (true)
        {
            if (!TimeControl.isPaused)
                TotalPlayTime++;

            yield return new WaitForSecondsRealtime(1);
        }
    }

    public static void ResetStatsCounts()
    {
        TotalPlayTime = 0;
        TimeSprintingPlayer = 0;
        EnemiesKilledByPlayer = 0;
        EnemiesKilledByTurrets = 0;
        ShotsFiredByPlayer = 0;
        ShotsHitByPlayer = 0;
        ShotsFiredByTurrets = 0;
        ShotsFiredByEnemies = 0;
        GrenadesThrownByPlayer = 0;
        MeleeAttacksByPlayer = 0;
        MeleeHitsByPlayer = 0;
        MeleeDamageByPlayer = 0;
        MeleeAttacksByEnemies = 0;
        MeleeDamageByEnemies = 0;
        EnemyDamageByPlayerExplosions = 0;
        EnemyDamageByPlayerProjectiles = 0;
        EnemyDamageByTurrets = 0;
        DamageToPlayer = 0;
        DamageToStructures = 0;
        DamageToEnemies = 0;
        StructuresBuilt = 0;
        StructuresLost = 0;
        AmountRepairedByPlayer = 0;
        GatesBuilt = 0;
        WallsBuilt = 0;
        TurretsBuilt = 0;

        PopulateStatsDict();
    }

    public static void PopulateStatsDict()
    {
        MiscStats.Clear();
        ProjectilesStats.Clear();
        MeleeStats.Clear();
        DamageStats.Clear();
        StructuresStats.Clear();

        MiscStats = new Dictionary<string, float>
        {
            {"Player Kills:", EnemiesKilledByPlayer},
            {"Turret Kills:", EnemiesKilledByTurrets},
            {"Seconds Sprinting:", TimeSprintingPlayer},
            {"Grenades Thrown:", GrenadesThrownByPlayer},
        };

        ProjectilesStats = new Dictionary<string, float>
        {
            {"Shots by Player:", ShotsFiredByPlayer},
            {"Hits by Player:", ShotsHitByPlayer},
            {"Damage by Player:", EnemyDamageByPlayerProjectiles},
            {"Shots by Turrets:", ShotsFiredByTurrets },
            {"Shots by Enemies:", ShotsFiredByEnemies},
            // Player Accuracy
        };

        MeleeStats = new Dictionary<string, float>
        {
            {"Attacks by Player:", MeleeAttacksByPlayer},
            {"Hits by Player:", MeleeHitsByPlayer},
            {"Damage by Player:", MeleeDamageByPlayer},
            {"Attacks by Enemies:", MeleeAttacksByEnemies},
            {"Damage by Enemies:", MeleeDamageByEnemies},
        };

        DamageStats = new Dictionary<string, float>
        {
            {"By Player Explosions:", EnemyDamageByPlayerExplosions},
            {"Total to Player:", DamageToPlayer},
            {"Total to Structures:", DamageToStructures},
            {"Total to Enemies:", DamageToEnemies},
            {"Repaired by Player:", AmountRepairedByPlayer},
            {"Repaired by Drones:", AmountRepairedByDrones},
        };

        StructuresStats = new Dictionary<string, float>
        {
            {"Total Built:", StructuresBuilt},
            {"Total Lost:", StructuresLost},
            {"Gates Built", GatesBuilt},
            {"Walls Built", WallsBuilt},
            {"Turrets Built", TurretsBuilt},
        };
    }

    public static Dictionary<string, float> GetMiscStats()
    {
        return MiscStats;
    }
    public static Dictionary<string, float> GetProjectilesStats()
    {
        return ProjectilesStats;
    }

    public static Dictionary<string, float> GetMeleeStats()
    {
        return MeleeStats;
    }

    public static Dictionary<string, float> GetDamageStats()
    {
        return DamageStats;
    }

    public static Dictionary<string, float> GetStructuresStats()
    {
        return StructuresStats;
    }

    // Add this method:
    public static int GetStructuresBuiltByType(StructureType type)
    {
        switch (type)
        {
            case StructureType.Gate:
                return GatesBuilt;
            case StructureType.OneByOneWall:
                return WallsBuilt;
            case StructureType.OneByOneCorner:
                return WallsBuilt;
            case StructureType.GunTurret:
                return TurretsBuilt;
            case StructureType.LaserTurret:
                return TurretsBuilt;
            case StructureType.SolarPanels:
                return SolarPanelsBuilt;
            case StructureType.EngineeringBay:
                return EngineeringBaysBuilt;
            case StructureType.PhotonicsBay:
                return PhotonicsBayBuilt;
            case StructureType.BallisticsBay:
                return BallisticsBayBuilt;
            case StructureType.ExplosivesBay:
                return ExplosivesBayBuilt;
            case StructureType.RepairBay:
                return RepairBayBuilt;
            case StructureType.DroneBay:
                return DroneBayBuilt;
            default:
                Debug.LogError("StatsCounterPlayer: Untracked structure type for stats: " + type +
                                 ". Returning 0.  You may need to add this type to the switch statement.");
                return 0;
        }
    }
    
    public static int GetStructuresPlacedByType(StructureType type)
    {
        switch (type)
        {
            case StructureType.Gate:
                return GatesPlaced;
            case StructureType.OneByOneWall:
                return WallsPlaced;
            case StructureType.OneByOneCorner:
                return WallsPlaced;
            case StructureType.GunTurret:
                return TurretsPlaced;
            case StructureType.LaserTurret:
                return TurretsPlaced;
            case StructureType.SolarPanels:
                return SolarPanelsPlaced;
            case StructureType.EngineeringBay:
                return EngineeringBaysPlaced;
            case StructureType.PhotonicsBay:
                return PhotonicsBayPlaced;
            case StructureType.BallisticsBay:
                return BallisticsBayPlaced;
            case StructureType.ExplosivesBay:
                return ExplosivesBayPlaced;
            case StructureType.RepairBay:
                return RepairBayPlaced;
            case StructureType.DroneBay:
                return DroneBayPlaced;
            default:
                Debug.LogError("StatsCounterPlayer: Untracked structure type for stats: " + type +
                                 ". Returning 0.  You may need to add this type to the switch statement.");
                return 0;
        }
    }
}
