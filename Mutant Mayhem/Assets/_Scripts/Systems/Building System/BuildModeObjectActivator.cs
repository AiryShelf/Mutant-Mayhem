using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeObjectActivator : MonoBehaviour
{
    [SerializeField] GameObject objectToToggle;

    [Header("Optional:")]
    [SerializeField] List<StructureType> typesToMatch = new List<StructureType>();
    [SerializeField] RangeCircle rangeCircleToSet;
    [SerializeField] StructureType rangeCircleType;

    void Start()
    {
        BuildingSystem.Instance.OnBuildMenuOpen += BuildMenuOpen;
        BuildMenuOpen(BuildingSystem.Instance.isInBuildMode);
    }

    void OnDestroy()
    {
        BuildingSystem.Instance.OnBuildMenuOpen -= BuildMenuOpen;
    }

    void BuildMenuOpen(bool open)
    {
        if (open)
        {
            //Debug.Log("BuildModeObjectActivator: Build Menu found opened for " + objectToToggle.name);
            if (typesToMatch.Count > 0)
            {
                StartCoroutine(CheckForMatch());
            }
            else
            {
                objectToToggle.SetActive(true);
                //Debug.Log($"BuildModeObjectActivator: No types to match, so {objectToToggle.name} activated by default.");
            }
        }
        else
        {
            //Debug.Log($"BuildModeObjectActivator: Build Menu found closed for {objectToToggle.name}, deactivating object.");
            StopAllCoroutines();
            objectToToggle.SetActive(false);
        }
    }

    IEnumerator CheckForMatch()
    {
        //Debug.Log("BuildModeObjectActivator: Starting to check for structure type match for " + objectToToggle.name);
        while (true)
        {
            bool matched = typesToMatch.Contains(BuildingSystem.Instance.structureInHand?.structureType ?? StructureType.SelectTool);
            objectToToggle.SetActive(matched);
            if (matched)
            {
                switch (rangeCircleType)
                {
                    case StructureType.GunTurret:
                        // Match the range to the turret gun's detect range by checking gunType in TurretGunSO list
                        TurretGunSO turretGun = TurretManager.Instance.turretGunList.Find(g => g.gunType == GunType.Bullet);
                        if (turretGun == null)
                        {
                            Debug.LogError("BuildModeObjectActivator: Could not find TurretGunSO with GunType.Bullet to set range circle");
                            break;
                        }
                        rangeCircleToSet.radius = turretGun != null ? turretGun.detectRange : 2f;
                        break;
                    case StructureType.LaserTurret:
                        TurretGunSO laserTurretGun = TurretManager.Instance.turretGunList.Find(g => g.gunType == GunType.Laser);
                        if (laserTurretGun == null)
                        {
                            Debug.LogError("BuildModeObjectActivator: Could not find TurretGunSO with GunType.Laser to set range circle");
                            break;
                        }
                        rangeCircleToSet.radius = laserTurretGun != null ? laserTurretGun.detectRange : 2f;
                        break;
                    case StructureType.DroneHangar:
                        rangeCircleToSet.radius = DroneManager.Instance.droneHangarRange;
                        break;
                    default:
                        break;
                }
            }  
            
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
