using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Melee-Single Forward", menuName = "Enemy Logic/Melee Logic/Single Forward Melee")]
public class EnemyMelee : EnemyMeleeSOBase
{  
    [SerializeField] private MeleeControllerEnemy meleeControllerEnemy;
    [SerializeField] private float _timeBetweenAttack = 1f;   
    [SerializeField] private float _timeTillExit = 3f;

    private float _timer;
    private float _exitTimer;

    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void DoEnterLogic() 
    {
        base.DoEnterLogic();
    }
    public override void DoExitLogic() 
    {
        base.DoExitLogic();
    }
    public override void DoFrameUpdateLogic() 
    { 
        base.DoFrameUpdateLogic();

        enemyBase.MoveEnemy(Vector2.zero);

        if (_timer > _timeBetweenAttack)
        {
            _timer = 0f;

            Vector2 dir = (playerTransform.position - enemyBase.transform.position).normalized;
            meleeControllerEnemy.isAttacking = true;
            
        }

        
        
        if (_exitTimer >= _timeTillExit)
        {
            _exitTimer = 0f;
        }

        _timer += Time.deltaTime;
    }
    public override void DoPhysicsUpdateLogic() 
    {
        base.DoPhysicsUpdateLogic();
    }
    public override void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType) 
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }
    public override void ResetValues() 
    {
        base.ResetValues();
    }
}

