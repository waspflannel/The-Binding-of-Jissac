using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{

    #region UNITS
    public const float pixelsPerUnit = 16f;
    public const float tileSizePixels = 16f;
    #endregion

    #region DUNGEON BUILD SETTINGS

    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion

    #region ROOM SETTINGS

    public const int maxChildCorridors = 3;//max number of child corridors leading from a room max should be 3
    public const float fadeIntime = 0.5f;//how long it takes to fade in the room when approached;
    #endregion

    #region ANIMATOR PARAMETERS
    //animator parameters - playert
    //the stringtohash function creates integers and its faster than comparing strings
    public static int aimUp = Animator.StringToHash("aimUp");
    public static int aimDown = Animator.StringToHash("aimDown");
    public static int aimUpRight = Animator.StringToHash("aimUpRight");
    public static int aimUpLeft = Animator.StringToHash("aimUpLeft");
    public static int aimRight = Animator.StringToHash("aimRight");
    public static int aimLeft = Animator.StringToHash("aimLeft");
    public static int isIdle = Animator.StringToHash("isIdle");
    public static int isMoving = Animator.StringToHash("isMoving");

    public static int rollUp = Animator.StringToHash("rollUp");
    public static int rollRight = Animator.StringToHash("rollRight");
    public static int rollLeft = Animator.StringToHash("rollLeft");
    public static int rollDown = Animator.StringToHash("rollDown");

    //for opening doors
    public static int open = Animator.StringToHash("open");

    #endregion

    #region GAMEOBJECT TAGS
    //used to compare the player tag and player weapon tag
    public const string playerTag = "Player";
    public const string playerWeapon = "playerWeapon";
    #endregion


    //if target distance is < this then the aim angle will be used(calulcated from player)
    //else the weapon aim angle will be used(calculated from the weapon shoot position)
    public const float useAimAngleDistance = 3.5f;


    public const float uiAmmoIconSpacing = 4f;
}
