using UnityEngine;

[CreateAssetMenu(fileName ="WeaponDetails_",menuName ="Scriptable Objects/Weapons/Weapon Details")]
public class WeaponDetailsSO : ScriptableObject
{
    public string weaponName;
    public Sprite weaponSprite;//sprites must have generate physics shape option ticked


    public Vector3 weaponShootPosition;//where to init the ammo
    public AmmoDetailsSO weaponCurrentAmmo;
    public bool hasInfiniteAmmo = false;
    public bool hasInfiniteClipCapacity = false;//for enemies
    public int weaponClipAmmoCapacity = 6;//number of ammo in a clip
    public int weaponAmmoCapacity = 100;//max rounds that can be held
    public float weaponFireRate = 0.2f;
    public float weaponPrechargeTime = 0f;//time in seconds to hold down before firing
    public float weaponReloadTime = 0f;


#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(weaponName), weaponName);
        HelperUtilities.ValidateCheckNullValues(this , nameof(weaponCurrentAmmo) , weaponCurrentAmmo);
        HelperUtilities.ValidateCheckPositiveValues(this, nameof(weaponFireRate), weaponFireRate, false);
        HelperUtilities.ValidateCheckPositiveValues(this, nameof(weaponPrechargeTime), weaponPrechargeTime, true);
        HelperUtilities.ValidateCheckPositiveValues(this, nameof(weaponReloadTime), weaponReloadTime, true);
        if (!hasInfiniteAmmo)
        {
            HelperUtilities.ValidateCheckPositiveValues(this, nameof(weaponAmmoCapacity), weaponAmmoCapacity, false);
        }
        if (!hasInfiniteClipCapacity)
        {
            HelperUtilities.ValidateCheckPositiveValues(this, nameof(weaponClipAmmoCapacity), weaponClipAmmoCapacity, false);
        }
    }
#endif
}
