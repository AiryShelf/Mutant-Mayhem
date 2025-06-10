using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Head")]
public class HeadGeneSO : GeneSOBase
{
    public Sprite sprite;

    [Header("Melee Settings")]
    public Vector2 meleeColliderOffset = new Vector2(0, 0);
    public Vector2 meleeColliderSize = new Vector2(1, 1);
    public float meleeDamage = 1f;
    public float attackDelay = 1f;
    public float knockback = 1f;
    public float selfKnockback = 1f;
    public float massModFactor = 1f;
}