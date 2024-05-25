using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Shoot-Straight-Single Projectile", menuName = "Enemy Logic/Shoot Logic/Straight Single Projectile")]
public class EnemyAttackSingleStraightProjectile : EnemyShootSOBase
{
    [SerializeField] private Rigidbody2D BulletPrefab;  
    [SerializeField] private float _timeBetweenShots = 1f;   
    [SerializeField] private float _timeTillExit = 3f;
    [SerializeField] private float _distanceToCountExit = 3f;
    [SerializeField] private float _bulletSpeed = 10f;

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

        if (_timer > _timeBetweenShots)
        {
            _timer = 0f;

            Vector2 dir = (playerTransform.position - enemyBase.transform.position).normalized;

            Rigidbody2D bullet = GameObject.Instantiate(BulletPrefab, enemyBase.transform.position, Quaternion.identity);
            bullet.velocity = dir * _bulletSpeed;
        }

        // Dont do this every frame, distance checks are expensive and there are better ways
        if (Vector2.Distance(playerTransform.position, enemyBase.transform.position) > _distanceToCountExit)
        {
            _exitTimer += Time.deltaTime;

            if (_exitTimer > _timeTillExit)
            {
                enemyBase.StateMachine.ChangeState(enemyBase.ChaseState);
            }
        }
        else
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

