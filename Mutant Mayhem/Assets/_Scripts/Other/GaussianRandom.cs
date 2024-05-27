using UnityEngine;
using System;

public class GaussianRandom
{
    private readonly System.Random _random = new System.Random();

    public double NextDouble(double mean, double standardDeviation)
    {
        double u1 = 1.0 - _random.NextDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - _random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1))
                               * Math.Sin(2.0 * Mathf.PI * u2); //random normal(0,1)
        double randNormal =
             mean + standardDeviation * randStdNormal; //random normal(mean,stdDev^2)
        return randNormal;
    }
}
