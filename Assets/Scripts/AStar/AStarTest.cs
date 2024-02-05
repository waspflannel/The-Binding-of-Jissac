using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarTest : MonoBehaviour
{
    private InstantiatedRoom instantiatedRoom;
    private Grid grid;
    private Tilemap frontTilemap;
    private Tilemap pathTilemap;
    private Vector3Int startGridPosition;
    private Vector3Int endGridPosition;
    private TileBase startPathTile;
    private TileBase endPathTile;

    private Vector3Int noValue = new Vector3Int(9999, 9999, 9999);
    private Stack<Vector3> pathStack;

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }
    private void onDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }
    private void Start()
    {
        startPathTile = GameResources.Instance.preferredEnemyPathTile;
        endPathTile = GameResources.Instance.enemyUnwalkableCollisionTileArray[0];
    }

    //gets called when entering a new room.
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
       
        pathStack = null;
        instantiatedRoom = roomChangedEventArgs.room.instantiatedRoom;
        frontTilemap = instantiatedRoom.transform.Find("Grid/Tilemap4_Front").GetComponent<Tilemap>();
        grid = instantiatedRoom.transform.GetComponentInChildren<Grid>();
        startGridPosition = noValue;
        endGridPosition = noValue;

        SetUpPathTilemap();

    }

    //use a clone of the front tilemap for the path.
    private void SetUpPathTilemap()
    {
        //get the cloned tilemap if it exists
        Transform tilemapCloneTransform = instantiatedRoom.transform.Find("Grid/Tilemap4_Front(Clone)");

        //if it doesnt
        if(tilemapCloneTransform == null)
        {
            //instantiate a a pathTileMap. change its sorting order
            //change it material and tag
            pathTilemap = Instantiate(frontTilemap, grid.transform);
            pathTilemap.GetComponent<TilemapRenderer>().sortingOrder = 4;
            pathTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
            pathTilemap.gameObject.tag = "Untagged";
        }
        else
        {
            //if it exists then use it and clear the tiles on it
            pathTilemap = instantiatedRoom.transform.Find("Grid/Tilemap4_Front(Clone)").GetComponent<Tilemap>();
            pathTilemap.ClearAllTiles();
        }

    }

    private void Update()
    {
        if(instantiatedRoom == null || startPathTile == null || endPathTile == null || grid == null || pathTilemap == null){
            return;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("setting start");
            ClearPath();
            SetStartPosition();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("setting end");
            ClearPath();
            SetEndPosition();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            DisplayPath();
        }
    }

    private void SetStartPosition()
    {
        if(startGridPosition == noValue)
        {
            
            startGridPosition = grid.WorldToCell(HelperUtilities.GetMouseWorldPosition());

            if (!IsPositionWithinBounds(startGridPosition))
            {
                startGridPosition = noValue;
                return;
            }
            pathTilemap.SetTile(startGridPosition, startPathTile);
        }
        else
        {
            pathTilemap.SetTile(startGridPosition, null);
            startGridPosition = noValue;
        }
    }
    private void SetEndPosition()
    {
        if (endGridPosition == noValue)
        {

            endGridPosition = grid.WorldToCell(HelperUtilities.GetMouseWorldPosition());

            if (!IsPositionWithinBounds(endGridPosition))
            {
                endGridPosition = noValue;
                return;
            }
            pathTilemap.SetTile(endGridPosition, endPathTile);
        }
        else
        {
            pathTilemap.SetTile(endGridPosition, null);
            endGridPosition = noValue;
        }
    }

    private bool IsPositionWithinBounds(Vector3Int position)
    {
        if (position.x < instantiatedRoom.room.templateLowerBounds.x || position.x > instantiatedRoom.room.templateUpperBounds.x
             || position.y < instantiatedRoom.room.templateLowerBounds.y || position.y > instantiatedRoom.room.templateUpperBounds.y)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void ClearPath()
    {
        //if pathstak is empty then return
        if(pathStack == null)
        {
            return;
        }
        //loop through each element in path stack and remove the path tile
        foreach(Vector3 worldPosition in pathStack)
        {
            pathTilemap.SetTile(grid.WorldToCell(worldPosition), null);
        }
        pathStack = null;
        endGridPosition = noValue;
        startGridPosition = noValue;
    }


    private void DisplayPath()
    {
        //if no start or end positions set then just return
        if(startGridPosition == noValue || endGridPosition == noValue)
        {
            return;
        }

        pathStack = AStar.BuildPath(instantiatedRoom.room, startGridPosition, endGridPosition);

        //if we have no path just return
        if(pathStack == null)
        {
            return;
        }

        //loop through the world positions , convert to grid position
        //and set the tile to the green tile.
        foreach(Vector3 worldPosition in pathStack)
        {
            pathTilemap.SetTile(grid.WorldToCell(worldPosition), startPathTile);
        }
    }
}
