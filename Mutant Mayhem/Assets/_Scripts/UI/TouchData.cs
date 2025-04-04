using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TouchPurpose
{
    None,
    Joystick,
    Look,
    Shoot,
    Melee,
    UI,
    BuildMenu,
    UpgradePanel,
}

public class TouchData
{
    public int fingerId;
    public TouchPurpose purpose;
    public Vector2 startPosition;
    public Vector2 currentPosition;
    public Vector2 lastScrollCheckPos;

    public TouchData(int fingerId, TouchPurpose purpose, Vector2 startPos)
    {
        this.fingerId = fingerId;
        this.purpose = purpose;
        this.startPosition = startPos;
        this.currentPosition = startPos;
        lastScrollCheckPos = startPos;
    }
}
