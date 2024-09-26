using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsCounterPlayer : MonoBehaviour
{
    public static float TotalPlayTime;
    public static float TimeSprintingPlayer;

    public static int EnemiesKilledByPlayer;
    public static int EnemiesKilledByTurrets;

    public static int ShotsFiredByPlayer;
    public static int ShotsHitByPlayer;
    public static int ShotsFiredByTurrets;
    public static int ShotsFiredByEnemies;
    public static int GrenadesThrownByPlayer;

    public static int MeleeAttacksByPlayer;
    public static int MeleeHitsByPlayer;
    public static float MeleeDamageByPlayer;
    public static int MeleeAttacksByEnemies;
    public static float MeleeDamageByEnemies;

    public static float TotalDamageByPlayerExplosions;
    public static float EnemyDamageByPlayerProjectiles;
    public static float DamageToPlayer;
    public static float DamageToStructures;
    public static float DamageToEnemies;

    public static int StructuresBuilt;
    public static int StructuresLost;

    public static Dictionary<string, float> StatsDict =
        new Dictionary<string, float>();

    void FixedUpdate()
    {
        TotalPlayTime += Time.fixedDeltaTime;
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
        TotalDamageByPlayerExplosions = 0;
        EnemyDamageByPlayerProjectiles = 0;
        DamageToPlayer = 0;
        DamageToStructures = 0;
        DamageToEnemies = 0;
        StructuresBuilt = 0;
        StructuresLost = 0;
        
        StatsDict.Clear();

        StatsDict = new Dictionary<string, float>
        {
            {"Sprinting Time (sec):", TimeSprintingPlayer},

            {"Enemies Killed by Player:", EnemiesKilledByPlayer},
            {"Enemies Killed by Turrets:", EnemiesKilledByTurrets},

            {"Shots Fired by Player:", ShotsFiredByPlayer},
            {"Shot Hits by Player:", ShotsHitByPlayer},
            {"Shots Fired by Turrets:", ShotsFiredByTurrets },
            {"Shots Fired by Enemies:", ShotsFiredByEnemies},
            {"Grenades Thrown by Player:", GrenadesThrownByPlayer},

            {"Melee Attacks by Player:", MeleeAttacksByPlayer},
            {"Melee Hits by Player:", MeleeHitsByPlayer},
            {"Melee Damage by Player:", MeleeDamageByPlayer},
            {"Melee Attacks by Enemies:", MeleeAttacksByEnemies},
            {"Melee Damage by Enemies:", MeleeDamageByEnemies},

            {"Total Damage by Explosions:", TotalDamageByPlayerExplosions},
            {"Projectile Damage by Player:", EnemyDamageByPlayerProjectiles},
            {"Total Damage to Player:", DamageToPlayer},
            {"Total Damage to Structures:", DamageToStructures},
            {"Total Damage to Enemies:", DamageToEnemies},

            {"Structures Built:", StructuresBuilt},
            {"Structures Lost:", StructuresLost}
        };
    }

    public string GetStatsString()
    {
        // Maybe make this a dictionary to populate the 2 aligned
        // TMP elements
        string text = 
        "Survival Time: " + TotalPlayTime +
        "\nSprinting Time: " + TimeSprintingPlayer +

        "\n\nEnemies Killed by Player: " + EnemiesKilledByPlayer +
        "\nEnemies Killed by Turrets: " + EnemiesKilledByTurrets +

        "\n\nShots Fired by Player: " + ShotsFiredByPlayer +
        "\nShot Hits by Player: " + ShotsHitByPlayer +
        "\nShots Fired by Turrets: " + ShotsFiredByTurrets +
        "\nShots Fired by Enemies: " + ShotsFiredByEnemies +
        "\nGrenades Thrown by Player: " + GrenadesThrownByPlayer +

        "\n\nMelee Attacks by Player: " + MeleeAttacksByPlayer +
        "\nMelee Hits by Player: " + MeleeHitsByPlayer +
        "\nMelee Damage by Player: " + MeleeDamageByPlayer +
        "\nMelee Attacks by Enemies: " + MeleeDamageByEnemies +

        "\n\nTotal Damage by Explosions: " + TotalDamageByPlayerExplosions +
        "\nProjectile Damage by Player: " + EnemyDamageByPlayerProjectiles +
        "\nTotal Damage to Player: " + DamageToPlayer +
        "\nTotal Damage to Structures: " + DamageToStructures +
        "\nTotal Damage to Enemies: " + DamageToEnemies +

        "\n\nStructures Built: " + StructuresBuilt +
        "\nStructures Lost: " + StructuresLost;
        return text;
    }
}
