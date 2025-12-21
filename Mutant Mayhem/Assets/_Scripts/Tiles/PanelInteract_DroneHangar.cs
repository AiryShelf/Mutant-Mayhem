using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelInteract_DroneHangar : PanelInteract
{
    public DroneContainer droneContainer;
    public RangeCircle rangeCircle;

    void Start()
    {
        // Get action for updating range circle when hangar range upgraded
        DroneManager.OnHangarRangeUpgraded += UpdateRangeCircle;   
    }

    void OnDestroy()
    {
        DroneManager.OnHangarRangeUpgraded -= UpdateRangeCircle;
    }

    public override void OpenPanel(Player panelOpener)
    {
        rangeCircle.EnableCircle(true);
        UpgradePanelManager.Instance.CloseAllPanels();
        UpgradePanelManager.Instance.OpenPanel(structureTypeForPanelInteract, this);
        playerWhoOpened = panelOpener;
        sqDistToPlayerWhoOpened = Vector3.SqrMagnitude(playerWhoOpened.transform.position - transform.position);

        StartCoroutine(CheckForClose());
        droneContainer = playerWhoOpened.stats.structureStats.currentDroneContainer;
    }

    public override void ClosePanel()
    {
        rangeCircle.EnableCircle(false);
        base.ClosePanel();
    }

    public void UpdateRangeCircle(float newRange)
    {
        rangeCircle.radius = newRange;
    }
}
