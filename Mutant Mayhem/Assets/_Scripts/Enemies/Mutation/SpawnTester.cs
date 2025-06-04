using UnityEngine;

public class SpawnTester : MonoBehaviour
{
    public EnemyRenderer enemyPrefab;

    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            var e = Instantiate(enemyPrefab, Random.insideUnitCircle * 3, Quaternion.identity);
            var g = new Genome(
                "Body_Dog",                 // temp until you make more bodies
                "Head_Dog",
                "Leg_Bat");

            e.ApplyGenome(g);
        }
    }
}