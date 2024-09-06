using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    Health health { get; set; }

    bool isHit { get; set; }

    float unfreezeTime { get; set; }

    Coroutine unfreezeAfterTime { get; set; }

    void ModifyHealth(float amount, GameObject gameObject);

    void StartFreeze();

    IEnumerator UnfreezeAfterTime();

    void Knockback(Vector2 dir, float knockback);

    void BulletHitEffect(Vector2 hitPos, Vector2 hitDir);

    void MeleeHitEffect(Vector2 hitPos, Vector2 hitDir);

    void Die();

}
