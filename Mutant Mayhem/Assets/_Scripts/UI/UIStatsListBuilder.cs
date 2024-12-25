using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_DeathStatsListBuilder : MonoBehaviour
{
    [SerializeField] WaveControllerRandom waveController;
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
    const string Padding = "\u00A0";
    const string Paddingx5 = "\u00A0 \u00A0\u00A0\u00A0\u00A0";

    public void BuildListAndText()
    {
        // Set progress texts
        string time = GameTools.FormatTimeFromSeconds(Mathf.FloorToInt(StatsCounterPlayer.TotalPlayTime));
        survivedForText.text = "You survived for " + time;
        int nightsSurvived = waveController.currentWaveIndex;
        nightReachedText.text = "Nights Survived: " + nightsSurvived;
        int previousRecord = ProfileManager.Instance.currentProfile.maxWaveSurvived;
        previousRecordText.text = "Previous Record: " + previousRecord;
        int researchPointsGained = deathManager.GetResearchPointsGain();
        researchPointsGainedText.text = "Research Points Gained: " + researchPointsGained;
        int totalResearchPoints = ProfileManager.Instance.currentProfile.researchPoints;
        totalResearchPointsText.text = "Total Research Points: " + totalResearchPoints;

        ClearAllStatGroups();
        StatsCounterPlayer.PopulateStatsDict();

        CreateStatGroup("MISC" + Paddingx5 + Padding + Padding, StatsCounterPlayer.GetMiscStats(), horizontalLayoutGroup);
        CreateStatGroup("PROJECTILES", StatsCounterPlayer.GetProjectilesStats(), horizontalLayoutGroup);
        CreateStatGroup("MELEE" + Paddingx5 + Padding, StatsCounterPlayer.GetMeleeStats(), horizontalLayoutGroup);
        CreateStatGroup("DAMAGE" + Paddingx5, StatsCounterPlayer.GetDamageStats(), horizontalLayoutGroup2);
        CreateStatGroup("STRUCTURES" + Padding, StatsCounterPlayer.GetStructuresStats(), horizontalLayoutGroup2);
    }

    void CreateStatGroup(string groupName, Dictionary<string, float> stats, HorizontalLayoutGroup layoutGroup)
    {
        // Create layouts
        GameObject newGroupObj = Instantiate(nameGroupPrefab, layoutGroup.transform);
        VerticalLayoutGroup newNameGroup = newGroupObj.GetComponent<VerticalLayoutGroup>();
        newGroupObj = Instantiate(valueGroupPrefab, layoutGroup.transform);
        VerticalLayoutGroup newValueGroup = newGroupObj.GetComponent<VerticalLayoutGroup>();

        // Create group, add title entry
        StatGroupWrapper statGroup = new StatGroupWrapper(newNameGroup, newValueGroup, groupTitleTextPrefab, nameTextPrefab, valueTextPrefab);
        statGroup.AddTitle(groupName);

        foreach (var kvp in stats)
        {
            statGroup.AddStat(kvp.Key, GameTools.ConvertToStatValue(kvp.Value));
        }
    }

    public void ClearAllStatGroups()
    {
        foreach (GameObject obj in horizontalLayoutGroup.transform)
        {
            Destroy(obj);
        }

        foreach (GameObject obj in horizontalLayoutGroup2.transform)
        {
            Destroy(obj);
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