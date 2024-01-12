using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFireable 
{
    //anyting that implements IFireable must have these two methods
    void InitialiseAmmo(AmmoDetailsSO ammoDetails , float aimAngle , float weaponAimAngle ,
        float ammoSpeed , Vector3 weaponAimDirectionVector , bool overrideAmmoMovement = false);

    GameObject GetGameObject();
}
