using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDStatsPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI creditsText;
    [SerializeField] Slider healthSlider;
    [SerializeField] Slider staminaSlider;

    Player player;
    Health playerHealthScript;
    Stamina playerStaminaScript;
    float playerHealth;
    float maxHealth;
    float playerStamina;
    float maxStamina;

    void Start()
    {
        player = FindObjectOfType<Player>();
        playerHealthScript = player.GetComponent<Health>();
        playerStaminaScript = player.GetComponent<Stamina>();
    }

    void FixedUpdate()
    {
        maxHealth = playerHealthScript.GetMaxHealth();
        maxStamina = playerStaminaScript.GetMaxStamina();
        playerHealth = playerHealthScript.GetHealth();
        playerStamina = playerStaminaScript.GetStamina();
        healthSlider.value = playerHealth / maxHealth;
        staminaSlider.value = playerStamina / maxStamina;
        creditsText.text = "Credits: " + BuildingSystem.PlayerCredits.ToString();
    }

}
