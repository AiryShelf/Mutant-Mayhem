using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsListBuilder : MonoBehaviour
{
    [SerializeField] GridLayoutGroup namesGrid;
    [SerializeField] GridLayoutGroup valuesGrid;
    [SerializeField] GameObject statsNameTextPrefab;
    [SerializeField] GameObject statsValueTextPrefab;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroups;
    
    public void RebuildList()
    {
        // Clear objects in layout groups
        for (int i = namesGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(namesGrid.transform.GetChild(i).gameObject);
        }

        for (int i = valuesGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(valuesGrid.transform.GetChild(i).gameObject);
        }

        StatsCounterPlayer.RebuildStatsDict();
        // The dict rebuild might not be necessary as I think the values are referenced properly
        // Also, I'd like to create multiple dictionaries for catagories of stats to be displayed
        // in proper formats

        // Create names and values in gridLayoutGroups        
        foreach (KeyValuePair<string,float> kvp in StatsCounterPlayer.StatsDict)
        {
            GameObject name = Instantiate(statsNameTextPrefab, namesGrid.transform);
            name.GetComponent<TextMeshProUGUI>().text = kvp.Key;

            GameObject value = Instantiate(statsValueTextPrefab, valuesGrid.transform);
            value.GetComponent<TextMeshProUGUI>().text = kvp.Value.ToString("#0.00");

            fadeCanvasGroups.individualElements.Add(name.GetComponent<CanvasGroup>());
            fadeCanvasGroups.individualElements.Add(value.GetComponent<CanvasGroup>());
        }
        
        //fadeCanvasGroups.isTriggered = true;
    }
 
}
