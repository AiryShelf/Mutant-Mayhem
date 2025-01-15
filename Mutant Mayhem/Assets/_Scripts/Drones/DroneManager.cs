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
        string poolName = "";
        switch (droneType)
        {
            case DroneType.Builder:
                poolName = "Drone_Construction";
                if (droneHangar.GetDroneCount(DroneType.Builder) >= droneHangar.maxConstructionDrones)
                {
                    MessagePanel.PulseMessage("Drone Hangar is full", Color.red);
                    return false;
                }
                break;
            case DroneType.Attacker:
                poolName = "Drone_Attack";
                if (droneHangar.GetDroneCount(DroneType.Attacker) >= droneHangar.maxAttackDrones)
                {
                    MessagePanel.PulseMessage("Drone Hangar is full", Color.red);
                    return false;
                }
                break;
        }

        Drone newDrone = PoolManager.Instance.GetFromPool(poolName).GetComponent<Drone>();
        if (newDrone != null)
        {
            droneHangar.AddDrone(newDrone);
            newDrone.Initialize();
            activeDroneCount++;
            if (newDrone is AttackDrone)
                activeAttackDrones++;
            else
                activeConstructionDrones++;
            return true;
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

        PoolManager.Instance.ReturnToPool(drone.objectPoolName, drone.gameObject);
    }
}
