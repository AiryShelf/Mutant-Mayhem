using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    public static int EnemyCount;
    public static int MaxEnemies = 100;

    void Start()
    {
        EnemyCount = 0;
    }
}
