using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Body")]
public class BodyGeneSO : ScriptableObject      // 🔸 NEW
{
    public string   id   = "Body_Default";      // unique ID
    public Sprite   sprite;
    public float    scale = 1f;
    public Vector2  bodyColliderOffset = new Vector2(0, 0);
    public Vector2  bodyColliderSize = new Vector2(1, 1);

    [Header("Anchor Offsets")]
    public Vector2  headAnchorOffset = new Vector2(0, 0.9f);
    public Vector2  leftLegAnchorOffset  = new Vector2(-0.3f, -0.9f);
    public Vector2  rightLegAnchorOffset = new Vector2( 0.3f, -0.9f);
}