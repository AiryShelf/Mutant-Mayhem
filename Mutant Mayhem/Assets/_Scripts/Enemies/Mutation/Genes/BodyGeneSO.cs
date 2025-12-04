using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Body")]
public class BodyGeneSO : GeneSOBase
{
    public Sprite sprite;
    public List<Sprite> corpseSprites;
    public string corpseExplosionPoolName = "Explosion_Corpse_Red";

    [Header("Anchor Offsets")]
    public Vector2 headAnchorOffset = new Vector2(0, 0.9f);
    public Vector2 leftLegAnchorOffset = new Vector2(-0.3f, -0.9f);
    public Vector2 rightLegAnchorOffset = new Vector2(0.3f, -0.9f);

    [Header("Body and Health Settings")]
    public List<Vector2> bodyColliderPoints;
    public float startMass = 1f;
    public float startHealth = 10f;
    public SoundSO painSound;

    [Header("Shadow Settings")]
    public Vector2[] shadowShapePoints;
}