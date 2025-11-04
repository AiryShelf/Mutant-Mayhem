using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeRange : MonoBehaviour
{
    public StructureSO myStructure;
    public RangeCircle rangeCircle;

    void OnEnable()
    {
        SetObjectRangeCircle();
    }

    void OnDisable()
    {
        rangeCircle.EnableCircle(false);
    }

    void SetObjectRangeCircle()
    {
        float radius = 0;
        if (myStructure.isTurret)
        {
            if (myStructure.structureType == StructureType.LaserTurret)
                radius = TurretManager.Instance.turretGunList[0].detectRange;
            else if (myStructure.structureType == StructureType.GunTurret)
                radius = TurretManager.Instance.turretGunList[1].detectRange;
        }
        else if (myStructure.structureType == StructureType.DroneHangar)
        {
            radius = DroneManager.Instance.droneHangarRange;
        }
        rangeCircle.radius = radius;

        rangeCircle.EnableCircle(true);
    }
}
