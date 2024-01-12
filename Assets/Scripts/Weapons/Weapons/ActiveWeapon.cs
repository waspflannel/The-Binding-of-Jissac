
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SetActiveWeaponEvent))]
[DisallowMultipleComponent]
public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] private PolygonCollider2D weaponPolygonCollider2D;
    [SerializeField] private Transform weaponShootPositionTransform;
    [SerializeField] private Transform weaponEffectPositionTransform;

    private SetActiveWeaponEvent setWeaponEvent;
    private Weapon currentWeapon;

    private void Awake()
    {
        setWeaponEvent = GetComponent<SetActiveWeaponEvent>();

    }
    private void OnEnable()
    {
        setWeaponEvent.OnSetActiveWeapon += SetWeaponEvent_OnSetActiveEvent;
    }
    private void OnDisable()
    {
        setWeaponEvent.OnSetActiveWeapon -= SetWeaponEvent_OnSetActiveEvent;
    }

    //if a SetActiveWeaponEvent is published
    private void SetWeaponEvent_OnSetActiveEvent(SetActiveWeaponEvent setActiveWeaponEvent , SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        //set the new weapon
        SetWeapon(setActiveWeaponEventArgs.weapon);
    }

    //when a new weapon is set , update the polygoncollider , spriterenderer and weaponShootPosition for
    //the new weapon.
    private void SetWeapon(Weapon weapon)
    {
        //set currentweapon to new weapon
        currentWeapon = weapon;
        weaponSpriteRenderer.sprite = currentWeapon.weaponDetails.weaponSprite;
        if(weaponPolygonCollider2D !=null && weaponSpriteRenderer.sprite != null)
        {
            List<Vector2> spritePhysicsShapePointList = new List<Vector2>();//make a list to hold the points
            weaponSpriteRenderer.sprite.GetPhysicsShape(0, spritePhysicsShapePointList);//get the physics shape
            weaponPolygonCollider2D.points = spritePhysicsShapePointList.ToArray();//update the polygon collider for new weapon
        }
        weaponShootPositionTransform.localPosition = currentWeapon.weaponDetails.weaponShootPosition;

    }


    //getters for active weapon info
    public AmmoDetailsSO GetCurrentAmmo()
    {
        return currentWeapon.weaponDetails.weaponCurrentAmmo;
    }
    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    public Vector3 GetShootPosition()
    {
        return weaponShootPositionTransform.position;
    }
    public Vector3 GetShootEffectPosition()
    {
        return weaponEffectPositionTransform.position;
    }
    public void RemoveCurrentWeapon()
    {
        currentWeapon = null;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValues(this, nameof(weaponSpriteRenderer), weaponSpriteRenderer);
        HelperUtilities.ValidateCheckNullValues(this, nameof(weaponPolygonCollider2D), weaponPolygonCollider2D);
        HelperUtilities.ValidateCheckNullValues(this, nameof(weaponShootPositionTransform), weaponShootPositionTransform);
        HelperUtilities.ValidateCheckNullValues(this, nameof(weaponEffectPositionTransform), weaponEffectPositionTransform);
    }

#endif
}
