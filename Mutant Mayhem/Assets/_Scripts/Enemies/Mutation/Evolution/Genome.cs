[System.Serializable]
public class Genome
{
    public string bodyId, headId, leftLegId, rightLegId;

    // 🔸 NEW – scale factors (1 = default size)
    public float  bodyScale      = 1f;
    public float  headScale      = 1f;
    public float  leftLegScale   = 1f;
    public float  rightLegScale  = 1f;

    public Genome(
        string body,  string head,
        string left,  string right,
        float  bScale = 1,  float hScale = 1,
        float  lScale = 1,  float rScale = 1)
    {
        bodyId = body; headId = head; leftLegId = left; rightLegId = right;
        bodyScale = bScale; headScale = hScale;
        leftLegScale = lScale; rightLegScale = rScale;
    }
}