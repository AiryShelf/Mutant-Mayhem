using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AugManager : MonoBehaviour
{
    public static AugManager Instance { get; private set; }

    public List<AugmentationBaseSO> availableAugmentations;
    [SerializeField] int maxAugmentationsStart;

    [Header("Dynamic values to apply Augs, don't set here")]
    public Dictionary<AugmentationBaseSO, int> selectedAugsWithLvls = new Dictionary<AugmentationBaseSO, int>();
    public int maxAugs;
    public int currentResearchPoints;
    public int grenadeAmmoMult = 1;
    public float grenadeCostMult = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Initialize()
    {
        ClearSelectedAugmentations();
        maxAugs = maxAugmentationsStart;
        currentResearchPoints = ProfileManager.Instance.currentProfile.researchPoints;

        grenadeAmmoMult = 1;
        grenadeCostMult = 1;
    }

    public void ApplySelectedAugmentations()
    {
        foreach (KeyValuePair<AugmentationBaseSO, int> kvp in selectedAugsWithLvls)
        {
            kvp.Key.ApplyAugmentation(this, kvp.Value);
        }
    }

    public void ClearSelectedAugmentations()
    {
        selectedAugsWithLvls.Clear();
    }
}
