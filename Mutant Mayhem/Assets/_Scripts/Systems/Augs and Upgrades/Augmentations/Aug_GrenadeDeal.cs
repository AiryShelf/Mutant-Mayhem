using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_GrenadeDeal_New", menuName = "Augmentations/Aug_GrenadeDeal")]
public class Aug_GrenadeDeal : AugmentationBaseSO
{
    public int grenadesAtStart; // Should match player's starting grenades*
    public int grenadesPerPurchaseDefault = 1;
    public int perLvlStartAddIncrement = 2;
    public float perLvlStartNegIncrement = 1;
    public float perLvlPurchaseAddInc = 0.5f;
    public float perLvlGrenadeCostMultInc = 0.5f;
    public float perLvlGrenadeCostNegInc = -0.5f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        Player player = FindObjectOfType<Player>();

        // Handle being at minLevel, and having no grenades to buy
        if (level <= minLvl)
        {
            // Zero grenade ammo
            player.stats.grenadeAmmo = 0;

            // Zero grenade purchases
            augManager.grenadeAmmoMult = 0;
            augManager.grenadeCostMult *= 0;

            return;
        }

        // Adjust grenade ammo and purchases for negative and positive levels
        if (level < 0)
        {
            player.stats.grenadeAmmo = grenadesAtStart + Mathf.CeilToInt(perLvlStartNegIncrement * level);
            // Force ammo mult to be 1 for negative levels, so it's not zero until min level
            if (level <= minLvl)
            {
                augManager.grenadeAmmoMult = 0;
                augManager.grenadeCostMult = 0;
            }
            else
            {
                augManager.grenadeAmmoMult = 1;
                augManager.grenadeCostMult = 1 + Mathf.Abs(perLvlGrenadeCostNegInc * level);
            }
        }
        else
        {
            player.stats.grenadeAmmo = grenadesAtStart + Mathf.FloorToInt(perLvlStartAddIncrement * level);

            augManager.grenadeAmmoMult = grenadesPerPurchaseDefault + Mathf.FloorToInt(perLvlPurchaseAddInc * level);
            augManager.grenadeCostMult = 1 + perLvlGrenadeCostMultInc * level;
        }

        Debug.Log("Aug applied Grenade Deal.  Ammo mult: " + augManager.grenadeAmmoMult + ", Cost mult: " + augManager.grenadeCostMult);
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float totalGrenadesAtStart = grenadesAtStart + (perLvlStartAddIncrement * level);
        float totalGrenadesPerPurchase = grenadesPerPurchaseDefault + Mathf.FloorToInt(perLvlPurchaseAddInc * level);
        float totalGrenadeCost = 1 + perLvlGrenadeCostMultInc * level;
        string description = "Start with " + totalGrenadesAtStart + " grenades.  Plus each time you purchase grenades, " +
                             "get " + totalGrenadesPerPurchase + " grenades for " + totalGrenadeCost + " times the cost of one";
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float totalGrenadesAtStart = Mathf.Clamp(grenadesAtStart + Mathf.CeilToInt(perLvlStartNegIncrement * level), 0, int.MaxValue);

        // Make it so player can still buy grenades at negative levels, but can't at min level
        float totalGrenadesPerPurchase;
        if (level > minLvl)
        {
            totalGrenadesPerPurchase = 1;
        }
        else
            totalGrenadesPerPurchase = 0;

        float totalGrenadeCost = 1 + perLvlGrenadeCostNegInc * -level;

        // Handle different text for not being able to buy any grenades
        string description;
        if (totalGrenadesPerPurchase > 0 )
        {
            description = "Start with " + totalGrenadesAtStart + " grenades.  Plus each time you buy a grenade, " +
                             "get " + totalGrenadesPerPurchase + " for " + totalGrenadeCost + " times the cost";

            return description;
        }
        
        description = "Start with " + totalGrenadesAtStart + " grenades.  " +
                          "You also can't buy any.  No grenades for you!";
                          
        return description;
    }

    public override string GetNeutralDescription(AugManager augManager, int level)
    {
        return "Raise or lower the level to adjust starting grenades and grenade cost";
    }
}
