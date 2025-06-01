using UnityEngine;

public class DefaultGeneticOps
{
    private BodyGeneSO[] bodies;
    private HeadGeneSO[] heads;
    private LegGeneSO[] legs;

    public DefaultGeneticOps()
    {
        GeneDatabase.InitialiseIfNeeded();
        bodies = Resources.LoadAll<BodyGeneSO>("");
        heads = Resources.LoadAll<HeadGeneSO>("");
        legs = Resources.LoadAll<LegGeneSO>("");
    }

    public Genome Crossover(Genome a, Genome b)
    {
        return new Genome(
            Random.value < 0.5f ? a.bodyId : b.bodyId,
            Random.value < 0.5f ? a.headId : b.headId,
            Random.value < 0.5f ? a.leftLegId : b.leftLegId,
            Random.value < 0.5f ? a.rightLegId : b.rightLegId,
            Mathf.Lerp(a.bodyScale, b.bodyScale, Random.value),
            Mathf.Lerp(a.headScale, b.headScale, Random.value),
            Mathf.Lerp(a.leftLegScale, b.leftLegScale, Random.value),
            Mathf.Lerp(a.rightLegScale, b.rightLegScale, Random.value)
        );
    }

    public void Mutate(Genome g, float mutationRate, float difficultyScale)
    {
        // 🔸 mutate part choice
        if (Random.value < mutationRate)
            g.bodyId = RandomChoice(bodies).id;
        if (Random.value < mutationRate)
            g.headId = RandomChoice(heads).id;
        if (Random.value < mutationRate)
            g.leftLegId = RandomChoice(legs).id;
        if (Random.value < mutationRate)
            g.rightLegId = RandomChoice(legs).id;

        // 🔸 mutate scales
        float delta = 0.15f;
        float maxTotal = 4f + difficultyScale;

        if (Random.value < mutationRate) g.bodyScale += Random.Range(-delta, delta);
        if (Random.value < mutationRate) g.headScale += Random.Range(-delta, delta);
        if (Random.value < mutationRate) g.leftLegScale += Random.Range(-delta, delta);
        if (Random.value < mutationRate) g.rightLegScale += Random.Range(-delta, delta);

        ClampAndNormalize(ref g, maxTotal);
    }

    private void ClampAndNormalize(ref Genome g, float maxTotal)
    {
        g.bodyScale     = Mathf.Clamp(g.bodyScale, 0.5f, 1.5f);
        g.headScale     = Mathf.Clamp(g.headScale, 0.5f, 1.5f);
        g.leftLegScale  = Mathf.Clamp(g.leftLegScale, 0.5f, 1.5f);
        g.rightLegScale = Mathf.Clamp(g.rightLegScale, 0.5f, 1.5f);

        float total = g.bodyScale + g.headScale + g.leftLegScale + g.rightLegScale;
        if (total > maxTotal)
        {
            float factor = maxTotal / total;
            g.bodyScale *= factor;
            g.headScale *= factor;
            g.leftLegScale *= factor;
            g.rightLegScale *= factor;
        }
    }

    private T RandomChoice<T>(T[] arr) => arr[Random.Range(0, arr.Length)];
}