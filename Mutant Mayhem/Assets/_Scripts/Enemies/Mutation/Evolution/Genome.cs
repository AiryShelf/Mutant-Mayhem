[System.Serializable]
public class Genome
{
    public string bodyId, headId, legId;

    // 🔸 NEW – scale factors (1 = default size)
    public float  bodyScale      = 1f;
    public float  headScale      = 1f;
    public float  legScale       = 1f;
    
    public int numberOfGenes => 3; // body, head, legs

    public Genome(
        string body, string head,
        string leg,
        float bScale = 1, float hScale = 1,
        float lScale = 1)
    {
        bodyId = body; headId = head; legId = leg;
        bodyScale = bScale; headScale = hScale;
        legScale = lScale;
    }
}