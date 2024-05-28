using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QCubeHealth : Health
{
    public override void Die()
    {
        QCubeController.IsDead = true;
    }   
}