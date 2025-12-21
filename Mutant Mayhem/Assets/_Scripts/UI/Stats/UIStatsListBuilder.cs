using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_DeathStatsListBuilder : MonoBehaviour
{
    [SerializeField] WaveController waveController;
    [SerializeField] DeathManager deathManager;

    [Header("Stats Info")]
    [SerializeField] TextMeshProUGUI survivedForText;
    [SerializeField] TextMeshProUGUI nightReachedText;
    [SerializeField] TextMeshProUGUI previousRecordText;
    [SerializeField] TextMeshProUGUI researchPointsGainedText;
    [SerializeField] TextMeshProUGUI totalResearchPointsText;

    [Header("Stats Panel")]
    [SerializeField] HorizontalLayoutGroup horizontalLayoutGroup;
    [SerializeField] HorizontalLayoutGroup horizontalLayoutGroup2;
    [SerializeField] GameObject nameGroupPrefab;
    [SerializeField] GameObject valueGroupPrefab;
    [SerializeField] GameObject groupTitleTextPrefab;
    [SerializeField] GameObject nameTextPrefab;
    [SerializeField] GameObject valueTextPrefab;

    public void BuildListAndText()
    {
        // Set progress texts
        string time = GameTools.FormatTimeFromSeconds(Mathf.FloorToInt(StatsCounterPlayer.TotalPlayTime));
        survivedForText.text = "You survived for " + time;
        int nightsSurvived = Mathf.Clamp(waveController.currentWaveIndex, 0, int.MaxValue);
        nightReachedText.text = "Nights Survived: " + nightsSurvived;
        // Use profile helpers to get previous record for this planet
        var profile = ProfileManager.Instance.currentProfile;
        if (profile == null)
        {
            Debug.LogError("UI_DeathStatsListBuilder: currentProfile is null, cannot read planet max index.");
            previousRecordText.text = "Previous Record: -";
        }
        else
        {
            string planetKey = PlanetManager.Instance.currentPlanet.bodyName;
            int previousIndex = profile.GetPlanetMaxIndex(planetKey);
            previousRecordText.text = "Previous Record: " + previousIndex;
        }
        int researchPointsGained = WaveController.Instance.GetResearchPointsTotal(nightsSurvived);
        researchPointsGainedText.text = "Research Points Gained: " + researchPointsGained;
        int totalResearchPoints = ProfileManager.Instance.currentProfile.researchPoints;
        totalResearchPointsText.text = "Total Research Points: " + totalResearchPoints;

        ClearAllStatGroups();
        StatsCounterPlayer.PopulateStatsDict();

        CreateStatGroup("PROJECTILES", StatsCounterPlayer.GetProjectilesStats(), horizontalLayoutGroup);
        CreateStatGroup("MELEE", StatsCounterPlayer.GetMeleeStats(), horizontalLayoutGroup);
        CreateStatGroup("MISC", StatsCounterPlayer.GetMiscStats(), horizontalLayoutGroup);
        CreateStatGroup("DAMAGE", StatsCounterPlayer.GetDamageStats(), horizontalLayoutGroup2);
        CreateStatGroup("STRUCTURES", StatsCounterPlayer.GetStructuresStats(), horizontalLayoutGroup2);
    }

    void CreateStatGroup(string groupName, Dictionary<string, float> stats, HorizontalLayoutGroup layoutGroup)
    {
        // Create layouts
        GameObject newGroupObj = Instantiate(nameGroupPrefab, layoutGroup.transform);
        VerticalLayoutGroup newNameGroup = newGroupObj.GetComponent<VerticalLayoutGroup>();
        newGroupObj = Instantiate(valueGroupPrefab, layoutGroup.transform);
        VerticalLayoutGroup newValueGroup = newGroupObj.GetComponent<VerticalLayoutGroup>();

        // Create group, add entries
        StatGroupWrapper statGroup = new StatGroupWrapper(newNameGroup, newValueGroup, nameTextPrefab, valueTextPrefab);
        statGroup.AddTitle(groupName, groupTitleTextPrefab);

        foreach (var kvp in stats)
        {
            statGroup.AddStat(kvp.Key, GameTools.ConvertToStatValue(kvp.Value));
        }
    }

    public void ClearAllStatGroups()
    {
        foreach (Transform transform in horizontalLayoutGroup.transform)
        {
            Destroy(transform.gameObject);
        }

        foreach (Transform trans in horizontalLayoutGroup2.transform)
        {
            Destroy(trans.gameObject);
        }
    }
}








