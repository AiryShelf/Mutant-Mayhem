using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectorOLD : MonoBehaviour
{
    /*
    public ConnectorBase myBase;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Color connectedColor;
    public Vector2 myDir; // To allow only one connection per direction

    Color startColor;
    public bool isConnected = false;
    ConnectorReceiver connectedReceiver;
    Collider2D myCollider;
    Bounds bounds;

    void OnDisable()
    {
        Debug.Log("Connector disabled");
        connectedReceiver = null;
        isConnected = false;      
    }

    void Start()
    {
        myCollider = GetComponent<Collider2D>();

        bounds = myCollider.bounds;
        startColor = sr.color;
    }

    void LateUpdate()
    {
        if (connectedReceiver == null || !connectedReceiver.gameObject.activeInHierarchy)
        {
            BreakConnection();
            return;
        }

        // Get the collider's bounds
        

        // Check for colliding Connectors within these bounds
        Collider2D[] foundColliders = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0, LayerMask.GetMask("BuildUI")); 

        bool stillConnected = false;
        foreach (var col in foundColliders)
        {
            Debug.Log("Found collider: " + col.gameObject);
            if (col.gameObject == connectedReceiver.gameObject)
                stillConnected = true;
        }

        if (!stillConnected)
        {
            BreakConnection();
            // Check for existing connector
            Collider2D foundCollider = Physics2D.OverlapBox(bounds.center, bounds.size, 0, LayerMask.GetMask("BuildUI"));
            if (foundCollider != null)
            {
                connectedReceiver = foundCollider.gameObject.GetComponent<ConnectorReceiver>();
                if (connectedReceiver != null)
                {
                    if (myBase.structuresToLink.Contains(connectedReceiver.myStructure))
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
        //myBase.AddConnection(this); ****
    }

    public void BreakConnection()
    {
        sr.color = startColor;

        if (!isConnected) return;
        
        //myBase.RemoveConnection(this, true); ****
        isConnected = false;
        connectedReceiver = null;
    }

    public void Disable()
    {
        sr.color = startColor;
        //myBase.RemoveConnection(this, false); ****
        isConnected = false;
        connectedReceiver = null;
        gameObject.SetActive(false);
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (connectedReceiver != null) return;

        Debug.Log("OnTiggerStay ran, with no connected receiver");
        connectedReceiver = collision.gameObject.GetComponent<ConnectorReceiver>();
        if (connectedReceiver != null)
        {
            Debug.Log("OnTiggerStay found a ConnectorReceiver");
            if (myBase.structuresToLink.Contains(connectedReceiver.myStructure))
                MakeConnection();
        }
    }

    /*
    public void OnTriggerExit2D(Collider2D collision)
    {
        var exitingConnector = collision.gameObject.GetComponent<ConnectorReceiver>();
        if (exitingConnector != null && exitingConnector == connectedConnector)
            BreakConnection();
    }
    */
}
