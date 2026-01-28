using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using UnityEngine;

public class AugManager : MonoBehaviour
{
    public static AugManager Instance { get; private set; }

    public List<AugmentationBaseSO> availableAugmentations;
    public static Dictionary<AugmentationBaseSO, int> selectedAugsWithLvls = new Dictionary<AugmentationBaseSO, int>();
    public static Dictionary<AugmentationBaseSO, int> selectedAugsTotalCosts = new Dictionary<AugmentationBaseSO, int>();
    public static string selectedAugsString = "";
    public int maxAugsStart = 9;
    public int maxAugsAddedPerPlanet = 2;

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

        // Load selection state from persisted profile strings
        LoadChoicesFromProfile(profile);

        // Rebuild all derived values deterministically
        RebuildFromSelections(profile);
    }

    public void RefreshMaxAugs()
    {
        if (ProfileManager.Instance == null || ProfileManager.Instance.currentProfile == null)
            return;
            
        maxAugs = maxAugsStart + ProfileManager.Instance.currentProfile.completedPlanets.Count * maxAugsAddedPerPlanet;
    }

    public void RefreshStats()
    {
        if (ProfileManager.Instance == null || ProfileManager.Instance.currentProfile == null)
            return;

        RebuildFromSelections(ProfileManager.Instance.currentProfile);
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

    public void SaveChoicesToProfile()
    {
        if (ProfileManager.Instance == null || ProfileManager.Instance.currentProfile == null)
            return;

        PlayerProfile profile = ProfileManager.Instance.currentProfile;

        profile.selectedAugsLevelsString = BuildIndexStringFromLevels();
        profile.selectedAugsCostsString = BuildIndexStringFromCosts();

        // Keep legacy mirror for any external usage
        selectedAugsString = profile.selectedAugsLevelsString;

        ProfileManager.Instance.SaveCurrentProfile();
    }

    public void LoadChoicesFromProfile(PlayerProfile profile)
    {
        selectedAugsWithLvls.Clear();
        selectedAugsTotalCosts.Clear();

        if (profile == null)
            return;

        // Null-safe for older profiles
        if (profile.selectedAugsLevelsString == null) profile.selectedAugsLevelsString = "";
        if (profile.selectedAugsCostsString == null) profile.selectedAugsCostsString = "";

        ParseLevelsString(profile.selectedAugsLevelsString);
        ParseCostsString(profile.selectedAugsCostsString);

        selectedAugsString = profile.selectedAugsLevelsString;
    }

    public void RebuildFromSelections(PlayerProfile profile)
    {
        if (profile == null)
            return;

        // Reset baseline derived values first
        RefreshMaxAugs();
        grenadeAmmoMult = 1;
        grenadeCostMult = 1;

        // Base RP + sum of selected costs (negative means "spent")
        currentResearchPoints = profile.researchPoints;
        foreach (var kvp in selectedAugsTotalCosts)
            currentResearchPoints += kvp.Value;

        // Menu-time effects: only apply what the AugPanel needs immediately.
        // Most augmentations are applied later when gameplay starts.
        ApplyMenuTimeMaxAugsAdjustment();

        // Safety: never allow the cap to drop below already-selected non-MaxAugs levels.
        int currentSelectedLevels = GetCurrentLevelCount();
        if (maxAugs < currentSelectedLevels)
            maxAugs = currentSelectedLevels;
    }

    string BuildIndexStringFromLevels()
    {
        if (availableAugmentations == null || availableAugmentations.Count == 0)
            return "";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < availableAugmentations.Count; i++)
        {
            AugmentationBaseSO aug = availableAugmentations[i];
            int lvl = 0;
            if (aug != null && selectedAugsWithLvls.ContainsKey(aug))
                lvl = selectedAugsWithLvls[aug];

            sb.Append(i).Append(":").Append(lvl);
            if (i < availableAugmentations.Count - 1)
                sb.Append("|");
        }
        return sb.ToString();
    }

    string BuildIndexStringFromCosts()
    {
        if (availableAugmentations == null || availableAugmentations.Count == 0)
            return "";

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < availableAugmentations.Count; i++)
        {
            AugmentationBaseSO aug = availableAugmentations[i];
            int cost = 0;
            if (aug != null && selectedAugsTotalCosts.ContainsKey(aug))
                cost = selectedAugsTotalCosts[aug];

            sb.Append(i).Append(":").Append(cost);
            if (i < availableAugmentations.Count - 1)
                sb.Append("|");
        }
        return sb.ToString();
    }

    void ParseLevelsString(string levelsString)
    {
        if (string.IsNullOrEmpty(levelsString) || availableAugmentations == null)
            return;

        string[] entries = levelsString.Split('|');
        foreach (string entry in entries)
        {
            if (string.IsNullOrEmpty(entry))
                continue;

            string[] parts = entry.Split(':');
            if (parts.Length != 2)
                continue;

            if (!int.TryParse(parts[0], out int index))
                continue;
            if (!int.TryParse(parts[1], out int lvl))
                continue;
            if (lvl == 0)
                continue;

            if (index < 0 || index >= availableAugmentations.Count)
                continue;

            AugmentationBaseSO aug = availableAugmentations[index];
            if (aug == null)
                continue;

            selectedAugsWithLvls[aug] = lvl;
        }
    }

    void ParseCostsString(string costsString)
    {
        if (string.IsNullOrEmpty(costsString) || availableAugmentations == null)
            return;

        string[] entries = costsString.Split('|');
        foreach (string entry in entries)
        {
            if (string.IsNullOrEmpty(entry))
                continue;

            string[] parts = entry.Split(':');
            if (parts.Length != 2)
                continue;

            if (!int.TryParse(parts[0], out int index))
                continue;
            if (!int.TryParse(parts[1], out int cost))
                continue;
            if (cost == 0)
                continue;

            if (index < 0 || index >= availableAugmentations.Count)
                continue;

            AugmentationBaseSO aug = availableAugmentations[index];
            if (aug == null)
                continue;

            selectedAugsTotalCosts[aug] = cost;
        }
    }

    void ApplyMenuTimeMaxAugsAdjustment()
    {
        // Baseline maxAugs is set by RefreshMaxAugs().
        // Here we apply ONLY the Aug_MaxAugs selection effect for the menu.
        foreach (KeyValuePair<AugmentationBaseSO, int> kvp in selectedAugsWithLvls)
        {
            if (kvp.Key is not Aug_MaxAugs maxAugsAug)
                continue;

            int level = kvp.Value;
            if (level == 0)
                continue;

            if (level > 0)
                maxAugs += maxAugsAug.lvlAddIncrement * level;
            else
                maxAugs -= maxAugsAug.lvlNegIncrement * Mathf.Abs(level);
        }

        if (maxAugs < 0)
            maxAugs = 0;
    }
}
