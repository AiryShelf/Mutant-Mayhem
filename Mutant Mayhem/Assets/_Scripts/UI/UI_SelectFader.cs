using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SelectFader : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] Image imageToFade;
    [SerializeField] Color selectedColor;
    Color startColor;

    void Awake()
    {
        startColor = imageToFade.color;
    }

    public void OnSelect(BaseEventData eventData)
    {
        imageToFade.color = selectedColor;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        imageToFade.color = startColor;
    }
}
