using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    public ConnectorBase myBase;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Color connectedColor;
    public Vector2 myDir; // To allow only one connection per direction

    Color startColor;
    public bool isConnected = false;
    ConnectorReceiver connectedConnector;

    void OnDisable()
    {
        connectedConnector = null;
        isConnected = false;      
    }

    void Start()
    {
        startColor = sr.color;
    }

    void Update()
    {
    }

    void LateUpdate()
    {
        if (connectedConnector == null || !connectedConnector.gameObject.activeInHierarchy)
        {
            BreakConnection();
            return;
        }

        // Get the collider's bounds
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider == null) return;

        Bounds bounds = myCollider.bounds;

        // Check for colliding Connectors within these bounds
        Collider2D[] foundColliders = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0, LayerMask.GetMask("BuildUI")); 

        bool stillConnected = false;
        foreach (var col in foundColliders)
        {
            if (col.gameObject == connectedConnector.gameObject)
                stillConnected = true;
        }

        if (!stillConnected)
        {
            BreakConnection();
            // Check for existing connector
            Collider2D foundCollider = Physics2D.OverlapBox(bounds.center, bounds.size, 0, LayerMask.GetMask("BuildUI:"));
            if (foundCollider != null)
            {
                connectedConnector = foundCollider.gameObject.GetComponent<ConnectorReceiver>();
                if (connectedConnector != null)
                {
                    if (myBase.structuresToLink.Contains(connectedConnector.myStructure))
                        MakeConnection();
                }
            }
        }
    }

    public void MakeConnection()
    {
        Debug.Log("Attempting to make connection");
        isConnected = true;
        sr.color = connectedColor;
        myBase.AddConnection(this);
    }

    public void BreakConnection()
    {
        if (!isConnected) return;

        sr.color = startColor;
        myBase.RemoveConnection(this, true);
        isConnected = false;
        connectedConnector = null;
    }

    public void Disable()
    {
        sr.color = startColor;
        myBase.RemoveConnection(this, false);
        isConnected = false;
        connectedConnector = null;
        gameObject.SetActive(false);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (connectedConnector != null) return;

        connectedConnector = collision.gameObject.GetComponent<ConnectorReceiver>();
        if (connectedConnector != null)
        {
            if (myBase.structuresToLink.Contains(connectedConnector.myStructure))
                MakeConnection();
        }
    }

    
    public void OnTriggerExit2D(Collider2D collision)
    {
        var exitingConnector = collision.gameObject.GetComponent<ConnectorReceiver>();
        if (exitingConnector != null && exitingConnector == connectedConnector)
            BreakConnection();
    }
    
}
