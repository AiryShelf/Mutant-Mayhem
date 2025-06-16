using UnityEngine;

[System.Serializable]
public class Genome
{
    [Header("Genes")]
    public BodyGeneSO bodyGene;
    public HeadGeneSO headGene;
    public LegGeneSO legGene;

    public int numberOfGenes => 3;
    float minColorBrightness = 1f;

    public Genome(BodyGeneSO body, HeadGeneSO head, LegGeneSO leg)
    {
        bodyGene = body;
        headGene = head;
        legGene = leg;
    }
    
    public void RandomizePartColor(GeneSOBase gene, Color color, float randColorRange)
    {
        float red = color.r + Random.Range(-randColorRange, randColorRange);
        float green = color.g + Random.Range(-randColorRange, randColorRange);
        float blue = color.b + Random.Range(-randColorRange, randColorRange);

        // Clamp color values to stay within range
        red = Mathf.Clamp01(red);
        green = Mathf.Clamp01(green);
        blue = Mathf.Clamp01(blue);

        // prevent division by zero, ensure color is bright enough
        if (red + green + blue == 0f)
        {
            red = minColorBrightness / 3;
            green = minColorBrightness / 3;
            blue = minColorBrightness / 3;
        }
        else
        if (red + green + blue < minColorBrightness)
        {
            var factor = minColorBrightness / (red + green + blue);
            red *= factor;
            green *= factor;
            blue *= factor;
        }

        gene.color = new Color(red, green, blue, color.a);
    }
}