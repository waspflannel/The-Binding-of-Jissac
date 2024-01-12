using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public string id;
    public string templateID;
    public GameObject prefab;
    public roomNodeTypeSO roomNodeType;

    //these are the coords as the room is placed
    //these wont be the same as the template l/u 
    //these will relate to grid coords as the room is placed
    public Vector2Int lowerBounds;
    public Vector2Int upperBounds;

    public Vector2Int templateLowerBounds;
    public Vector2Int templateUpperBounds;

    public Vector2Int[] spawnPositionArray;
    public List<string> childRoomIDList;
    public string parentRoomID;
    public List<Doorway> doorWayList;
    public bool isPositioned = false;
    

    public InstantiatedRoom instantiatedRoom;

    public bool isClearedOfenemies = false;
    public bool isPreviouslyVisited = false;
    public bool isLit;
    public Room()//constuctor
    {
        childRoomIDList = new List<string>();
        doorWayList = new List<Doorway>();
    }
}
