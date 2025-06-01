using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutatedEnemy : EnemyBase
{
    [Header("Mutation")]
    public EnemyIndividual individual;

    public override void Awake()
    {
        base.Awake();
    }

    public void AssignIndividual(EnemyIndividual individual)
    {
        if (individual == null)
        {
            Debug.LogError("EnemyIndividual is null, cannot assign to EnemyBase.");
            return;
        }
        this.individual = individual;
    }

    public override void Die()
    {
        if (individual != null)
        {
            EvolutionManager.Instance.AddFitness(individual, health.GetMaxHealth() - health.GetHealth());
        }
        base.Die();
    }
}
