using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]
public class FireWeapon : MonoBehaviour
{
    private float fireRateCoolDownTimer = 0f;
    private float firePrechargeTimer = 0f;
    private WeaponFiredEvent weaponFiredEvent;
    private FireWeaponEvent fireWeaponEvent;
    private ReloadWeaponEvent reloadWeaponEvent;
    private ActiveWeapon activeWeapon;
    private void Awake()
    {
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        activeWeapon = GetComponent<ActiveWeapon>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
    }
    private void OnEnable()
    {
        fireWeaponEvent.OnWeaponFire += WeaponFiredEvent_OnWeaponFire;

    }
    private void OnDisable()
    {
        fireWeaponEvent.OnWeaponFire -= WeaponFiredEvent_OnWeaponFire;
    }

    private void Update()
    {
        //Debug.Log(fireRateCoolDownTimer);
        //fireRateCoolDownTImer is set to zero so when decremented it goes negative
        //which does not violate the restriction in the method CanWeaponFire
        //after firing it gets reset
        fireRateCoolDownTimer -= Time.deltaTime;
    }

    private void WeaponFiredEvent_OnWeaponFire(FireWeaponEvent fireWeaponEvent , FireWeaponEventArgs fireWeaponArgs)
    {
        WeaponFire(fireWeaponArgs);
    }

    private void WeaponFire(FireWeaponEventArgs fireWeaponArgs)
    {
        WeaponPreCharge(fireWeaponArgs);
        if (fireWeaponArgs.fire)
        {
            if (IsWeaponReadyToFire())
            {
                FireAmmo(fireWeaponArgs.aimAngle, fireWeaponArgs.weaponAimAngle, fireWeaponArgs.weaponAimDirectionVector);
                ResetCoolDownTimer();
                ResetPreChargeTimer();
            }
        }
    }

    private void WeaponPreCharge(FireWeaponEventArgs fireWeaponEventArgs)
    {
        //since when we first hold it down it it will be false
        //it will reset it to the charge timer and then start decrementing.
        if (fireWeaponEventArgs.firePreviousFrame)
        {
            firePrechargeTimer -= Time.deltaTime;
        }
        else
        {
            ResetPreChargeTimer();
        }
    }

    private bool IsWeaponReadyToFire()
    {
        //if infinite clip capacity isnt set in the weapon details and the clip is empty
        //if we dont have an infinite clip and we run out of ammo
        if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity &&
            activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo <= 0)
        {
            //if we dont have infinite clip capacity and we run out of ammo
            reloadWeaponEvent.CallOnReloadWeaponEvent(activeWeapon.GetCurrentWeapon(), 0);
            return false;
        }
        //if the weapon is reloading
        if (activeWeapon.GetCurrentWeapon().isWeaponReloading)
        {
            return false;
        }
        //if the weapon is cooling down or not precharged
        if(fireRateCoolDownTimer > 0f || firePrechargeTimer >0f)
        {
            return false;
        }
 
        //if the weapon doesnt have any ammo <=0 and it doesnt have infinite ammo set in its details
        //if we are out of ammo and we dont have infinite ammo
        if (activeWeapon.GetCurrentWeapon().weaponRemainingAmmo <= 0 && !activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteAmmo)
        {

            return false;
        }
        return true;
    }

    private void FireAmmo(float aimAngle , float weaponAimAngle , Vector3 weaponAimDirectionVector)
    {
        AmmoDetailsSO currentAmmo = activeWeapon.GetCurrentAmmo();

        if(currentAmmo != null)
        {
            //get ammo prefab
            GameObject ammoPrefab = currentAmmo.ammoPrefabArray[Random.Range(0, currentAmmo.ammoPrefabArray.Length)];
            //get random speed value
            float ammoSpeed = Random.Range(currentAmmo.ammoSpeedMin, currentAmmo.ammoSpeedMax);
            //get gameobject from pool , send in the prefab , position to shoot from and rotation
            IFireable ammo = (IFireable)PoolManager.Instance.ReuseComponenet(ammoPrefab, activeWeapon.GetShootPosition(), Quaternion.identity);
            //init ammo
            ammo.InitialiseAmmo(currentAmmo, aimAngle, weaponAimAngle, ammoSpeed, weaponAimDirectionVector);

            //reduce ammo clip cound if not infinite clip capacity;
            if (!activeWeapon.GetCurrentWeapon().weaponDetails.hasInfiniteClipCapacity)
            {
                activeWeapon.GetCurrentWeapon().weaponClipRemainingAmmo--;
                activeWeapon.GetCurrentWeapon().weaponRemainingAmmo--;
            }
            //call weapon fired Event
            weaponFiredEvent.CallOnWeaponFired(activeWeapon.GetCurrentWeapon());

        }
    }

    private void ResetCoolDownTimer()
    {
        fireRateCoolDownTimer = activeWeapon.GetCurrentWeapon().weaponDetails.weaponFireRate;
    }

    private void ResetPreChargeTimer()
    {
        firePrechargeTimer = activeWeapon.GetCurrentWeapon().weaponDetails.weaponPrechargeTime;
    }

}
