using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Body")]
public class BodyGeneSO : GeneSOBase
{
    public Sprite sprite;

    [Header("Anchor Offsets")]
    public Vector2 headAnchorOffset = new Vector2(0, 0.9f);
    public Vector2 leftLegAnchorOffset = new Vector2(-0.3f, -0.9f);
    public Vector2 rightLegAnchorOffset = new Vector2(0.3f, -0.9f);

    [Header("Body and Health Settings")]
    public Vector2 bodyColliderOffset = new Vector2(0, 0);
    public Vector2 bodyColliderSize = new Vector2(1, 1);
    public float startMass = 1f;
    public float startHealth = 10f;

    [Header("Shadow Settings")]
    public Vector2[] shadowShapePoints;
}