using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyProducer : MonoBehaviour
{
    [SerializeField] int supplyAmount = 8;

    void OnEnable()
    {
        //Debug.Log($"Supply producer enabled, adding supply of {supplyAmount} from {gameObject.name}");
        SupplyManager.SupplyProduced += supplyAmount;
    }

    void OnDisable()
    {
        //Debug.Log($"Supply producer disabled, removing supply of {supplyAmount} from {gameObject.name}");
        SupplyManager.SupplyProduced -= supplyAmount;
    }
}
