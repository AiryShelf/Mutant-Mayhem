using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        ApplyBehaviourSet(legGene.idleSOBase, legGene.chaseSOBase, legGene.shootSOBase);
        RestartStateMachine();

        ApplyGenomeToEnemyBase();
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
            float fitness = 0f;
            if (individual.variant == MutantVariant.Fighter)
            {
                fitness = meleeController.damageDealt;
                individual.AddFitness(fitness);
            }
            else
            {
                fitness = health.GetMaxHealth();
                individual.AddFitness(fitness);
            }

            Debug.Log($"Mutant {individual.variant} died with fitness: {fitness}");
        }
        base.Die();
    }

    public override void RandomizeColor()
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
        rotateSpeedBaseStart = g.legGene.rotateSpeedBaseStart;
        startMass = g.bodyGene.startMass;
        health.startMaxHealth = g.bodyGene.startHealth;
    }

    public void ApplyGenomeToPartStats()
    {
        if (individual == null)
        {
            Debug.LogWarning("No individual assigned on ApplyGenomeToParts.");
            return;
        }

        Genome g = individual.genome;

        health.SetMaxHealth(health.GetMaxHealth() * g.bodyGene.scale);
        health.SetHealth(health.GetMaxHealth());
        rb.mass = startMass * g.bodyGene.scale * g.headGene.massModFactor;

        SetCombinedPolygonCollider(g);
        SetMeleeSettings(g);

        moveSpeedBase *= Mathf.Clamp(g.legGene.scale, 1, float.MaxValue);
        Debug.Log($"MoveSpeedBase: {moveSpeedBase}, Mass: {rb.mass}");

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

    #region Helpers

    void SetCombinedPolygonCollider(Genome g)
    {
        List<Vector2> headOrdered = g.headGene.headColliderPoints.Select(p => p * g.headGene.scale).ToList();
    List<Vector2> bodyOrdered = g.bodyGene.bodyColliderPoints.Select(p => p * g.bodyGene.scale).ToList();

    List<Vector2> combinedPoints = new List<Vector2>();
    combinedPoints.AddRange(headOrdered);
    combinedPoints.AddRange(bodyOrdered);

    bodyCollider.SetPath(0, combinedPoints.ToArray());
    }

    void SetMeleeSettings(Genome g)
    {
        meleeCollider.offset = g.headGene.meleeColliderOffset;
        meleeCollider.size = g.headGene.meleeColliderSize;

        meleeController.meleeDamage = g.headGene.meleeDamage * g.headGene.scale;
        meleeController.attackDelay = g.headGene.attackDelay;
        meleeController.knockback = g.headGene.knockback * g.headGene.scale;
        meleeController.selfKnockback = g.headGene.selfKnockback * rb.mass;
    }
    
    #endregion
}