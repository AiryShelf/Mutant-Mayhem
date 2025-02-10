using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SelectFader : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] Image imageToFade;
    [SerializeField] float selectedAlpha;
    float startAlpha;

    void Awake()
    {
        startAlpha = imageToFade.color.a;
    }

    public void OnSelect(BaseEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
