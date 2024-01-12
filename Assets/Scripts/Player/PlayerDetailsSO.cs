using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//every playable character will have one of these SO which will contain information for that character
[CreateAssetMenu(fileName = "PlayerDetails_" , menuName = "Scriptable Objects/Player/Player Details")]
public class PlayerDetailsSO : ScriptableObject
{
    #region Header PLAYER BASE DETAILS
    [Space(10)]
    [Header("PLAYER BASE DETAILS")]
    #endregion


    #region Tooltip
    [Tooltip("Player character name")]
    #endregion
    public string playerCharacterName;


    #region Tooltip
    [Tooltip("Prefab gameobjet for the player")]
    #endregion
    public GameObject playerPrefab;

    #region Tooltip
    [Tooltip("Player Runtime animator controller")]
    #endregion
    public RuntimeAnimatorController runtimeAnimatorController;

    #region Header HEALTH
    [Space(10)]
    [Header("HEALTH")]
    #endregion
    #region Tooltip
    [Tooltip("Player starting health amount")]
    #endregion
    public int playerHealthAmount;


    #region Header OTHER
    [Space(10)]
    [Header("OTHER")]
    #endregion
    #region Tooltip
    [Tooltip("Player icon sprite to be used in minimap")]
    #endregion
    public Sprite playerMinimapIcon;


    #region Tooltip
    [Tooltip("Player Hand Sprite")]
    #endregion
    public Sprite playerHandSprite;


    public WeaponDetailsSO startingWeapon;
    public List<WeaponDetailsSO> startingWeaponList;


    #region Validation
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(playerCharacterName), playerCharacterName);
        HelperUtilities.ValidateCheckNullValues(this, nameof(playerPrefab), playerPrefab);
        HelperUtilities.ValidateCheckPositiveValues(this, nameof(playerHealthAmount), playerHealthAmount , false);
        HelperUtilities.ValidateCheckNullValues(this, nameof(playerMinimapIcon), playerMinimapIcon);
        HelperUtilities.ValidateCheckNullValues(this, nameof(playerHandSprite), playerHandSprite);
        HelperUtilities.ValidateCheckNullValues(this, nameof(runtimeAnimatorController), runtimeAnimatorController);

        HelperUtilities.ValidateCheckNullValues(this, nameof(startingWeapon), startingWeapon);
        HelperUtilities.ValidateCheckEnumerateValues(this, nameof(startingWeaponList), startingWeaponList);
    }

    #endregion


}
