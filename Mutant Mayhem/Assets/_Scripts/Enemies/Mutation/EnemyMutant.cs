using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMutant : EnemyBase
{
    [Header("Mutation")]
    public EnemyIndividual individual;
    public EnemyRenderer enemyRenderer;

    public void InitializeMutant(EnemyIndividual ind)
    {
        if (ind == null)
        {
            Debug.LogError("MutatedEnemy initialization failed: Individual is null.");
            return;
        }

        AssignIndividual(ind);
        ApplyGenomeToPartStats();
        ApplyGenomeToEnemyRenderer();

        //InitializeStateMachine();
        ApplyGenomeToStateMachine();
        RestartStateMachine();
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
            float fitness = health.GetMaxHealth() - health.GetHealth();
            fitness += meleeController.damageDealt;


            EvolutionManager.Instance.AddFitness(individual, fitness);
        }
        base.Die();
    }
    public void ApplyGenomeToPartStats()
    {
        if (individual == null)
        {
            Debug.LogWarning("No individual assigned on ApplyGenomeToParts.");
            return;
        }

        Genome g = individual.genome;

        // Apply scales to health, damage, and movement speed
        health.SetMaxHealth(health.GetMaxHealth() * g.bodyScale);
        meleeController.meleeDamage *= g.headScale;
        moveSpeedBase *= Mathf.Clamp(g.legScale / 6, 0.3f, 3);

        Debug.Log($"Applied genome scales - Body: {g.bodyScale}, Head: {g.headScale}, Legs: {g.legScale}");
    }

    void ApplyGenomeToEnemyRenderer()
    {
        if (enemyRenderer == null)
        {
            Debug.LogWarning("No enemy renderer assigned to ApplyGenomeToEnemyRenderer.");
            return;
        }

        enemyRenderer.ApplyGenome(individual.genome);
    }

    void ApplyGenomeToStateMachine()
    {
        EnemyIdleSOBaseInstance = Instantiate(individual.genome.idleSOBase);
        EnemyChaseSOBaseInstance = Instantiate(individual.genome.chaseSOBase);

        if (EnemyIdleSOBaseInstance == null || EnemyChaseSOBaseInstance == null)
        {
            Debug.LogError("EnemyIdleSOBase or EnemyChaseSOBase is null, failed to apply to state machine.");
            return;
        }
    }
}
