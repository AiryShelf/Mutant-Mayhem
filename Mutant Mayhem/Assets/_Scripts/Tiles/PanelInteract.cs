using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelInteract : MonoBehaviour
{
    UiUpgradePanel uiUpgradePanel;
    Transform openerTransform;

    public void OpenPanel(Transform panelOpener)
    {
        uiUpgradePanel.gameObject.SetActive(true);
        openerTransform = panelOpener;

        StartCoroutine(CheckForClose());
    }

    public void ClosePanel()
    {
        uiUpgradePanel.gameObject.SetActive(false);
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
            yield return null;
        }
    }
}
