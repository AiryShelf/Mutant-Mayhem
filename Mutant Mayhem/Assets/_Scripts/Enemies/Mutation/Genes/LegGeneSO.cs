using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Leg")]
public class LegGeneSO : GeneSOBase
{
    public RuntimeAnimatorController leftLegAnimatorController;
    public RuntimeAnimatorController rightLegAnimatorController;

    [Header("Behavior Settings")]
    public EnemyIdleSOBase idleSOBase;
    public EnemyChaseSOBase chaseSOBase;
    public EnemyShootSOBase shootSOBase;
    public bool isFlying = false;

    [Header("Movement Settings")]
    public float moveSpeedBaseStart = 1f;
    public float rotateSpeedBaseStart = 1f;

    [Header("Leg Animation Settings")]
    public float animSpeedFactor;
    public float switchToRunBuffer;
    public float maxAnimSpeed;

    [Header("Shadow Settings")]
    public bool bodyCastsShadows = true;
    
}