using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Color connectedColor;

    Color startColor;
    public bool isConnected = false;

    void Start()
    {
        startColor = sr.color;
    }

    public void MakeConnection()
    {
        //Debug.Log("Attempting to make connection");
        isConnected = true;
        sr.color = connectedColor;
    }

    public void BreakConnection()
    {
        sr.color = startColor;
        isConnected = false;
    }

    public void Disable()
    {
        sr.color = startColor;
        isConnected = false;
        gameObject.SetActive(false);
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
