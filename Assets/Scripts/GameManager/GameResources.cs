using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
        //checks if instance is null if it is load an object of type GameResources and put into instance.
    {
        get
        {
            if(instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");//anything with recources name in unity asset file manager can be accessed with Resources.Load
            }
            return instance; 
        }
    }

    #region Header DUNGEON
    [Space(10)]
    [Header("DUNGEON")]
    #endregion Header DUNGEON
    #region Tooltip
    [Tooltip("Populate with the dungeon roomNodeTypeListSO")]
    #endregion
    public roomNodeTypeListSO roomNodeTypeList;

    #region Header PLAYER
    [Space(10)]
    [Header("PLAYER")]
    #endregion Header PLAYER
    #region Tooltip
    [Tooltip("The current player SO - this is used to reference the current player between scenes")]
    #endregion
    public CurrentPlayerSO currentPlayer;

    #region Header MATERIALS
    [Space(10)]
    [Header("Materials")]
    #endregion
    #region Tooltip
    [Tooltip("Dimmed Material")]
    #endregion
    public Material dimmedMaterial;
    public Material litMaterial;
    public Shader variableLitShader;

    //this is an array because theres two tiles for it the full and the half
    public TileBase[] enemyUnwalkableCollisionTileArray;
    public TileBase preferredEnemyPathTile;

    public GameObject ammoIconPrefab;


#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValues(this, nameof(roomNodeTypeList), roomNodeTypeList);
        HelperUtilities.ValidateCheckNullValues(this, nameof(currentPlayer), currentPlayer);
        HelperUtilities.ValidateCheckNullValues(this, nameof(dimmedMaterial), dimmedMaterial);
        HelperUtilities.ValidateCheckNullValues(this, nameof(litMaterial), litMaterial);
        HelperUtilities.ValidateCheckNullValues(this, nameof(variableLitShader), variableLitShader);
        HelperUtilities.ValidateCheckNullValues(this, nameof(ammoIconPrefab), ammoIconPrefab);
        HelperUtilities.ValidateCheckNullValues(this, nameof(preferredEnemyPathTile), preferredEnemyPathTile);
        HelperUtilities.ValidateCheckEnumerateValues(this, nameof(enemyUnwalkableCollisionTileArray), enemyUnwalkableCollisionTileArray);
    }

#endif

}
