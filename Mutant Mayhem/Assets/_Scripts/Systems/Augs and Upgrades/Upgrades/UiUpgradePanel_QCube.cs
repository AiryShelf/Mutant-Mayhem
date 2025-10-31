using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiUpgradePanel_QCube : UiUpgradePanel
{
    [Header("QCube and Power")]
    [SerializeField] Slider cubeHealthSlider;
    [SerializeField] Image powerImage;
    [SerializeField] TextMeshProUGUI powerText;

    public override void OpenPanel(PanelInteract interactSource)
    {
        base.OpenPanel(interactSource);

        PowerManager.Instance.OnPowerChanged += UpdatePowerText;
        QCubeController.Instance.cubeHealth.OnCubeHealthChanged += UpdateCubeHealth;
        UpdatePowerText(PowerManager.Instance.powerBalance);
        UpdateCubeHealth(QCubeController.Instance.cubeHealth.GetHealth());
    }

    public override void ClosePanel()
    {
        base.ClosePanel();

        PowerManager.Instance.OnPowerChanged -= UpdatePowerText;
        QCubeController.Instance.cubeHealth.OnCubeHealthChanged -= UpdateCubeHealth;
    }

    void UpdateCubeHealth(float newHealth)
    {
        cubeHealthSlider.value = newHealth / QCubeController.Instance.cubeHealth.GetMaxHealth();
    }

    void UpdatePowerText(int powerBalance)
    {
        if (powerBalance >= 0)
            powerImage.color = Color.white;
        else 
            powerImage.color = Color.red;

        powerText.text = powerBalance.ToString();
    }
}
