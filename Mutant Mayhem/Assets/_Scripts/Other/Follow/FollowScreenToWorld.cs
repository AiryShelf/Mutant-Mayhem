using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowScreenToWorld : MonoBehaviour
{
    public bool useUiTrans = true;
    RectTransform startingUiTrans;
    public RectTransform uiTrans;

    public Transform worldTrans;

    void Awake()
    {
        startingUiTrans = uiTrans;
    }

    void Update()
    {
        if (useUiTrans || worldTrans == null)
            transform.position = Camera.main.ScreenToWorldPoint(uiTrans.position);
        else 
            transform.position = worldTrans.position;
    }

    public void ResetUiTransToStart()
    {
        useUiTrans = true;
        uiTrans = startingUiTrans;
    }
}
