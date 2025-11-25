using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyMutant : EnemyBase
{
    public GeneticIndividual individual;

    [Header("Mutation")]
    public MutantRenderer mutantRenderer;
    public Rigidbody2D mutantRb;
    public PolygonCollider2D bodyCollider;
    public BoxCollider2D meleeCollider;
    public float fighterHealthFitnessMultiplier = 0.1f; // Multiplier for fitness if fighter does no damage

    public override void Awake()
    {
        if (WaveController.Instance != null)
        {
            waveController = WaveController.Instance;
        }

        rb = mutantRb;

        startMass = rb.mass;
        startLocalScale = transform.localScale;
        moveSpeedBaseStart = moveSpeedBase;

        InitializeStateMachine();
    }

    public void InitializeMutant(GeneticIndividual ind)
    {
        if (ind == null)
        {
            Debug.LogError("MutatedEnemy initialization failed: Individual is null.");
            return;
        }

        if (waveController == null)
        {
            waveController = WaveController.Instance;
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

    public void AssignIndividual(GeneticIndividual individual)
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

        // Set movement speed and a minimum based on mass
        moveSpeedBase *= rb.mass * waveController.speedMultiplier;
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
        List<Vector2> headOrdered = g.headGene.headColliderPoints.Select(p => p * g.headGene.scale * WaveController.Instance.sizeMultiplier).ToList();
        List<Vector2> bodyOrdered = g.bodyGene.bodyColliderPoints.Select(p => p * g.bodyGene.scale * WaveController.Instance.sizeMultiplier).ToList();

        List<Vector2> combinedPoints = new List<Vector2>();
        combinedPoints.AddRange(headOrdered);
        combinedPoints.AddRange(bodyOrdered);

        bodyCollider.SetPath(0, combinedPoints.ToArray());
        bodyCollider.isTrigger = g.legGene.isFlying;
    }

    void SetHealthSettings(Genome g)
    {
        // Treat this like a “size” and square it to get an area-like factor
        float bodyAreaScale = g.bodyGene.scale * g.bodyGene.scale;

        health.SetMaxHealth(bodyAreaScale * health.startMaxHealth * waveController.healthMultiplier);
        health.SetHealth(health.GetMaxHealth());
        health.painSound = g.bodyGene.painSound;

        // Freeze time still based on body size only
        float baseScaleForFreeze = 9f;
        float sizeRatio = Mathf.Max(g.bodyGene.scale / baseScaleForFreeze, 0.1f);
        unfreezeTime = g.bodyGene.freezeTime / sizeRatio;
    }

    void SetMeleeSettings(Genome g)
    {
        float headAreaScale = g.headGene.scale * g.headGene.scale;
        meleeController.meleeSound = g.headGene.meleeSound;
        meleeCollider.offset = g.headGene.meleeColliderOffset;
        meleeCollider.size = g.headGene.meleeColliderSize;

        meleeController.meleeDamage = g.headGene.meleeDamage * headAreaScale * waveController.damageMultiplier;
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
        if (individual.variant == PopulationVariantType.Fighter)
        {
            fitness = meleeController.playerDamageDealt
                        + meleeController.structureDamageDealt / 2;
            if (fitness <= 0)
                fitness = health.GetMaxHealth() * fighterHealthFitnessMultiplier;
            individual.AddFitness(fitness);
        }
        else
        {
            fitness = health.GetMaxHealth() / 2 + meleeController.structureDamageDealt;
            individual.AddFitness(fitness);
        }

        Debug.Log($"Mutant {individual.variant} died with fitness: {fitness}");
    }
    
    #endregion
}