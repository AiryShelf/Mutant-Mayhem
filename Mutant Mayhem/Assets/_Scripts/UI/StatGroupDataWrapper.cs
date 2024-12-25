using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class StatGroupWrapper
{
    public VerticalLayoutGroup nameGroup { get; private set; }
    public VerticalLayoutGroup valueGroup { get; private set; }
    GameObject titlePrefab;
    GameObject namePrefab;
    GameObject valuePrefab;

    public StatGroupWrapper(VerticalLayoutGroup nameGroup, VerticalLayoutGroup valueGroup, GameObject titlePrefab, GameObject namePrefab, GameObject valuePrefab)
    {
        this.nameGroup = nameGroup;
        this.valueGroup = valueGroup;
        this.titlePrefab = titlePrefab;
        this.namePrefab = namePrefab;
        this.valuePrefab = valuePrefab;
    }

    public void AddTitle(string name)
    {
        GameObject statObj = GameObject.Instantiate(titlePrefab, nameGroup.transform);
        statObj.GetComponentInChildren<TextMeshProUGUI>().text = name;

        GameObject valueObj = GameObject.Instantiate(valuePrefab, valueGroup.transform);
        valueObj.GetComponentInChildren<TextMeshProUGUI>().text = "";
    }

    public void AddStat(string name, string value)
    {
        GameObject statObj = GameObject.Instantiate(namePrefab, nameGroup.transform);
        statObj.GetComponentInChildren<TextMeshProUGUI>().text = name;

        GameObject valueObj = GameObject.Instantiate(valuePrefab, valueGroup.transform);
        valueObj.GetComponentInChildren<TextMeshProUGUI>().text = value;
    }
}