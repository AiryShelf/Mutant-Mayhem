using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DroneInfoIcon : MonoBehaviour
{
    [SerializeField] Sprite constructionDroneSprite;
    [SerializeField] Sprite attackDroneSprite;
    [SerializeField] Image droneIconImage;
    [SerializeField] Slider droneHealthSlider;
    [SerializeField] RadialEnergyUI radialEnergyUI;
    [SerializeField] float chargingEffectScale = 3f;
    int previousEnergy = -1;

    public void SetIconState(Drone drone)
    {
        switch (drone.droneType)
        {
            case DroneType.Builder:
                droneIconImage.sprite = constructionDroneSprite;
                break;
            case DroneType.Attacker:
                droneIconImage.sprite = attackDroneSprite;
                break;
        }

        float healthPercent = drone.droneHealth.GetHealth() / drone.droneHealth.GetMaxHealth();
        droneHealthSlider.value = healthPercent;

        radialEnergyUI.OnEnergyChanged(drone.energy, drone.energyMax);

        if (drone.energy > previousEnergy)
        {
            // Scale energy bar up to indicate recharge
            radialEnergyUI.transform.localScale = Vector3.one * chargingEffectScale;
        }
        else
        {
            radialEnergyUI.transform.localScale = Vector3.one;
        }

        previousEnergy = drone.energy;
    }
}
