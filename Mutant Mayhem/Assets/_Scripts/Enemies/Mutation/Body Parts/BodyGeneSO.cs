using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Body")]
public class BodyGeneSO : ScriptableObject      // 🔸 NEW
{
    public string   id   = "Body_Default";      // unique ID
    public Sprite   sprite;

    // 🔸 Anchor offsets relative to body’s pivot
    public Vector2  headAnchorOffset   = new Vector2(0,  0.9f);
    public Vector2  leftLegAnchorOffset  = new Vector2(-0.3f, -0.9f);
    public Vector2  rightLegAnchorOffset = new Vector2( 0.3f, -0.9f);
}