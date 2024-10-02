using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStatsListBuilder : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI survivedForText;
    [SerializeField] TextMeshProUGUI nightReachedText;
    [SerializeField] TextMeshProUGUI previourRecordText;
    [SerializeField] TextMeshProUGUI researchPointsGainedText;
    [SerializeField] TextMeshProUGUI totalResearchPointsText;
    [SerializeField] GridLayoutGroup statNamesGrid;
    [SerializeField] GridLayoutGroup statValuesGrid;
    [SerializeField] GameObject statsNameTextPrefab;
    [SerializeField] GameObject statsValueTextPrefab;
    [SerializeField] FadeCanvasGroupsWave myFadeCanvasGroups;
    
    public void BuildListandText(WaveControllerRandom waveController, DeathManager deathManager)
    {
        // Set progress texts
        string time = GameTools.FormatTimeFromSeconds(Mathf.FloorToInt(StatsCounterPlayer.TotalPlayTime));
        survivedForText.text = "You survived for " + time;
        int nightReached = waveController.currentWaveIndex + 1;
        nightReachedText.text = "Night Reached: " + nightReached;
        int previousRecord = ProfileManager.Instance.currentProfile.maxWaveReached;
        previourRecordText.text = "Previous Record: " + previousRecord;
        int researchPointsGained = deathManager.GetResearchPointsGain();
        researchPointsGainedText.text = "Research Points Gained: " + researchPointsGained;
        int totalResearchPoints = ProfileManager.Instance.currentProfile.researchPoints + researchPointsGained;
        totalResearchPointsText.text = "Total Research Points: " + totalResearchPoints;

        // Clear objects in stats layout groups
        for (int i = statNamesGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(statNamesGrid.transform.GetChild(i).gameObject);
        }

        for (int i = statValuesGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(statValuesGrid.transform.GetChild(i).gameObject);
        }

        StatsCounterPlayer.PopulateStatsDict();
        
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
 
}
