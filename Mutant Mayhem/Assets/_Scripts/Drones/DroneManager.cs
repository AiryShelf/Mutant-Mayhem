using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public List<Drone> allDrones;
    public List<bool> unlockedDrones;
    public int activeDroneCount = 0;
    public int activeConstructionDrones = 0;
    public int activeAttackDrones = 0;

    public static DroneManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }  

    public bool SpawnDroneInHangar(DroneType droneType, DroneHangar droneHangar)
    {
        if (droneHangar.controlledDrones.Count >= droneHangar.maxDrones)
            return false;

        string poolName = "";
        switch (droneType)
        {
            case DroneType.Builder:
                poolName = "Drone_Construction";
                activeConstructionDrones++;
                break;
        }

        foreach (Drone drone in allDrones)
        {
            if (drone.droneType == droneType)
            {
                Drone newDrone = PoolManager.Instance.GetFromPool(poolName).GetComponent<Drone>();
                droneHangar.AddDrone(newDrone);
                activeDroneCount++;
                return true;
            }
        }

        return false;
    }

    public void RemoveDrone(Drone drone)
    {
        activeDroneCount--;

        switch (drone.droneType)
        {
            case DroneType.Builder:
                activeConstructionDrones--;
                break;
            case DroneType.Attacker:
                activeAttackDrones--;
                break;
        }
    }
}
