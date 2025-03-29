using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectorBase : MonoBehaviour
{
    public StructureSO myStructure;
    public List<StructureSO> structuresToLink;
    public List<Connector> myConnectors;
    [SerializeField] TextMeshPro powerGeneratedText;
    [SerializeField] PowerSource powerSource;

    [Header("Dyanamic vars, don't set here")]
    public int powerToAdd = 0;
    [SerializeField] List<Connector> connections;
    [SerializeField] List<Connector> uiConnections;

    void OnEnable()
    {
        UpdatePowerText();
        StartCoroutine(CheckConnectionsContinuous());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        powerGeneratedText.transform.rotation = Quaternion.identity;
    }

    IEnumerator CheckConnectionsContinuous()
    {
        while (true)
        {
            CheckConnections(true);
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    public void CheckConnections(bool checkBuildHighlight)
    {
        foreach (var connector in myConnectors)
        {
            var pos = connector.transform.position;
            bool isRealTile =  !TileManager.Instance.IsTileBlueprint(pos);
            
            var gridPos = TileManager.Instance.WorldToGrid(pos);
            var structure = TileManager.Instance.GetStructureAt(pos);
            StructureType foundType = StructureType.None;
            if (structure != null)
            {
                foundType = structure.structureType;
                foreach (var s in structuresToLink)
                {
                    if (foundType == s.structureType)
                        AddConnection(connector, isRealTile);
                }
            }
            else if (checkBuildHighlight)
            {
                if (BuildingSystem.Instance.highlightPositions.Contains(gridPos))
                {
                    foundType = BuildingSystem.Instance.structureInHand.structureType;
                    foreach (var s in structuresToLink)
                    {
                        if (foundType == s.structureType)
                            AddConnection(connector, false);
                    }
                }
            }

            if (foundType == StructureType.None)
                RemoveConnection(connector);
        }
    }

    public void AddConnection(Connector connector, bool isRealConnection)
    {
        connector.MakeConnection();

        if (!isRealConnection)
        {
            AddUiConnection(connector);
            return;
        }
        else
            AddUiConnection(connector);

        if (connections.Contains(connector)) return;
        
        connections.Add(connector);
        UpdatePowerText();

        if (powerSource != null)
            powerSource.AddNeighborBonus();
    }

    public void AddUiConnection(Connector connector)
    {
        if (uiConnections.Contains(connector)) return;

        uiConnections.Add(connector);
        UpdatePowerText();
    }

    public void RemoveConnection(Connector connector)
    {
        RemoveUiConnection(connector);
        connector.BreakConnection();

        if (!connections.Contains(connector)) return;
        connections.Remove(connector);
        UpdatePowerText();

        if (powerSource != null)
            powerSource.RemoveNeighborBonus();
    }

    void RemoveUiConnection(Connector connector)
    {
        if (uiConnections.Contains(connector))
            uiConnections.Remove(connector);

        UpdatePowerText();
    }

    public void UpdatePowerText()
    {
        powerToAdd = uiConnections.Count * myStructure.powerNeighborBonus;

        powerGeneratedText.text = "<sprite=1>" + (myStructure.powerGenerated + powerToAdd).ToString();
    }
}
