using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    Health health { get; set; }

    void ModifyHealth(float amount);

    void Knockback(Vector2 dir, float knockback);

    void BulletHitEffect(Vector2 hitPos, Vector2 hitDir);

    void MeleeHitEffect(Vector2 hitPos, Vector2 hitDir);

    void Die();

}
