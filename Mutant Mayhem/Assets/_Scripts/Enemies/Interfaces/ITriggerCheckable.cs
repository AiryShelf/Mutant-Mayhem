using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITriggerCheckable
{
    bool IsAggroed { get; set; }
    bool IsWithinMeleeDistance { get; set; }

    void SetAggroStatus(bool isAggroed);
    void SetMeleeDistanceBool(bool IsWithinMeleeDistance);
}
