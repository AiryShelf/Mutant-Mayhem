using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float toolTipDelay = 0.3f;
    public string header;
    [Multiline()]
    public string content;

    Coroutine delay;
    

    void Awake()
    {
        UIStructure uIStructure = GetComponent<UIStructure>();
        if (uIStructure != null)
        {
            content = uIStructure.structureSO.description;
            header = "";
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (delay == null)
            delay = StartCoroutine(DelayedCall());
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
        yield return new WaitForSecondsRealtime(toolTipDelay);
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
