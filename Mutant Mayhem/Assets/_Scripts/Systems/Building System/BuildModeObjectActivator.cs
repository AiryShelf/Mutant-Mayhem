using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeObjectActivator : MonoBehaviour
{
    [SerializeField] GameObject objectToToggle;

    [Header("Optional:")]
    [SerializeField] List<StructureType> typesToMatch = new List<StructureType>();

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
        if (open)
        {
            if (typesToMatch.Count > 0)
                StartCoroutine(CheckForMatch());
            else
                objectToToggle.SetActive(true);
        }
        else
        {
            StopAllCoroutines();
            objectToToggle.SetActive(false);
        }
    }

    IEnumerator CheckForMatch()
    {
        while (true)
        {
            objectToToggle.SetActive(typesToMatch.Contains(BuildingSystem.Instance.structureInHand.structureType));
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
