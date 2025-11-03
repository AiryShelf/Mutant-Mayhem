using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyManager : MonoBehaviour
{
    public static SupplyManager Instance { get; private set; }
    public static Action<int> OnSupplyBalanceChanged;

    private static int _supplyBalance;
    public static int SupplyBalance
    {
        get => _supplyBalance;
        set
        {
            if (_supplyBalance == value) return;
            int delta = value - _supplyBalance;
            _supplyBalance = value;
            OnSupplyBalanceChanged?.Invoke(value);
            //Debug.Log($"Supply balance changed by {delta} (new total: {_supplyBalance})");
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
