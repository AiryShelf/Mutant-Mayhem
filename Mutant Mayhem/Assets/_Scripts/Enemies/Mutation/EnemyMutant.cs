using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMutant : EnemyBase
{
    [Header("Mutation")]
    public MutantIndividual individual;
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

    public void InitializeMutant(MutantIndividual ind)
    {
        if (ind == null)
        {
            Debug.LogError("MutatedEnemy initialization failed: Individual is null.");
            return;
        }

        AssignIndividual(ind);
        ind.fitness = 0f;

        LegGeneSO legGene = ind.genome.legGene;
        ApplyBehaviourSet(legGene.idleSOBase, legGene.chaseSOBase, legGene.shootSOBase);
        RestartStateMachine();
        ResetStats();
        
        ApplyGenomeToPartStats();
        ApplyGenomeToEnemyRenderer();
    }

    public void AssignIndividual(MutantIndividual individual)
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
            
            float healthFitness = health.GetMaxHealth() - health.GetHealth();
            healthFitness *= 0.5f;
            Debug.Log("Fitness from health: " + healthFitness);
            float damageFitness = meleeController.damageDealt;
            Debug.Log("Fitness from damage dealt: " + damageFitness);
            float fitness = healthFitness + damageFitness;
            Debug.Log($"Mutant {individual.variant} died with fitness: {fitness}");


            individual.AddFitness(fitness);
        }
        base.Die();
    }

    public override void RandomizeColor()
    {
        mutantRenderer.RandomizeColor(randColorRange);
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
        bodyCollider.offset = g.bodyGene.bodyColliderOffset;
        bodyCollider.size = g.bodyGene.bodyColliderSize * g.bodyGene.scale;

        meleeController.meleeDamage *= g.headGene.scale;

        moveSpeedBase *= Mathf.Clamp(g.legGene.scale / 2, 0.3f, 3);

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