using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeRange : MonoBehaviour
{
    public StructureSO myStructure;
    public RangeCircle lineRendererCircle;

    void OnEnable()
    {
        SetObjectRangeCircle();
    }

    public void SetObjectRangeCircle()
    {
        float radius = 0;
        if (myStructure.isTurret)
        {
            
            if (myStructure.structureType == StructureType.LaserTurret)
                radius = TurretManager.Instance.turretGunList[0].detectRange;
            else if (myStructure.structureType == StructureType.GunTurret)
                radius = TurretManager.Instance.turretGunList[1].detectRange;
        }
        lineRendererCircle.radius = radius;
    }
}
