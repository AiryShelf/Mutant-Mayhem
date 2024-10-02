using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupsReceiver : PickupsContainerBase
{
    [SerializeField] GameObject textFlyPrefab;

    public override void AddToContainer(Pickup pickup)
    {
        base.AddToContainer(pickup);

        // Apply credits
        int credits = pickup.pickupData.credits;
        BuildingSystem.PlayerCredits += credits;
        PlayCreditsEffects(credits);
        Debug.Log("Added " + credits + " Credits");

        container.Remove(pickup);
        Destroy(pickup.gameObject);
    }

    void PlayCreditsEffects(int credits)
    {
        GameObject textFly = Instantiate(textFlyPrefab, transform.position, Quaternion.identity);
        float angle = Random.Range(0f, Mathf.PI * 2);
        Vector2 flyDir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        textFly.GetComponent<TextFly>().Initialize("+ " + credits + " C", flyDir, true);
    }

}
