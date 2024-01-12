using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//subscriber to aim weapon event
[RequireComponent(typeof(AimWeaponEvent))]
[DisallowMultipleComponent]
public class AimWeapon : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the transform from the child weaponrotationpoint gameobject")]
    #endregion
    [SerializeField] private Transform weaponRotationPointTransform;

    private AimWeaponEvent aimWeaponEvent;

    private void Awake()
    {
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
    }

    private void OnEnable()
    {
        //subscribe to aim weapon event
        aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;
    }
    private void OnDisable()
    {
        //unsubscribe from aim weapon event
        aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim; 
    }

    //aim weapon event handler
    //called when subscribing/unsubscribing
    private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent , AimWeaponEventArgs aimWeaponEventArgs)
    {
        Aim(aimWeaponEventArgs.aimDirection, aimWeaponEventArgs.aimAngle);
    }

    //this flips the players weapon depending on direction
    private void Aim(AimDirection aimDirection , float aimAngle)
    {
        //set angle of the weapon transform\
        //this animates the player weapon to spin around depending on aimangle
        weaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

        //flip weapon transform based on player direction
        switch (aimDirection)
        {
            case AimDirection.Left:
            case AimDirection.UpLeft:
                weaponRotationPointTransform.localScale = new Vector3(1f, -1f, 0f);
                break;

            case AimDirection.Up:
            case AimDirection.UpRight:
            case AimDirection.Right:
            case AimDirection.Down:
                weaponRotationPointTransform.localScale = new Vector3(1f, 1f, 0f);
                break;


        }
    }

    #region Validation
    private void onValidate()
    {
        //make sure weaponRotationPointTransform is filled in the editor
        HelperUtilities.ValidateCheckNullValues(this, nameof(weaponRotationPointTransform), weaponRotationPointTransform);
    }
    #endregion Validation

}
