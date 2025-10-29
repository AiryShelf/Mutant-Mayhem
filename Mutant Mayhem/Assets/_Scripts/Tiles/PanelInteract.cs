using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelInteract : MonoBehaviour
{
    [SerializeField] StructureType structureTypeForPanelInteract;
    Transform openerTransform;

    public void OpenPanel(Transform panelOpener)
    {
        UpgradePanelManager.Instance.CloseAllPanels();
        UpgradePanelManager.Instance.OpenPanel(structureTypeForPanelInteract, this);
        openerTransform = panelOpener;

        StartCoroutine(CheckForClose());
    }

    public void ClosePanel()
    {
        UpgradePanelManager.Instance.ClosePanel(structureTypeForPanelInteract);
        StopAllCoroutines();
    }

    IEnumerator CheckForClose()
    {
        while (true)
        {
            if (Vector3.SqrMagnitude(openerTransform.position - transform.position) > 1f)
            {
                ClosePanel();
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
