using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AugManager : MonoBehaviour
{
    public static AugManager Instance { get; private set; }

    public List<AugmentationBaseSO> availableAugmentations;
    [SerializeField] int maxAugmentationsStart;
    public static Dictionary<AugmentationBaseSO, int> selectedAugsWithLvls = new Dictionary<AugmentationBaseSO, int>();
    public static Dictionary<AugmentationBaseSO, int> selectedAugsTotalCosts = new Dictionary<AugmentationBaseSO, int>();
    public static string selectedAugsString = "";

    [Header("Dynamic vars, don't set here")]
    public int maxAugs;
    public int currentResearchPoints;

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

    void OnEnable()
    {
        ProfileManager.OnProfileIsSet += ResetAugManager;
    }

    void OnDisable()
    {
        ProfileManager.OnProfileIsSet -= ResetAugManager;
    }

    void Start()
    {
        ResetAugManager(ProfileManager.Instance.currentProfile);
    }

    public void Initialize()
    {
        // Only reset if profile has changed *** doesnt work
        //if (lastUsedProfile != null && lastUsedProfile.Equals(ProfileManager.Instance.currentProfile.profileName))
            //return;
        
        //Reset
        //selectedAugsWithLvls.Clear();
        //maxAugs = maxAugmentationsStart;
        //currentResearchPoints = ProfileManager.Instance.currentProfile.researchPoints;
        grenadeAmmoMult = 1;
        grenadeCostMult = 1;
        
    }

    void ResetAugManager(PlayerProfile profile)
    {
        //Debug.Log("AugManager has been reset");
        selectedAugsWithLvls.Clear();
        selectedAugsTotalCosts.Clear();
        if (profile == null)
            return;

        maxAugs = maxAugmentationsStart;
        currentResearchPoints = ProfileManager.Instance.currentProfile.researchPoints;
    }

    public void RefreshCurrentRP()
    {
        currentResearchPoints = ProfileManager.Instance.currentProfile.researchPoints;

        foreach (var kvp in selectedAugsTotalCosts)
        {
            currentResearchPoints += kvp.Value;
        }
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
            if (kvp.Key is Aug_MaxAugs)
                continue;
                
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
                //Debug.Log("Removed an aug from manager");
            }
        }
    }

    void SetSelectedAugsString()
    {
        selectedAugsString = "";

        // Build a stable, index-ordered augmentation string
        for (int i = 0; i < availableAugmentations.Count; i++)
        {
            AugmentationBaseSO aug = availableAugmentations[i];

            int lvl = 0; // default if not selected
            if (selectedAugsWithLvls.ContainsKey(aug))
                lvl = selectedAugsWithLvls[aug];

            selectedAugsString += i.ToString() + ":" + lvl.ToString();

            if (i < availableAugmentations.Count - 1)
                selectedAugsString += "|";
        }
    }
}
