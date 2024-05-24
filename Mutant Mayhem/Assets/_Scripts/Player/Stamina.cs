using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : MonoBehaviour
{
    [SerializeField] float stamina = 100f;
    [SerializeField] float staminaRegen = 5f;

    float maxStamina;


    void Awake()
    {
        maxStamina = stamina;
    }

    void Start()
    {
        
    }

    void Update()
    {
        StaminaRegen();
        //Debug.Log(stamina);
    }

    void StaminaRegen()
    {
        stamina += staminaRegen * Time.deltaTime;
        if (stamina > maxStamina)
        {
            stamina = maxStamina;
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

    public float GetMaxStamina()
    {
        return maxStamina;
    }
}
