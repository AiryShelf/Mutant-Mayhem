using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiUpgradePanel_DroneHangar : UiUpgradePanel
{
    [Header("Drone Hangar")]
    [SerializeField] GameObject dockedLayoutObject;
    [SerializeField] GameObject launchedLayoutObject;
    [SerializeField] string droneInfoIconPoolName = "Drone_Info_Icon";
    List<DroneInfoIcon> activeDroneInfoIcons = new List<DroneInfoIcon>();

    public DroneContainer droneContainer { get; private set; }
    Coroutine updateDroneInfoCoroutine;

    public void OpenPanel(PanelInteract interactSource, DroneContainer newDroneContainer)
    {
        base.OpenPanel(interactSource);

        droneContainer = newDroneContainer;
        InitializeDroneInfoIcons();

        updateDroneInfoCoroutine = StartCoroutine(UpdateDroneInfoIcons());
    }

    void InitializeDroneInfoIcons()
    {
        for (int i = 0; i < droneContainer.maxDrones; i++)
        {
            GameObject icon = PoolManager.Instance.GetFromPool(droneInfoIconPoolName);
            icon.transform.SetParent(dockedLayoutObject.transform, false);
            DroneInfoIcon infoIconComp = icon.GetComponent<DroneInfoIcon>();
            activeDroneInfoIcons.Add(infoIconComp);
            icon.SetActive(false);
            icon.transform.localScale = Vector3.one;
        }
    }

    public override void ClosePanel()
    {
        base.ClosePanel();
        droneContainer = null;

        foreach (DroneInfoIcon icon in activeDroneInfoIcons)
        {
            PoolManager.Instance.ReturnToPool(droneInfoIconPoolName, icon.gameObject);
        }
        activeDroneInfoIcons.Clear();

        if (updateDroneInfoCoroutine != null)
            StopCoroutine(updateDroneInfoCoroutine);
    }

    IEnumerator UpdateDroneInfoIcons()
    {
        while (true)
        {
            // Deactivate before refresh
            foreach (DroneInfoIcon icon in activeDroneInfoIcons)
            {
                icon.gameObject.SetActive(false);
            }

            // Refresh icons
            foreach (Drone drone in droneContainer.controlledDrones)
            {
                foreach (DroneInfoIcon infoIcon in activeDroneInfoIcons)
                {
                    // Find an inactive icon to use
                    if (!infoIcon.gameObject.activeSelf)
                    {
                        // Activate, assign to docked or launched layout
                        infoIcon.gameObject.SetActive(true);
                        infoIcon.transform.SetParent(drone.isDocked ? dockedLayoutObject.transform : launchedLayoutObject.transform, false);
                        infoIcon.SetIconState(drone);
                        break;
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}
