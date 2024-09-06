using UnityEngine;

public abstract class AppleBaseState
{
    // This will be the "Abstract State"

    // abstract mean it needs to be defined in classes that derive from this class
    public abstract void EnterState(AppleStateManager apple);

    public abstract void UpdateState(AppleStateManager apple);

    public abstract void OnCollisionEnter2D(AppleStateManager apple, Collision2D collision);


}
