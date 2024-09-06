using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    [SerializeField] int maxEnemies = 150;
    public static int EnemyCount;
    public static int MaxEnemies;

    void Start()
    {
        MaxEnemies = maxEnemies;
        EnemyCount = 0;
    }
}
