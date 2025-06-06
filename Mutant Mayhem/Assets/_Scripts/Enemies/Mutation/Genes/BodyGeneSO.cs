using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Body")]
public class BodyGeneSO : ScriptableObject
{
    public string   id   = "Body_Default";
    public Sprite   sprite;
    public Color    color = Color.white;
    public float    scale = 1f;
    public Vector2  bodyColliderOffset = new Vector2(0, 0);
    public Vector2  bodyColliderSize = new Vector2(1, 1);

    [Header("Anchor Offsets")]
    public Vector2  headAnchorOffset = new Vector2(0, 0.9f);
    public Vector2  leftLegAnchorOffset  = new Vector2(-0.3f, -0.9f);
    public Vector2  rightLegAnchorOffset = new Vector2( 0.3f, -0.9f);
}