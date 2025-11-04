using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SupplyManager : MonoBehaviour
{
    public static SupplyManager Instance { get; private set; }

    public int startingSupplyLimit;
    public static Action<int> OnSupplyLimitChanged;
    static int _excessSupply;
    static int _supplyLimit;
    public static int SupplyLimit
    {
        get => _supplyLimit;
        set
        {
            if (_supplyLimit == value) return;
            _supplyLimit = value;
            OnSupplyLimitChanged?.Invoke(value);
            Debug.Log($"Supply limit changed to {_supplyLimit}");
            LimitSupplyProduction();
        }
    }

    public static Action<int> OnSupplyConsumptionChanged;
    static int _supplyConsumption;
    public static int SupplyConsumption
    {
        get => _supplyConsumption;
        set
        {
            if (_supplyConsumption == value) return;
            _supplyConsumption = value;
            OnSupplyConsumptionChanged?.Invoke(value);

            if (_supplyBalance != _supplyProduction - _supplyConsumption)
            {
                SupplyBalance = _supplyProduction - _supplyConsumption;
            }
            LimitSupplyProduction();
        }
    }

    public static Action<int> OnSupplyProductionChanged;
    static int _supplyProduction;
    public static int SupplyProduced
    {
        get => _supplyProduction;
        set
        {
            if (_supplyProduction == value) return;
            _supplyProduction = value;
            OnSupplyProductionChanged?.Invoke(value);

            if (_supplyBalance != _supplyProduction - _supplyConsumption)
            {
                SupplyBalance = _supplyProduction - _supplyConsumption;
            }
            LimitSupplyProduction();
        }
    }
    
    public static Action<int> OnSupplyBalanceChanged;
    static int _supplyBalance;
    public static int SupplyBalance
    {
        get => _supplyBalance;
        private set
        {
            if (_supplyBalance == value) return;
            int delta = value - _supplyBalance;
            _supplyBalance = value;
            OnSupplyBalanceChanged?.Invoke(value);
            //Debug.Log($"Supply balance changed by {delta} (new total: {_supplyBalance})");
            LimitSupplyProduction();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        SupplyLimit = startingSupplyLimit;
        SupplyBalance = 0;
        SupplyProduced = 0;
        SupplyConsumption = 0;
    }

    // Called when Produced supply exceeds the limit
    static void LimitSupplyProduction()
    {
        if (_excessSupply <= 0 && SupplyProduced <= SupplyLimit) return;

        // Adjust PowerBalance to not exceed SupplyLimit
        int excessSupply = SupplyProduced - SupplyLimit;
        _excessSupply = excessSupply;

        if (excessSupply <= 0)
        {
            excessSupply = 0;
            _excessSupply = 0;
        }

        SupplyBalance = SupplyProduced - excessSupply - SupplyConsumption;
        //Debug.Log($"Supply production limited by {excessSupply} to not exceed limit of {SupplyLimit}");
    }
}
