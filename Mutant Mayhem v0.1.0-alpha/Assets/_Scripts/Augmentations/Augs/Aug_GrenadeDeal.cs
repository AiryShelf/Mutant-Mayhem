using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_GrenadeDeal_New", menuName = "Augmentations/Aug_GrenadeDeal")]
public class Aug_GrenadeDeal : AugmentationBaseSO
{
    public int amountToStartWith;
    public int grenadesPerPurchase;
    public float grenadeCostMult;
    public int lvlStartIncrement = 3;
    public float lvlGrenadeCostIncrement = 0.5f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        // Adjust grenade ammo
        Player player = FindObjectOfType<Player>();
        player.stats.grenadeAmmo += amountToStartWith;

        // Adjust grenade purchases
        augManager.grenadeAmmoMult = grenadesPerPurchase;
        augManager.grenadeCostMult *= grenadeCostMult;
    }
}
