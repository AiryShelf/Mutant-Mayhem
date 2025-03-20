using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeObjectActivator : MonoBehaviour
{
    [SerializeField] GameObject objectToToggle;

    void Start()
    {
        BuildingSystem.Instance.OnBuildMenuOpen += BuildMenuOpen;
        BuildMenuOpen(BuildingSystem.Instance.isInBuildMode);
    }

    void OnDestroy()
    {
        BuildingSystem.Instance.OnBuildMenuOpen -= BuildMenuOpen;
    }

    void BuildMenuOpen(bool open)
    {
        objectToToggle.SetActive(open);
    }
}
