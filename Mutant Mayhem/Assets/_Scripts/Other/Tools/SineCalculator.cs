using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineCalculator : MonoBehaviour
{
    public static float sine_Freq2_Val;

    void FixedUpdate()
    {
        sine_Freq2_Val = Mathf.Sin(Time.time * 2);
    }
}
