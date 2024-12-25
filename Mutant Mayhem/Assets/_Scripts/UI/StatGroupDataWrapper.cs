using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class StatGroupWrapper
{
    public VerticalLayoutGroup nameGroup { get; private set; }
    public VerticalLayoutGroup valueGroup { get; private set; }
    GameObject namePrefab;
    GameObject valuePrefab;

    public StatGroupWrapper(VerticalLayoutGroup nameGroup, VerticalLayoutGroup valueGroup, 
                            GameObject namePrefab, GameObject valuePrefab)
    {
        this.nameGroup = nameGroup;
        this.valueGroup = valueGroup;
        this.namePrefab = namePrefab;
        this.valuePrefab = valuePrefab;
    }

    public void AddTitle(string name, GameObject titlePrefab)
    {
        if (titlePrefab == null)
        {
            Debug.LogError("StatGroupDataWrapper: Tried to add title entry with no titlePrefab");
            return;
        }

        GameObject statObj = GameObject.Instantiate(titlePrefab, nameGroup.transform);
        statObj.GetComponentInChildren<TextMeshProUGUI>().text = name;

        GameObject valueObj = GameObject.Instantiate(valuePrefab, valueGroup.transform);
        valueObj.GetComponentInChildren<TextMeshProUGUI>().text = "";
    }

    public void AddStat(string name, string value)
    {
        if (namePrefab == null || valuePrefab == null)
        {
            Debug.LogError("StatGroupDataWrapper: Tried to add entry with no name/value prefab");
            return;
        }

        GameObject statObj = GameObject.Instantiate(namePrefab, nameGroup.transform);
        statObj.GetComponentInChildren<TextMeshProUGUI>().text = name;

        GameObject valueObj = GameObject.Instantiate(valuePrefab, valueGroup.transform);
        TextMeshProUGUI tmp = valueObj.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = value;
    }

    public void AddStat(string name, string value, Color valueColor)
    {
        if (namePrefab == null || valuePrefab == null)
        {
            Debug.LogError("StatGroupDataWrapper: Tried to add entry with no name/value prefab");
            return;
        }

        GameObject statObj = GameObject.Instantiate(namePrefab, nameGroup.transform);
        statObj.GetComponentInChildren<TextMeshProUGUI>().text = name;

        GameObject valueObj = GameObject.Instantiate(valuePrefab, valueGroup.transform);
        TextMeshProUGUI tmp = valueObj.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = value;
        tmp.color = valueColor;
    }

    public void RefreshLayout(GameObject objectToRefresh)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(objectToRefresh.GetComponent<RectTransform>());
    }
}