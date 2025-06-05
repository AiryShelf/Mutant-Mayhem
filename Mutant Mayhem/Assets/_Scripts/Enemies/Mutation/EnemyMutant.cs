using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMutant : EnemyBase
{
    [Header("Mutation")]
    public EnemyIndividual individual;
    public MutantRenderer mutantRenderer;
    public Rigidbody2D mutantRb;
    public CapsuleCollider2D bodyCollider;

    public override void Awake()
    {
        waveController = FindObjectOfType<WaveControllerRandom>();

        rb = mutantRb;

        startMass = rb.mass;
        startLocalScale = transform.localScale;
        moveSpeedBaseStart = moveSpeedBase;

        InitializeStateMachine();
    }

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

        ApplyBehaviourSet(ind.genome.idleSOBase, ind.genome.chaseSOBase, ind.genome.shootSOBase);
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

    #region Apply Genome

    public void ApplyGenomeToPartStats()
    {
        if (individual == null)
        {
            Debug.LogWarning("No individual assigned on ApplyGenomeToParts.");
            return;
        }

        Genome g = individual.genome;

        // Apply scales
        health.SetMaxHealth(health.GetMaxHealth() * g.bodyGene.scale);
        rb.mass = startMass * g.bodyGene.scale;
        bodyCollider.offset = g.bodyColliderOffset;
        bodyCollider.size = g.bodyColliderSize * g.bodyGene.scale;

        meleeController.meleeDamage *= g.headScale;

        moveSpeedBase *= Mathf.Clamp(g.legScale / 2, 0.3f, 3);

        Debug.Log($"Applied genome scales - Body: {g.bodyGene.scale}, Head: {g.headGene.scale}, Legs: {g.legGene.scale}");
    }

    void ApplyGenomeToEnemyRenderer()
    {
        if (mutantRenderer == null)
        {
            Debug.LogWarning("No enemy renderer assigned to ApplyGenomeToEnemyRenderer.");
            return;
        }

        mutantRenderer.ApplyGenome(individual.genome);
    }

    #endregion
}