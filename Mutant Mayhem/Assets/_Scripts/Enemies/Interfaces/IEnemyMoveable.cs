using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public interface IEnemyMoveable
{
    Rigidbody2D RB { get; set; }
    Vector2 FacingDirection { get; set; }

    void MoveEnemy(Vector2 velocity);
    void CheckFacingDirection(Vector2 velocity);
}
