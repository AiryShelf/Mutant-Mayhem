using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsCounterPlayer : MonoBehaviour
{
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

    [Header("Structures")]
    public static int StructuresBuilt;
    public static int StructuresLost;
    public static float AmountRepaired;
    public static int DoorsBuilt;
    public static int WallsBuilt;
    public static int TurretsBuilt;

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
        yield return new WaitForSeconds(1);
        TotalPlayTime++;
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
        AmountRepaired = 0;
        DoorsBuilt = 0;
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
        };

        StructuresStats = new Dictionary<string, float>
        {
            {"Total Built:", StructuresBuilt},
            {"Total Lost:", StructuresLost},
            {"Damage Repaired:", AmountRepaired},
            {"Doors Built", DoorsBuilt},
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
}
