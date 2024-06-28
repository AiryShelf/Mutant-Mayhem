using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : MonoBehaviour
{
    public PlayerStats stats;
    [SerializeField] float stamina = 100f;

    void Start()
    {
        stamina = stats.staminaMax;
    }

    void FixedUpdate()
    {
        StaminaRegen();
    }

    void StaminaRegen()
    {
        stamina += stats.staminaRegen * Time.fixedDeltaTime;
        if (stamina > stats.staminaMax)
        {
            stamina = stats.staminaMax;
        }
    }

    public void ModifyStamina(float amount)
    {
        stamina += amount;
    }

    public float GetStamina()
    {
        return stamina;
    }
}
