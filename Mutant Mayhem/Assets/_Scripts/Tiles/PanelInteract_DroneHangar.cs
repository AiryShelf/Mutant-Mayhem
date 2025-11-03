using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelInteract_DroneHangar : PanelInteract
{
    public DroneContainer droneContainer;

    public override void OpenPanel(Player panelOpener)
    {
        UpgradePanelManager.Instance.CloseAllPanels();
        UpgradePanelManager.Instance.OpenPanel(structureTypeForPanelInteract, this);
        playerWhoOpened = panelOpener;
        sqDistToPlayerWhoOpened = Vector3.SqrMagnitude(playerWhoOpened.transform.position - transform.position);

        StartCoroutine(CheckForClose());
        droneContainer = playerWhoOpened.stats.structureStats.currentDroneContainer;
    }
}
