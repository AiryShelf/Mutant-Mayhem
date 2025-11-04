using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiUpgradePanel_EngineeringBay : UiUpgradePanel
{
    [Header("Engineering Bay")]
    [SerializeField] Image powerImage;
    [SerializeField] TextMeshProUGUI powerText;
    [SerializeField] Image supplyImage;
    [SerializeField] TextMeshProUGUI supplyBalanceText;
    [SerializeField] TextMeshProUGUI supplyLimitText;

    public override void OpenPanel(PanelInteract interactSource)
    {
        base.OpenPanel(interactSource);
        PowerManager.Instance.OnPowerChanged += UpdatePowerText;
        SupplyManager.OnSupplyConsumptionChanged += UpdateSupplyConsumptionVsGeneration;
        SupplyManager.OnSupplyProductionChanged += UpdateSupplyConsumptionVsGeneration;
        SupplyManager.OnSupplyLimitChanged += UpdateSupplyLimit;

        UpdatePowerText(PowerManager.Instance.powerBalance);
        UpdateSupplyConsumptionVsGeneration(SupplyManager.SupplyProduced - SupplyManager.SupplyConsumption);
        UpdateSupplyLimit(SupplyManager.SupplyLimit);
    }

    public override void ClosePanel()
    {
        base.ClosePanel();
        PowerManager.Instance.OnPowerChanged -= UpdatePowerText;
        SupplyManager.OnSupplyConsumptionChanged -= UpdateSupplyConsumptionVsGeneration;
        SupplyManager.OnSupplyProductionChanged -= UpdateSupplyConsumptionVsGeneration;
        SupplyManager.OnSupplyLimitChanged -= UpdateSupplyLimit;
    }

    void UpdatePowerText(int powerBalance)
    {
        if (powerBalance >= 0)
            powerImage.color = Color.white;
        else
            powerImage.color = Color.red;

        powerText.text = powerBalance.ToString();
    }

    void UpdateSupplyConsumptionVsGeneration(int supplyBalance)
    {
        Color color;
        if (supplyBalance >= 0)
        {
            color = Color.white;
        }
        else
        {
            color = Color.red;
        }

        supplyBalanceText.text = $"Used: {SupplyManager.SupplyConsumption}/{SupplyManager.SupplyProduced}";
        supplyBalanceText.color = color;
        supplyImage.color = color;
    }
    
    void UpdateSupplyLimit(int supplyLimit)
    {
        supplyLimitText.text = $"Limit: {supplyLimit}";
    }
}
