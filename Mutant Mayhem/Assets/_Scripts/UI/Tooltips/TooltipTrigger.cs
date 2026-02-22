using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string header;
    [Multiline()]
    public string content;

    Coroutine delay;
    
    void OnDisable()
    {
        TooltipSystem.Hide();
        if (delay != null)
        {
            StopCoroutine(delay);
            delay = null;
        }
    }

    void Start()
    {
        
        UIUpgradeButton uIUpgrade = GetComponent<UIUpgradeButton>();
        UIStructure uIStructure = GetComponent<UIStructure>();

        // UIUpgrade descriptions
        if (uIUpgrade != null)
        {
            content = uIUpgrade.tooltipDescription;
            header = "";
        }

        // Use UIStructure description if is UIStructure
        else if (uIStructure != null)
        {
            content = uIStructure.structureSO.description;
            header = "";
        }
        else
        {
            // Uses 'content' input from inspector
        }
        
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!TooltipSystem.fadedOut)
        {
            TooltipSystem.Show(content, header);
            return;
        }
        if (delay == null)
        {
            delay = StartCoroutine(DelayedCall());
        }
        else
        {
            StopCoroutine(delay);
            delay = StartCoroutine(DelayedCall());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
        if (delay != null)
        {
            StopCoroutine(delay);
            delay = null;
        }       
    }

    IEnumerator DelayedCall()
    {
        yield return new WaitForSecondsRealtime(TooltipSystem.Instance.tooltipDelay);
        TooltipSystem.Show(content, header);
    }

    public void OnMouseEnter()
    {
        if (delay == null)
            delay = StartCoroutine(DelayedCall());
    }

    public void OnMouseExit()
    {
        TooltipSystem.Hide();
        if (delay != null)
        {
            StopCoroutine(delay);
            delay = null;
        } 
    }
}
