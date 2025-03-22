using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectorBase : MonoBehaviour
{
    public StructureSO myStructure;
    public List<StructureSO> structuresToLink;
    [SerializeField] List<Connector> myConnectors;
    [SerializeField] TextMeshPro powerGeneratedText;
    [SerializeField] PowerSource powerSource;

    [Header("Dyanamic vars, don't set here")]
    public int powerToAdd = 0;
    [SerializeField] List<Connector> connections; // Serialized for debug

    void OnEnable()
    {
        UpdatePowerText();
        StartCoroutine(CheckConnections());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        powerGeneratedText.transform.rotation = Quaternion.identity;
    }

    IEnumerator CheckConnections()
    {
        while (true)
        {
            foreach (var connector in myConnectors)
            {
                List<Vector3Int> otherPositions;
                var pos = connector.transform.position;
                var gridPos = TileManager.Instance.WorldToGrid(pos);
                var structure = TileManager.Instance.GetStructureAt(pos);
                StructureType foundType = StructureType.None;
                if (structure != null)
                {
                    foundType = structure.structureType;
                    otherPositions = TileManager.Instance.GetStructurePositions(TileManager.AnimatedTilemap, gridPos);
                }
                else
                {
                    if (BuildingSystem.Instance.highlightPositions.Contains(gridPos))
                        foundType = BuildingSystem.Instance.structureInHand.structureType;
                }

                if (foundType == StructureType.None)
                    RemoveConnection(connector);
                else
                {
                    foreach (var s in structuresToLink)
                    {
                        if (foundType == s.structureType)
                            AddConnection(connector);
                    }
                }
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    public void AddConnection(Connector connector)
    {
        if (connections.Contains(connector)) return;

        //Debug.Log("ConnectorBase: Added Connection");
        connections.Add(connector);
        connector.MakeConnection();
        UpdatePowerText();

        if (powerSource != null)
            powerSource.SetNeighborBonus(powerToAdd);
    }

    public void RemoveConnection(Connector connector)
    {
        if (!connections.Contains(connector)) return;

        //Debug.Log("ConnectorBase: Removed Connection");
        connections.Remove(connector);
        connector.BreakConnection();
        UpdatePowerText();
    }

    public void UpdatePowerText()
    {
        powerToAdd = 0;
        foreach (var c in connections)
            powerToAdd += myStructure.powerNeighborBonus;

        powerGeneratedText.text = "<sprite=1>" + (myStructure.powerGenerated + powerToAdd).ToString();
    }
}
