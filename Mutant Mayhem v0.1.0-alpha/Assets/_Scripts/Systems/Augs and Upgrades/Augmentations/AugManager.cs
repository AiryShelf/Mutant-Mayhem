using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AugManager : MonoBehaviour
{
    public static AugManager Instance { get; private set; }

    public List<AugmentationBaseSO> availableAugmentations;
    [SerializeField] int maxAugmentationsStart;
    public Dictionary<AugmentationBaseSO, int> selectedAugsWithLvls = new Dictionary<AugmentationBaseSO, int>();

    [Header("Dynamic values to apply Augs, don't set here")]
    public int maxAugs;
    public int currentResearchPoints;

    string lastUsedProfile = "";

    // Vals to pass to other systems and objects
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
        // Only reset if profile has changed
        if (lastUsedProfile != null && lastUsedProfile.Equals(ProfileManager.Instance.currentProfile.profileName))
            return;
        
        //Reset
        selectedAugsWithLvls.Clear();
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

    public int GetCurrentLevelCount()
    {
        int lvls = 0;
        foreach (KeyValuePair<AugmentationBaseSO, int> kvp in selectedAugsWithLvls)
        {
            lvls += Mathf.Abs(kvp.Value);
        }

        return lvls;
    }

    public void IncrementLevel(AugmentationBaseSO aug, bool addingLevel)
    {
        if (addingLevel)
        {
            if (!selectedAugsWithLvls.ContainsKey(aug))
                selectedAugsWithLvls.Add(aug, 1);
            else
                selectedAugsWithLvls[aug]++;
        }
        else
        {
            if (!selectedAugsWithLvls.ContainsKey(aug))
                selectedAugsWithLvls.Add(aug, -1);
            else
                selectedAugsWithLvls[aug]--;
        }

        if (selectedAugsWithLvls.ContainsKey(aug))
        {
            if (selectedAugsWithLvls[aug] == 0)
            {
                selectedAugsWithLvls.Remove(aug);
                Debug.Log("Removed an aug from manager");
            }
        }
    }
}
