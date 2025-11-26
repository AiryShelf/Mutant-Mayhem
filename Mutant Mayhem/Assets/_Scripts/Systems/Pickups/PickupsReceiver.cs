using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupsReceiver : PickupsContainerBase
{
    [SerializeField] Color creditsColor;
    [SerializeField] float textFlyAlphaMax;
    [SerializeField] float textPulseScaleMax = 1.5f;

    public override void AddToContainer(Pickup pickup)
    {
        base.AddToContainer(pickup);

        // Apply credits
        int credits = pickup.pickupData.credits;
        BuildingSystem.PlayerCredits += credits;
        PlayCreditsEffects(credits);
        //Debug.Log("Added " + credits + " Credits");

        container.Remove(pickup);

        // Return pickup to pool
        PoolManager.Instance.ReturnToPool("Pickup", pickup.gameObject);
    }

    void PlayCreditsEffects(int credits)
    {
        GameObject textFly = PoolManager.Instance.GetFromPool("TextFlyWorld_Credits");
        float angle = Random.Range(0f, Mathf.PI * 2);
        Vector2 flyDir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        textFly.transform.position = transform.position;
        string creditsCommas = credits.ToString("N0");
        textFly.GetComponent<TextFly>().Initialize("+ " + creditsCommas + " C", creditsColor, textFlyAlphaMax, flyDir, true, textPulseScaleMax);
    }
}
