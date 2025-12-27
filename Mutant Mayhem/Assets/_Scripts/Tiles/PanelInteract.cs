using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelInteract : MonoBehaviour
{
    public StructureType structureTypeForPanelInteract;
    public InteractHighlighter interactHighlighter;
    protected Player playerWhoOpened;
    protected float sqDistToPlayerWhoOpened = 0;

    public virtual void OpenPanel(Player panelOpener)
    {
        UpgradePanelManager.Instance.CloseAllPanels();
        UpgradePanelManager.Instance.OpenPanel(structureTypeForPanelInteract, this);
        playerWhoOpened = panelOpener;
        sqDistToPlayerWhoOpened = Vector3.SqrMagnitude(playerWhoOpened.transform.position - transform.position);

        StartCoroutine(CheckForClose());
        
        TouchManager.Instance.ShowLeftSideAttackButtons(false);
    }

    public virtual void ClosePanel()
    {
        if (playerWhoOpened != null)
        {
            playerWhoOpened.ExitInteractMode();
            playerWhoOpened = null;
        }
        UpgradePanelManager.Instance.ClosePanel(structureTypeForPanelInteract);
        StopAllCoroutines();

        TouchManager.Instance.ShowLeftSideAttackButtons(true);
    }

    protected IEnumerator CheckForClose()
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
