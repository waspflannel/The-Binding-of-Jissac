using UnityEngine;

[CreateAssetMenu(fileName = "AmmoDetails_", menuName = "Scriptable Objects/Weapons/Ammo Details")]
public class AmmoDetailsSO : ScriptableObject
{


    public string ammoName;
    public bool isPlayerAmmo;//to distinguish player vs enemy ammo
    public Sprite ammoSprite;
    //populate with prefab for ammo , if multiple are specified then a random will be selected
    //it can be an ammo pattern it just needs to use IFireable
    public GameObject[] ammoPrefabArray;
    public Material ammoMaterial;

    public float ammoChargeTime = 0.1f;
    public Material ammoChargeMaterial;

    public int ammoDamage = 1;
    //if speeds are same the ammo speed will be consistent , otherwise it will be
    //a random number in the range of the two
    public float ammoSpeedMin = 20f;
    public float ammoSpeedMax = 20f;
    public float ammoRange = 20f;
    //used for ammo patterns;
    public float ammoRotationSpeed = 1f;
    public float ammoSpreadMin = 0f;
    public float ammoSpreadMax = 0f;
    //min and max ammo that are spawned per shot
    public int ammoSpawnAmountMin = 1;
    public int ammoSpawnAmountMax = 1;
    //a time intevel between spawned ammo
    public float ammoSpawnIntervalMin = 0f;
    public float ammoSpawnIntervalMax = 0f;

    public bool isAmmoTrail = false;
    public float ammoTrailTime = 3f;
    public Material ammoTrailMaterial;
    [Range(0f, 1f)] public float ammoTrailStartWidth;
    [Range(0f, 1f)] public float ammoTrailEndWidth;


#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(ammoName), ammoName);
        HelperUtilities.ValidateCheckNullValues(this, nameof(ammoSprite), ammoSprite);
        HelperUtilities.ValidateCheckEnumerateValues(this, nameof(ammoPrefabArray), ammoPrefabArray);
        HelperUtilities.ValidateCheckNullValues(this, nameof(ammoMaterial), ammoMaterial);

        if(ammoChargeTime > 0)
        {
            HelperUtilities.ValidateCheckNullValues(this, nameof(ammoChargeMaterial), ammoChargeMaterial);
        }
        HelperUtilities.ValidateCheckPositiveValues(this, nameof(ammoDamage), ammoDamage , false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpeedMin), ammoSpeedMin, nameof(ammoSpeedMax), ammoSpeedMax, false);
        HelperUtilities.ValidateCheckPositiveValues(this, nameof(ammoRange), ammoRange ,false);
        HelperUtilities.ValidateCheckPositiveValues(this, nameof(ammoRotationSpeed), ammoRotationSpeed, true);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpreadMin), ammoSpreadMin, nameof(ammoSpreadMax), ammoSpreadMax, true);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpawnAmountMin), ammoSpawnAmountMin, nameof(ammoSpawnAmountMax), ammoSpawnAmountMax, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpawnIntervalMin), ammoSpawnIntervalMin, nameof(ammoSpawnIntervalMax), ammoSpawnIntervalMax, true);

        if (isAmmoTrail)
        {
            HelperUtilities.ValidateCheckPositiveValues(this, nameof(ammoTrailTime), ammoTrailTime, false);
            HelperUtilities.ValidateCheckNullValues(this, nameof(ammoTrailMaterial), ammoTrailMaterial);
            HelperUtilities.ValidateCheckPositiveValues(this, nameof(ammoTrailStartWidth), ammoTrailStartWidth, false);
            HelperUtilities.ValidateCheckPositiveValues(this, nameof(ammoTrailEndWidth), ammoTrailEndWidth, false);
        }
    }

#endif
}
