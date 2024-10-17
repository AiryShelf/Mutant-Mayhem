using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyMoveable
{
    Rigidbody2D rb { get; set; }
    Vector2 facingDirection { get; set; }

    void MoveEnemy(Vector2 velocity);
    void ChangeFacingDirection(Vector2 velocity, float speed);
}
