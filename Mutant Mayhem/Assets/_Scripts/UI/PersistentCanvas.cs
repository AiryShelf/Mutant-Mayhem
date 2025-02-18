using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersistentCanvas : MonoBehaviour
{
    public static PersistentCanvas Instance;
    public Image customCursor;

    private void Awake()
    {
        // Only one persistent canvas
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            
            Destroy(gameObject);
            return;
        }
    }
}
