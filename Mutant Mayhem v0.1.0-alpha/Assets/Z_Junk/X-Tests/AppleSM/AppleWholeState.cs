using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleWholeState : AppleBaseState
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void EnterState(AppleStateManager apple)
    {
        Debug.Log("Hello from whole state");
    }

    public override void UpdateState(AppleStateManager apple)
    {
        
    }

    public override void OnCollisionEnter2D(AppleStateManager apple, Collision2D collision)
    {
        
    }
}
