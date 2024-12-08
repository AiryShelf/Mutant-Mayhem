using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    Health health { get; set; }
    bool isHit { get; set; }

    void ModifyHealth(float amount, float damageScale, Vector2 hitDir, GameObject gameObject);
    void Knockback(Vector2 dir, float knockback);
    void Die();

}
