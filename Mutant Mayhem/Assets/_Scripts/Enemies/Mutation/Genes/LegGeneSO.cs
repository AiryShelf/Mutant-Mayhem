using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyEvolution/Genes/Leg")]
public class LegGeneSO : GeneSOBase
{
    public List<Sprite> leftLegCorpseSprites;
    public List<Sprite> rightLegCorpseSprites;
    public RuntimeAnimatorController leftLegAnimatorController;
    public RuntimeAnimatorController rightLegAnimatorController;

    [Header("Behavior Settings")]
    public EnemyIdleSOBase idleSOBase;
    public EnemyChaseSOBase chaseSOBase;
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