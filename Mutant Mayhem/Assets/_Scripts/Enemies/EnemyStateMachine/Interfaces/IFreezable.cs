using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFreezable
{
    float unfreezeTime { get; set; }
    Coroutine unfreezeAfterTime { get; set; }
    
    void StartFreeze();
    IEnumerator UnfreezeAfterTime();
}
