using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyConsumer : MonoBehaviour
{
    [SerializeField] int supplyConsumptionAmount = 1;

    void OnEnable()
    {
        //Debug.Log("Supply consumer enabled, consuming supply of " + supplyConsumptionAmount);
        SupplyManager.SupplyBalance -= supplyConsumptionAmount;
    }

    void OnDisable()
    {
        //Debug.Log("Supply consumer disabled, restoring supply of " + supplyConsumptionAmount);
        SupplyManager.SupplyBalance += supplyConsumptionAmount;
    }
}
