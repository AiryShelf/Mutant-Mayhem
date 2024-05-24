using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleGrowingState : AppleBaseState
{
    Vector3 startingAppleSize = new Vector3(0.1f, 0.1f, 0.1f);
    Vector3 growAppleScalar = new Vector3(0.1f, 0.1f, 0.1f);

    public override void EnterState(AppleStateManager apple)
    {
        Debug.Log("Hellow from entering the growing state");
        apple.transform.localScale = startingAppleSize;
    }

    public override void UpdateState(AppleStateManager apple)
    {
        // Debug.Log("I get ran every frame in GrowingState");
        if (apple.transform.localScale.x < 1)
        {
            apple.transform.localScale += growAppleScalar * Time.deltaTime;
        }
        else
        {
            apple.SwitchState(apple.WholeState);
        }
    }

    public override void OnCollisionEnter2D(AppleStateManager apple, Collision2D collision)
    {
        Debug.Log("Collision detected in GrowingState");
    }
}
