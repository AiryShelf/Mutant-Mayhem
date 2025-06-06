using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Leg")]
public class LegGeneSO : ScriptableObject       // 🔸 NEW
{
    public string id = "Leg_Default";
    public RuntimeAnimatorController leftLegAnimatorController;
    public RuntimeAnimatorController rightLegAnimatorController;
    public float scale = 1f;

    [Header("Behavior Settings")]
    public EnemyIdleSOBase idleSOBase;
    public EnemyChaseSOBase chaseSOBase;
    public EnemyShootSOBase shootSOBase;
    public bool isFlying = false;

    [Header("Leg Animation Settings")]
    public float animSpeedFactor;
    public float switchToRunBuffer;
    public float maxAnimSpeed;
}