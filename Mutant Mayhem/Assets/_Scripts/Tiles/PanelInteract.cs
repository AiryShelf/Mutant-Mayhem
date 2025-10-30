using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelInteract : MonoBehaviour
{
    [SerializeField] StructureType structureTypeForPanelInteract;
    Player playerWhoOpened;
    float sqDistToPlayerWhoOpened = 0;

    public void OpenPanel(Player panelOpener)
    {
        UpgradePanelManager.Instance.CloseAllPanels();
        UpgradePanelManager.Instance.OpenPanel(structureTypeForPanelInteract, this);
        playerWhoOpened = panelOpener;
        sqDistToPlayerWhoOpened = Vector3.SqrMagnitude(playerWhoOpened.transform.position - transform.position);

        StartCoroutine(CheckForClose());
    }

    public void ClosePanel()
    {
        if (playerWhoOpened != null)
        {
            playerWhoOpened.ExitInteractMode();
            playerWhoOpened = null;
        }
        UpgradePanelManager.Instance.ClosePanel(structureTypeForPanelInteract);
        StopAllCoroutines();
    }

    IEnumerator CheckForClose()
    {
        while (true)
        {
            if (playerWhoOpened == null || Vector3.SqrMagnitude(playerWhoOpened.transform.position - transform.position) > sqDistToPlayerWhoOpened + 2f)
            {
                ClosePanel();
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
