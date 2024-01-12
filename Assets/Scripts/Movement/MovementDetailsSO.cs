using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName= "MovementDetails_", menuName = "Scriptable Objects/Movement/MovementDetails")]
public class MovementDetailsSO : ScriptableObject
{
    #region Header MOVEMENT DETAILS
    [Space(10)]
    [Header("MOVEMENT DETAILS")]
    #endregion Header
    #region Tooltip
    [Tooltip("The minimum Move Speed. The GetMoveSpeed method calculates a random value between the min and max")]
    #endregion Tooltip
    public float minMoveSpeed = 8f;
    #region Tooltip
    [Tooltip("The Maximum Move Speed. The GetMoveSpeed method calculates a random value between the min and max")]
    #endregion Tooltip
    public float maxMoveSpeed = 8f;

    #region Tooltip
    [Tooltip("if rolling , roll at this speed")]
    #endregion Tooltip
    public float rollSpeed;
    #region Tooltip
    [Tooltip("if rolling , this is roll distance")]
    #endregion Tooltip
    public float rollDistance;
    #region Tooltip
    [Tooltip("if rolling , this is the roll cooldown")]
    #endregion Tooltip
    public float rollCooldownTime;


    //get a random moveSpeed between the min and and max values

    public float GetMoveSpeed()
    {
        if(minMoveSpeed == maxMoveSpeed)
        {
            return minMoveSpeed;
        }
        else
        {
            return Random.Range(minMoveSpeed, maxMoveSpeed);
        }
    }

    #region Validation
    private void OnValidate()
    {
        //make sure the min max move speed range is propa
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(minMoveSpeed), minMoveSpeed, nameof(maxMoveSpeed), maxMoveSpeed, false);

        //make sure all roll variables are positive
        if(rollDistance !=0f || rollCooldownTime != 0f || rollSpeed != 0f)
        {
            HelperUtilities.ValidateCheckPositiveValues(this, nameof(rollDistance), rollDistance, false);
            HelperUtilities.ValidateCheckPositiveValues(this, nameof(rollCooldownTime), rollCooldownTime, false);
            HelperUtilities.ValidateCheckPositiveValues(this, nameof(rollSpeed), rollSpeed, false);
        }
    }
    #endregion Validation

}