/*
public class UI_DeathStatsListBuilder : MonoBehaviour
{
    [SerializeField] FadeCanvasGroupsWave myFadeCanvasGroups;
    [SerializeField] TextMeshProUGUI survivedForText;
    [SerializeField] TextMeshProUGUI nightReachedText;
    [SerializeField] TextMeshProUGUI previourRecordText;
    [SerializeField] TextMeshProUGUI researchPointsGainedText;
    [SerializeField] TextMeshProUGUI totalResearchPointsText;

    [Header("Stats Panel")]
    [SerializeField] VerticalLayoutGroup miscGroup;
    [SerializeField] VerticalLayoutGroup projectilesGroup;
    [SerializeField] VerticalLayoutGroup meleeGroup;
    [SerializeField] VerticalLayoutGroup damageGroup;
    [SerializeField] VerticalLayoutGroup structuresGroup;
    [SerializeField] GridLayoutGroup statNamesGrid;
    [SerializeField] GridLayoutGroup statValuesGrid;
    [SerializeField] GameObject statsNameTextPrefab;
    [SerializeField] GameObject statsValueTextPrefab;
    
    public void BuildListandText(WaveControllerRandom waveController, DeathManager deathManager)
    {
        // Set progress texts
        string time = GameTools.FormatTimeFromSeconds(Mathf.FloorToInt(StatsCounterPlayer.TotalPlayTime));
        survivedForText.text = "You survived for " + time;
        int nightsSurvived = waveController.currentWaveIndex;
        nightReachedText.text = "Nights Survived: " + nightsSurvived;
        int previousRecord = ProfileManager.Instance.currentProfile.maxWaveSurvived;
        previourRecordText.text = "Previous Record: " + previousRecord;
        int researchPointsGained = deathManager.GetResearchPointsGain();
        researchPointsGainedText.text = "Research Points Gained: " + researchPointsGained;
        int totalResearchPoints = ProfileManager.Instance.currentProfile.researchPoints;
        totalResearchPointsText.text = "Total Research Points: " + totalResearchPoints;

        ClearStatLists();
        PopulateStatLists();
        
        // I'd like to create multiple dictionaries for categories of stats

        // Create names and values in gridLayoutGroups        
        foreach (KeyValuePair<string,float> kvp in StatsCounterPlayer.StatsDict)
        {
            GameObject name = Instantiate(statsNameTextPrefab, statNamesGrid.transform);
            name.GetComponent<TextMeshProUGUI>().text = kvp.Key;

            GameObject value = Instantiate(statsValueTextPrefab, statValuesGrid.transform);
            value.GetComponent<TextMeshProUGUI>().text = kvp.Value.ToString("#0");

            myFadeCanvasGroups.individualElements.Add(name.GetComponent<CanvasGroup>());
            myFadeCanvasGroups.individualElements.Add(value.GetComponent<CanvasGroup>());
        }
        myFadeCanvasGroups.InitializeToFadedOut();
        
        //fadeCanvasGroups.isTriggered = true;
    }

    void ClearStatLists()
    {

        // Clear objects in stats layout groups
        for (int i = statNamesGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(statNamesGrid.transform.GetChild(i).gameObject);
        }

        for (int i = statValuesGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(statValuesGrid.transform.GetChild(i).gameObject);
        }
    }

    void PopulateStatLists()
    {
        

        StatsCounterPlayer.PopulateStatsDict();
    }
 
}
*/