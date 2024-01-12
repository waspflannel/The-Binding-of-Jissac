using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class IdleEvent : MonoBehaviour
{
    //no need for eventargs all this does is tell us when we are idle
    public event Action<IdleEvent> OnIdle;

    public void CallIdleEvent()
    {
        //if invoked do nothing otherwise invoke
        OnIdle?.Invoke(this);
    }
}
