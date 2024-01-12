using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName= "roomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class roomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;//name of the room ( boss room , corridor, etc...)

    #region Header
    [Header("Only flag the RoomNodeTypes that should be visible in the editor")]
    #endregion Header
    public bool displayInNodeGraphEditor = true;
    #region Header
    [Header("One Type should be a corridor")]
    #endregion Header
    public bool isCorridor;
    #region Header
    [Header("One type should be a CorridorNS ")]
    #endregion Header
    public bool isCorridorNS;
    #region Header
    [Header("One Type Should be a CorridorEW")]
    #endregion Header
    public bool isCorridorEW;
    #region Header
    [Header("One type should be an entrance")]
    #endregion Header
    public bool isEntrance;
    #region Header
    [Header("one type should be a boss Room")]
    #endregion Header
    public bool isBossRoom;
    #region Header
    [Header("one type should be none (Unassigned)")]
    #endregion Header
    public bool isNone;

    #region Validation
    private void OnValidate()//used to check changes in inspector
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
    #endregion


}
