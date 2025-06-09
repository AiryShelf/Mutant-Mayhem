using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QualityObjectActivator : MonoBehaviour
{
    [SerializeField] GameObject objectToActivate; 
    [SerializeField] List<QualityLevel> levelsToActivate; // Deactivates otherwise
    
    void OnEnable()
    {
        QualityManager.OnQualitySettingsChanged += CheckIfActivate;
    }

    void OnDisable()
    {
        QualityManager.OnQualitySettingsChanged -= CheckIfActivate;
    }
    
    void CheckIfActivate(QualityLevel qualityLevel)
    {
        if (levelsToActivate.Contains(qualityLevel))
            objectToActivate.SetActive(true);
        else
            objectToActivate.SetActive(false);
    }
}
