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
    }
}
