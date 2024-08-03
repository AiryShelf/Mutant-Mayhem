using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITriggerCheckable
{
    bool IsAggroed { get; set; }
    bool IsWithinShootDistance { get; set; }

    void SetAggroStatus(bool isAggroed);
    void SetShootDistanceBool(bool IsWithinShootDistance);
}
