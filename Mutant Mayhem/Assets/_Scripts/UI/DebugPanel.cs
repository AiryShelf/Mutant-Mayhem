using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] GameObject debugPanel;
    [SerializeField] TextMeshProUGUI enemyCountText;
    [SerializeField] List<Transform> buttonTransforms;

    EnemySpawner enemySpawner;

    
    void Start()
    {
        enemySpawner = FindObjectOfType<EnemySpawner>();
        debugPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        enemyCountText.text = "EnemyCount: " + EnemySpawner.GLOBAL_enemyCount.ToString();

        if (Input.GetKeyDown("p"))
        {
            if (debugPanel.activeInHierarchy)
            {
                debugPanel.SetActive(false);
            }
            else
            {
                debugPanel.SetActive(true);
                Button firstButton = GetComponentInChildren<Button>();
                if (firstButton != null)
                    EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
                else
                    Debug.Log("Could not find first button on debug panel");
            }
        }
    }
    
    public void SpawnCrab1()
    {
        SpawnByIndex(0);
    }

    public void SpawnCrab2()
    {
        SpawnByIndex(1);
    }

    public void SpawnDog()
    {
        SpawnByIndex(2);
    }

    void SpawnByIndex(int i)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(buttonTransforms[i].position);
        Instantiate(enemySpawner.enemyList[i], worldPos, Quaternion.identity);
    }
}
