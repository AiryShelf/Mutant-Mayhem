using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyMutant : EnemyBase
{
    public MutantIndividual individual;

    [Header("Mutation")]
    public MutantRenderer mutantRenderer;
    public Rigidbody2D mutantRb;
    public PolygonCollider2D bodyCollider;
    public BoxCollider2D meleeCollider;
    public float healthFitnessMultiplier = 0.25f;

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

        LegGeneSO legGene = ind.genome.legGene;
        HeadGeneSO headGene = ind.genome.headGene;
        ApplyBehaviourSet(legGene.idleSOBase, legGene.chaseSOBase, headGene.shootSOBase);
        RestartStateMachine();

        ApplyGenomeToEnemyBase();
        ResetStats();

        ApplyGenomeToParts();
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
        ApplyFitness();

        base.Die();
    }

    public override void RandomizeEnemyBaseColor()
    {
        mutantRenderer.RandomizeColor(randColorRange);
    }

    #region Apply Genome

    void ApplyGenomeToEnemyBase()
    {
        if (individual == null)
        {
            Debug.LogWarning("No individual assigned on ApplyGenomeToEnemyBase.");
            return;
        }

        Genome g = individual.genome;

        moveSpeedBaseStart = g.legGene.moveSpeedBaseStart;
        rotateSpeedBase = g.legGene.rotateSpeedBaseStart;
        startMass = g.bodyGene.startMass;
        health.startMaxHealth = g.bodyGene.startHealth;
    }

    public void ApplyGenomeToParts()
    {
        if (individual == null)
        {
            Debug.LogWarning("No individual assigned on ApplyGenomeToParts.");
            return;
        }

        Genome g = individual.genome;

        SetHealthSettings(g);
        float areaScale = g.bodyGene.scale * g.headGene.scale;
        rb.mass = startMass * areaScale * g.headGene.massModFactor;

        SetCombinedPolygonCollider(g);
        SetMeleeSettings(g);

        moveSpeedBase *= Mathf.Clamp(g.legGene.scale * g.legGene.scale, 1, float.MaxValue);
        //Debug.Log($"EnemyMutant - MoveSpeedBase: {moveSpeedBase}, Mass: {rb.mass}");

       // Debug.Log($"Applied genome scales - Body: {g.bodyGene.scale}, Head: {g.headGene.scale}, Legs: {g.legGene.scale}");
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

    #region Helpers

    void SetCombinedPolygonCollider(Genome g)
    {
        // Combines the head and body collider points
        List<Vector2> headOrdered = g.headGene.headColliderPoints.Select(p => p * g.headGene.scale).ToList();
        List<Vector2> bodyOrdered = g.bodyGene.bodyColliderPoints.Select(p => p * g.bodyGene.scale).ToList();

        List<Vector2> combinedPoints = new List<Vector2>();
        combinedPoints.AddRange(headOrdered);
        combinedPoints.AddRange(bodyOrdered);

        bodyCollider.SetPath(0, combinedPoints.ToArray());
        bodyCollider.isTrigger = g.legGene.isFlying;
    }

    void SetHealthSettings(Genome g)
    {
        float areaScale = g.bodyGene.scale * g.headGene.scale;
        health.SetMaxHealth(g.bodyGene.startHealth * areaScale);
        health.SetHealth(health.GetMaxHealth());
        health.painSound = g.bodyGene.painSound;
        unfreezeTime = g.bodyGene.freezeTime / (g.bodyGene.scale / 6);
    }

    void SetMeleeSettings(Genome g)
    {
        float areaScale = g.bodyGene.scale * g.headGene.scale;
        meleeController.meleeSound = g.headGene.meleeSound;
        meleeCollider.offset = g.headGene.meleeColliderOffset;
        meleeCollider.size = g.headGene.meleeColliderSize;

        meleeController.meleeDamage = g.headGene.meleeDamage * areaScale;
        meleeController.attackDelay = g.headGene.attackDelay * waveController.attackDelayMult;
        meleeController.knockback = g.headGene.knockback * rb.mass;
        meleeController.selfKnockback = g.headGene.selfKnockback * rb.mass;
    }

    void ApplyFitness()
    {
        // Apply fitness based on variant
        if (individual == null)
        {
            Debug.LogError("Individual is null on Die.");
            return;
        }
        
        float fitness;
        if (individual.variant == MutantVariant.Fighter)
        {
            fitness = meleeController.playerDamageDealt
                        + meleeController.structureDamageDealt / 2;
            if (fitness <= 0)
                fitness = health.GetMaxHealth() * healthFitnessMultiplier;
            individual.AddFitness(fitness);
        }
        else
        {
            fitness = health.GetMaxHealth() + meleeController.structureDamageDealt;
            individual.AddFitness(fitness);
        }

        //Debug.Log($"Mutant {individual.variant} died with fitness: {fitness}");
    }
    
    #endregion
}