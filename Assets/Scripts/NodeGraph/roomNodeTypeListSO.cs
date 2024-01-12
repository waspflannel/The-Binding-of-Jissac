using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="roomNodeTypeListSO", menuName= "Scriptable Objects/Dungeon/Room Node Type List")]
public class roomNodeTypeListSO : ScriptableObject
{
    #region Header ROOM NODE TYPE LIST
    [Space(10)]
    [Header("ROOM NODE TYPE LIST")]
    #endregion
    #region Tooltip
    [Tooltip("This list should be populated with all the roomNodeTypeSO for the game")]
    #endregion

    public List<roomNodeTypeSO> list;

    #region Validation
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerateValues(this, nameof(list), list);

    }
    #endregion
}
