using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Tilemaps;


[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public Bounds roomColliderBounds;
    [HideInInspector] public int[,] aStarMovementPenalty;
    

    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        //save room collider bounds
        roomColliderBounds = boxCollider2D.bounds;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == Settings.playerTag && room != GameManager.Instance.GetCurrentRoom())
        {
            this.room.isPreviouslyVisited = true;
            StaticEventHandler.CallRoomChangedEvent(room);
        }
    }
    public void Initialise(GameObject roomGameobject)
    {
        PopulateTilemapMemberVariables(roomGameobject);
        BlockOffUnusedDoorways();
        AddObstacles();
        AddDoorsToRooms();
        DisableCollisionTilemapRenderer();
        
    }

    //this method loops through our tilemaps and populates the grid and tilemap member variables
    private void PopulateTilemapMemberVariables(GameObject roomGameobject)
    {
        //since Grid is a child in the main Room prefab object we can retrieve it using GetComponentInChildren
        grid = roomGameobject.GetComponentInChildren<Grid>();

        //get tilemaps and store in an array
        Tilemap[] tilemaps = roomGameobject.GetComponentsInChildren<Tilemap>();

        //loop through array
        foreach(Tilemap tilemap in tilemaps)
        {
            if(tilemap.gameObject.tag == "groundTilemap")
            {
                groundTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration1Tilemap")
            {
                decoration1Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration2Tilemap")
            {
                decoration2Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "frontTilemap")
            {
                frontTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "collisionTilemap")
            {
                collisionTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "minimapTilemap")
            {
                minimapTilemap = tilemap;
            }
        }
    }
    private void BlockOffUnusedDoorways()
    {
        //loop through doorways and call block off function
        foreach(Doorway doorway in room.doorWayList)
        {
            if (doorway.isConnected)
            {
                continue;
            }
            if(collisionTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(collisionTilemap, doorway);
            }
            if (minimapTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(minimapTilemap, doorway);
            }
            if (groundTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(groundTilemap, doorway);
            }
            if (decoration1Tilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration1Tilemap, doorway);
            }
            if (frontTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(frontTilemap, doorway);
            }
        }
    }
    //block off the doorway on a tilemap layer
    private void BlockADoorwayOnTilemapLayer(Tilemap tilemap , Doorway doorway)
    {
        switch (doorway.orientation)
        {
            //for north and south we need to block off horizontally
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tilemap, doorway);
                break;

            //for east and west we need to block off vertically
            case Orientation.east:
            case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;
            case Orientation.none:
                break;
        }
    }

    //will copy tiles from left to right and top to bottom
    private void BlockDoorwayHorizontally(Tilemap tilemap , Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;
        //loop through every tile to copy
        for(int xPos =0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for(int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                //get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                //copy the tile
                tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                //set rotation of the tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);
            }
        }
    }
    private void BlockDoorwayVertically(Tilemap tilemap , Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {
            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
            {
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x  + xPos, startPosition.y -1 - yPos, 0), transformMatrix);
            }
        }
    }
    private void AddObstacles()
    {
        aStarMovementPenalty = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1];
        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for(int y=0; y< (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                //set default movement penalty for grid squares
                aStarMovementPenalty[x, y] = Settings.defaultAStarMovementPenalty;
                //add obstacles for collision tiles so enemies dont walk through it
                TileBase tile = collisionTilemap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));
                //once we get the tile , loop through all the tiles we dont want enemies to walk through
                foreach (TileBase collisionTile in GameResources.Instance.enemyUnwalkableCollisionTileArray)
                {
                    //if the tile we got from the tilemap matches that tile
                    if(tile == collisionTile)
                    {
                        //set penalty to 0
                        aStarMovementPenalty[x, y] = 0;
                        break;
                    }
                    
                }
                if(tile == GameResources.Instance.preferredEnemyPathTile)
                {
                    aStarMovementPenalty[x, y] = Settings.preferredPathAStarMovementPenalty;
                }

            }
        }
    }


    //this adds door prefabs to the doors
    private void AddDoorsToRooms()
    {
        if (room.roomNodeType.isCorridorEW || room.roomNodeType.isCorridorNS)
        {
            return;
        }
        foreach(Doorway doorway in room.doorWayList)
        {
            if(doorway.doorPrefab !=null && doorway.isConnected)
            {
                float tileDistance = Settings.tileSizePixels / Settings.pixelsPerUnit;

                GameObject door = null;
                if(doorway.orientation == Orientation.north)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    //fix the position, we move each door half in the x and 1 in the y
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y + tileDistance, 0f);
                }
                else if(doorway.orientation == Orientation.south)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    //for south doors we position it 1 more on the x but leave y the same
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y, 0f);
                }
                else if (doorway.orientation == Orientation.east)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    //for east we add 1 to the x and 1*1.25 to the y
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance, doorway.position.y + tileDistance*1.25f, 0f);
                }
                else if (doorway.orientation == Orientation.west)
                {
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    //for west we leave x the same and 1*1.25 to the y
                    door.transform.localPosition = new Vector3(doorway.position.x, doorway.position.y + tileDistance * 1.25f, 0f);
                }
                Door doorComponenet = door.GetComponent<Door>();
                if (room.roomNodeType.isBossRoom)
                {
                    doorComponenet.isBossRoomDoor = true;
                    doorComponenet.LockDoor();
                }
            }
        }
    }
    //disable collision tilemap renderer
    //stops it from showing on the screen
    private void DisableCollisionTilemapRenderer()
    {
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }
}
