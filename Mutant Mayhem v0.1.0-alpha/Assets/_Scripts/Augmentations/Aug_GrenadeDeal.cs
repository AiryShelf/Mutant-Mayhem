using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_GrenadeDeal_New", menuName = "Augmentations/Aug_GrenadeDeal")]
public class Aug_GrenadeDeal : AugmentationBaseSO
{
    public int grenadesAddAtStart; // Should match player's starting grenades*
    public int grenadesPerPurchase;
    public float grenadeCostMult;
    public int lvlStartIncrement = 2;
    public float lvlPerPurchaseAddInc = 0.5f;
    public float lvlGrenadeCostMultInc = 0.5f;
    public float lvlGrenadeCostNegInc = 0.5f;

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

        // Adjust grenade purchases for negative and positive levels
        player.stats.grenadeAmmo += Mathf.FloorToInt(grenadesAddAtStart + (lvlStartIncrement * level));
        if (level < 0)
        {
            // Force ammo mult to be 1 for negative levels, so it's not zero until min level
            
            if (level <= minLvl)
            {
                augManager.grenadeAmmoMult = 0;
                augManager.grenadeCostMult = 0;
            }
            else
            {
                augManager.grenadeAmmoMult = 1;
                augManager.grenadeCostMult = grenadeCostMult + Mathf.Abs(lvlGrenadeCostNegInc * level);
            }
        }
        else
        {
            augManager.grenadeAmmoMult = Mathf.FloorToInt(grenadesPerPurchase + (lvlPerPurchaseAddInc * level));
            augManager.grenadeCostMult = grenadeCostMult + (lvlGrenadeCostMultInc * level);
        }

        Debug.Log("Aug applied Grenade Deal.  Ammo mult: " + augManager.grenadeAmmoMult + ", Cost mult: " + augManager.grenadeCostMult);
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float totalGrenadesAtStart = grenadesAddAtStart + (lvlStartIncrement * level);
        float totalGrenadesPerPurchase = Mathf.FloorToInt(grenadesPerPurchase + (lvlPerPurchaseAddInc * level));
        float totalGrenadeCost = grenadeCostMult + (lvlGrenadeCostMultInc * level);
        string description = "Start with " + totalGrenadesAtStart + " grenades.  Plus each time you purchase grenades, " +
                             "get " + totalGrenadesPerPurchase + " grenades for " + totalGrenadeCost + " times the cost of one";
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float totalGrenadesAtStart = Mathf.Clamp(Mathf.FloorToInt(grenadesAddAtStart + (lvlStartIncrement * level)), 0, int.MaxValue);

        // Make it so player can still buy grenades at negative levels, but can't at min level
        float totalGrenadesPerPurchase;
        if (level > minLvl)
        {
            totalGrenadesPerPurchase = 1;
        }
        else
            totalGrenadesPerPurchase = 0;

        float totalGrenadeCost = grenadeCostMult + (lvlGrenadeCostNegInc * -level);

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
