using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FollowCameraSetter : MonoBehaviour
{
    [SerializeField] Transform objToFollow;
    [SerializeField] CinemachineVirtualCamera followCamera;

    void Start()
    {
        followCamera.Follow = objToFollow;
    }
}
