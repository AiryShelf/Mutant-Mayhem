using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Shoot-Straight-Single Projectile", menuName = "Enemy Logic/Shoot Logic/Straight Single Projectile")]
public class EnemyAttackSingleStraightProjectile : EnemyShootSOBase
{
    [SerializeField] private Rigidbody2D BulletPrefab;  
    [SerializeField] private float _timeBetweenShots = 1f;   
    [SerializeField] private float _timeTillExit = 3f;
    [SerializeField] private float _bulletSpeed = 10f;

    private float _timer;
    Coroutine WaitToExit;

    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void DoEnterLogic() 
    {
        base.DoEnterLogic();

        WaitToExit = enemyBase.StartCoroutine(StateChangeCheck());
        // Shoot next physicsUpdate
        _timer = _timeBetweenShots;
    }
    public override void DoExitLogic() 
    {
        base.DoExitLogic();

        enemyBase.StopCoroutine(WaitToExit);
    }
    public override void DoFrameUpdateLogic() 
    { 
        base.DoFrameUpdateLogic();
    }

    public override void DoPhysicsUpdateLogic() 
    {
        base.DoPhysicsUpdateLogic();

        enemyBase.MoveEnemy(Vector2.zero);

        if (_timer > _timeBetweenShots)
        {
            _timer = 0f;

            Vector2 dir = (playerTransform.position - enemyBase.transform.position).normalized;

            Rigidbody2D bullet = GameObject.Instantiate(BulletPrefab, 
                                            enemyBase.transform.position, Quaternion.identity);
            bullet.velocity = dir * _bulletSpeed;
        }
        _timer += Time.deltaTime;  
    }
    
    public override void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType) 
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }
    
    public override void ResetValues() 
    {
        base.ResetValues();
    }

    IEnumerator StateChangeCheck()
    {
        while (true)
        {
            //Debug.Log("State change coroutine running");
            yield return new WaitForSeconds(_timeTillExit);
            
            if (!enemyBase.IsWithinShootDistance)
            {
                enemyBase.StateMachine.ChangeState(enemyBase.ChaseState);
            }
        }
    }
}

