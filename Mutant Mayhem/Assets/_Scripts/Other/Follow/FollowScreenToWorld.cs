using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowScreenToWorld : MonoBehaviour
{
    [SerializeField] RectTransform uiTrans;
    [SerializeField] Transform worldTrans;

    void Update()
    {
        worldTrans.position = Camera.main.ScreenToWorldPoint(uiTrans.position);
    }
}
