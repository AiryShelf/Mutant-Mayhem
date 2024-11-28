using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Firewall : MonoBehaviour
{
    [SerializeField] float moveSpeed;

    void FixedUpdate()
    {
        transform.position += transform.right * moveSpeed;
        transform.position = new Vector2 (transform.position.x, Camera.main.transform.position.y);
    }
}
