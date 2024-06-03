using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleStateManager : MonoBehaviour
{
    // This will hold the reference to the active state in the state machine
    // State machine can only be in one state at a time
    // This will essentially be the "Context" of the state machine

    public AppleBaseState currentState;
    public AppleGrowingState GrowingState = new AppleGrowingState();
    public AppleChewedState ChewedState = new AppleChewedState();
    public AppleRottenState RottenState = new AppleRottenState();
    public AppleWholeState WholeState = new AppleWholeState();

    void Start()
    {
        currentState = GrowingState;

        currentState.EnterState(this);
    }

    private void OnCollisionEnter2D(Collision2D collision) 
    {
        currentState.OnCollisionEnter2D(this, collision);    
    }

    
    void Update()
    {
        currentState.UpdateState(this);
    }

    public void SwitchState(AppleBaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }
}
