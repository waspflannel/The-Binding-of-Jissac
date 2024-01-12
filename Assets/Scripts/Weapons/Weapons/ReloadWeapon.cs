using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponReloadedEvent))]
[RequireComponent(typeof(SetActiveWeaponEvent))]
[DisallowMultipleComponent]
public class ReloadWeapon : MonoBehaviour
{

    private WeaponReloadedEvent weaponReloadedEvent;

    private ReloadWeaponEvent reloadWeaponEvent;
    private SetActiveWeaponEvent setActiveWeaponEvent;

    private Coroutine reloadWeaponCoroutine;
    private void Awake()
    {
        weaponReloadedEvent = GetComponent<WeaponReloadedEvent>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
    }
    private void OnEnable()
    {
        reloadWeaponEvent.OnReloadWeapon += ReloadWeaponEvent_OnReloadWeapon;
        setActiveWeaponEvent.OnSetActiveWeapon += SetActiveWeaponEvent_OnSetActiveWeapon;
    }
    private void OnDisable()
    {

        reloadWeaponEvent.OnReloadWeapon -= ReloadWeaponEvent_OnReloadWeapon;
        setActiveWeaponEvent.OnSetActiveWeapon -= SetActiveWeaponEvent_OnSetActiveWeapon;
    }

    private void ReloadWeaponEvent_OnReloadWeapon(ReloadWeaponEvent reloadWeaponEvent, ReloadWeaponEventArgs reloadweaponEventArgs)
    {
       
        StartReloadWeapon(reloadweaponEventArgs);
    }
    private void StartReloadWeapon(ReloadWeaponEventArgs reloadweaponEventArgs)
    {
        //if theres a couroutine going on stop it
        if (reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
        }
        reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(reloadweaponEventArgs.weapon, reloadweaponEventArgs.topUpAmmoPercent));
    }

    private IEnumerator ReloadWeaponRoutine(Weapon weapon, int topUpAmmoPercent)
    {
        weapon.isWeaponReloading = true;
        //while timer is less than reload timer set in weapon details
        while (weapon.weaponReloadTimer < weapon.weaponDetails.weaponReloadTime)
        {
            //increment it
            weapon.weaponReloadTimer += Time.deltaTime;
            yield return null;
        }
        //its gonna wait for the whole while statment to finish before doing this remaining part
        if (topUpAmmoPercent != 0)
        {
            //aget ammo to increase
            int ammoIncrease = Mathf.RoundToInt((weapon.weaponDetails.weaponAmmoCapacity * topUpAmmoPercent) / 100f);
            //get the total ammo
            int totalAmmo = weapon.weaponRemainingAmmo + ammoIncrease;

            //if the total ammo is more than what we can hold
            if (totalAmmo > weapon.weaponDetails.weaponAmmoCapacity)
            {
                //set it to the weapon details capacity
                weapon.weaponRemainingAmmo = weapon.weaponDetails.weaponAmmoCapacity;
            }
            else
            {
                //otherwise add the total ammo
                weapon.weaponRemainingAmmo = totalAmmo;
            }
        }
        //if the weapon has infinite ammo just refil clip
        if (weapon.weaponDetails.hasInfiniteAmmo)
        {
            weapon.weaponClipRemainingAmmo = weapon.weaponDetails.weaponClipAmmoCapacity;
        }
        //if not infinite ammo then if remaning ammo is more than required to fill the clip , then fully refil clip
        else if (weapon.weaponRemainingAmmo >= weapon.weaponDetails.weaponClipAmmoCapacity)
        {
            weapon.weaponClipRemainingAmmo = weapon.weaponDetails.weaponClipAmmoCapacity;
        }
        //else set the clip to remaining ammo;
        else
        {
            weapon.weaponClipRemainingAmmo = weapon.weaponRemainingAmmo;
        }
        weapon.weaponReloadTimer = 0f;
        weapon.isWeaponReloading = false;
        weaponReloadedEvent.CallOnWeaponReloadedEvent(weapon);
    }

    //if we switch the active weapon while the previous weapon is reloading
    private void SetActiveWeaponEvent_OnSetActiveWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        if (setActiveWeaponEventArgs.weapon.isWeaponReloading)
        {
            //stop the reload couroutine
            if(reloadWeaponCoroutine != null)
            {
                StopCoroutine(reloadWeaponCoroutine);
            }
            reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(setActiveWeaponEventArgs.weapon, 0));
        }

    }

}
