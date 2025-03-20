using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectorBase : MonoBehaviour
{
    public StructureSO myStructure;
    public List<StructureSO> structuresToLink;
    [SerializeField] List<Connector> myConnectors;
    [SerializeField] List<ConnectorReceiver> myReceivers;
    [SerializeField] List<Connector> connections;
    public int powerToAdd = 0;
    [SerializeField] TextMeshPro powerGeneratedText;
    [SerializeField] List<Vector2> connectorDirections;

    void OnEnable()
    {
        BuildingSystem.Instance.OnBuildMenuOpen += BuildMenuOpen;
        BuildMenuOpen(BuildingSystem.Instance.isInBuildMode);
        UpdatePowerText();  
    }

    void OnDisable()
    {
        if (BuildingSystem.Instance != null)
            BuildingSystem.Instance.OnBuildMenuOpen -= BuildMenuOpen;
    }

    void Update()
    {
        powerGeneratedText.transform.rotation = Quaternion.identity;
    }

    void BuildMenuOpen(bool open)
    {
        if (powerGeneratedText != null)
            powerGeneratedText.enabled = open;

        foreach (var connector in myConnectors)
        {
            if (connector != null)
            {
                connector.gameObject.SetActive(open);
                if (!open)
                    connector.Disable();
            }
        }
        foreach (var receiver in myReceivers)
        {
            if (receiver != null)
                receiver.gameObject.SetActive(open);
        }
    }

    public void AddConnection(Connector connector)
    {
        if (connectorDirections.Contains(connector.myDir) || connections.Contains(connector)) return;

        Debug.Log("ConnectorBase: Removed Connection");
        connections.Add(connector);
        connectorDirections.Add(connector.myDir);
        UpdatePowerText();

        SetConnectorsOn(connector, false);
    }

    public void RemoveConnection(Connector connector, bool enabled)
    {
        Debug.Log("ConnectorBase: Removed Connection");

        connections.Remove(connector);
        if (connectorDirections.Contains(connector.myDir))
            connectorDirections.Remove(connector.myDir);

        UpdatePowerText();
        SetConnectorsOn(connector, enabled);
    }

    void SetConnectorsOn(Connector connector, bool on)
    {
        foreach (var link in myConnectors)
        {
            if (link == connector) continue;

            if (link.myDir == connector.myDir)
                link.gameObject.SetActive(on);
        }
    }

    public void UpdatePowerText()
    {
        List<Vector2> dirs = new List<Vector2>();
        List<Connector> connectorsToRemove = new List<Connector>();
        powerToAdd = 0;
        foreach (var c in connections)
        {
            if (dirs.Contains(c.myDir))
            {
                connectorsToRemove.Add(c);
                continue;
            }
            powerToAdd += myStructure.powerNeighborBonus;
            dirs.Add(c.myDir);
        }
        foreach (var c in connectorsToRemove)
            connections.Remove(c);

        powerGeneratedText.text = "<sprite=1>" + (myStructure.powerGenerated + powerToAdd).ToString();
    }
}
