using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : MonoBehaviour
{
    public PlayerStats stats;
    [SerializeField] float stamina = 100f;
    [SerializeField] float staminaRegen = 5f;



    void Start()
    {
        stamina = stats.staminaMax;
    }

    void FixedUpdate()
    {
        StaminaRegen();
        //Debug.Log(stamina);
    }

    void StaminaRegen()
    {
        stamina += staminaRegen * Time.fixedDeltaTime;
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
