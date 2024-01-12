using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[DisallowMultipleComponent]
//the class acts as the publisher and the callaimweaponevent method is what triggers the events
public class AimWeaponEvent : MonoBehaviour
{

    //actual event that can be subscribed too
    public event Action<AimWeaponEvent, AimWeaponEventArgs> OnWeaponAim;

    //used to publish the information to all subscribbers,
    //when called it triggers onWeaponAim event and sends event data packed inside a
    //AimWeaponEventArgs to all subscribers
    public void CallAimWeaponEvent(AimDirection aimDirection , float aimAngle , float weaponAimAngle , Vector3 weaponAimDirectionVector)
    {
        OnWeaponAim?.Invoke(this, new AimWeaponEventArgs()
        {
            aimDirection = aimDirection,
            aimAngle = aimAngle,
            weaponAimAngle = weaponAimAngle,
            weaponAimDirectionVector = weaponAimDirectionVector
        });
    }
}

//this class is the event data
public class AimWeaponEventArgs : EventArgs
{
    public AimDirection aimDirection;
    public float aimAngle;
    public float weaponAimAngle;
    public Vector3 weaponAimDirectionVector;
}
