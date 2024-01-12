using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private roomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    private void OnEnable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 0f);
    }

    private void OnDisable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }
    protected override void Awake()
    {
        base.Awake();//still run the base awake

        //load the room node type list
        LoadRoomNodeTypeList();

        //set dimmed material to fully visable
        

    }

    //load the room node type list from gameResources
    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }



    //this will keep trying to generate a dungeon
    //first it will loop through all allowed attempts of a random node graph before going to the next
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList; //set the template list to the current levels template list
        LoadRoomTemplatesIntoDictionary();
        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        //keep looping while dungeonBuildSuccessful is false and dungeonBuildAttempts is less than allowed attempts
        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            roomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);//get a random graph

            int dungeonRebuildAttemptsForNodeGraph = 0;//rebuild attempts for the randomly selected node graph
            dungeonBuildSuccessful = false;

            //loop until dungeon is built or more than max  attempts for the node graph
            while (!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                //clear dungeon room gameobjects and dungeon room dict
                ClearDungeon();
                dungeonRebuildAttemptsForNodeGraph++;


                //this will pass a boolean value into dungeonBuildSuccessful , if its false it will keep relooping
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);

            }
            if (dungeonBuildSuccessful)
            {
                InstantiateRoomGameobjects();
            }
        }
        return dungeonBuildSuccessful;

    }



    private void LoadRoomTemplatesIntoDictionary()
    {
        //clear room template dictionary
        roomTemplateDictionary.Clear();

        //load room template list into dict
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            //if the dict doesnt already have the key 
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                //add it to the dict
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                //if there is already there then send error to console
                Debug.Log("Duplicate room template key in " + roomTemplateList);
            }
        }
    }
    //attempt to randomly build the dungeon for the selected room node graph

    private bool AttemptToBuildRandomDungeon(roomNodeGraphSO roomNodeGraph)
    {
        Queue<roomNodeSO> openRoomNodeQueue = new Queue<roomNodeSO>();
        //get the entrace and add to queue
        roomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("no entrance Node");
            return false;
        }
        bool noRoomOverlaps = true;
        //process the queue
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);
        //if all room nodes have been processed and there has been no room overlap
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    private bool ProcessRoomsInOpenRoomNodeQueue(roomNodeGraphSO roomNodeGraph, Queue<roomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        //while 
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            //get the node
            roomNodeSO roomNode = openRoomNodeQueue.Dequeue();
            //loop through its children
            foreach (roomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                //add its children to the queue
                openRoomNodeQueue.Enqueue(childRoomNode);
            }
            //if room node type is an entrance , mark as positioned and add to room directory
            if (roomNode.roomNodeType.isEntrance)
            {


                //////////////////////////////////////////////////////
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                if(roomTemplate == null)
                {
                Debug.Log("null room template it is null");
                }

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);
                room.isPositioned = true;
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            //its not an entrance
            else
            {
                //get the parent room fro the node
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }

        }
        return noRoomOverlaps;
    }
    private bool CanPlaceRoomWithNoOverlaps(roomNodeSO roomNode, Room parentRoom)
    {
        //init and assume true until proven otherwise
        bool roomOverlaps = true;

        //do while room overlaps , try to place all available doorways of the parent 
        //until the room is placed without overlap
        while (roomOverlaps)
        {
            //select random unconnected available doorway for parent
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                //if no more doorways to try then we have an overlap
                return false;
            }
            //get a random parent doorway
            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];
            //get a random room template for room node that is consistent with the parent door orientation
            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            //create a room from template
            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;
                room.isPositioned = true;
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }

        }
        return true;
    }
    //get random room template for room node taking into account the parent doorway orientation
    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(roomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;

        //if room node is a corridor then select random correct corridor room template based on parent doorway orientation
        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;


                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;

                case Orientation.none:
                    break;

                default:
                    break;
            }
        }
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }
        return roomTemplate;
    }

    //returns true if the room doesnt overlap , false otherwise
    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        //get the opposite doorway
        //if we have an entrance and we selected the north doorway of the entrance
        //then for the corridor we need the south entrance to align with the north entance
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);

        //if no doorway in room opposite to parent doorway
        if (doorway == null)
        {
            //marked parent doorway so we dont try to connect it again
            doorwayParent.isUnavailable = true;
            return false;
        }

        //calculate world grid position
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;
        Vector2Int adjustment = Vector2Int.zero;

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;
            case Orientation.none:
                break;
            default:
                break;
        }
        //calculate lower and upper bounds based on positioning to align with parent doorway
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);

        //overlappingRoom ==null  means the room is not overlapping
        if (overlappingRoom == null)
        {
            //mark doorways as connected and unavailable
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;
            return true;
        }
        else
        {
            //mark parent doorway as unavailable so we dont try and connect it again
            doorwayParent.isConnected = true;
            return false;
        }

    }
    //get the doorway from the doorway list that has opposite orientation to doorway
    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach (Doorway doorwayToCheck in doorwayList)
        {
            //loop through the doorways and return the doorway that has the opposite orientation as parent doorway
            if (parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
        }
        return null;
    }
    //check for rooms that overlap the upper and lower bound parameters
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            //skip if same room as room to test or room hasnt been positioned
            if (room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }
            //check if a placed room is over another placed room
            if (IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }
        return null;
    }
    private bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX = isOverLappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);
        bool isOverlappingY = isOverLappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);

        if (isOverlappingX && isOverlappingY)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //check if interval 1 overlaps interval2 , this is used in the IsOverLappingRoom method
    private bool isOverLappingInterval(int imin1, int imax1, int imin2, int imax2)
    {

        //if the max of the mins is less than or equal to the min of the maxes , it overlaps
        if (Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //get a random room template from the roomtemplatelist that matches the roomtype and return it
    private RoomTemplateSO GetRandomRoomTemplate(roomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();
        

        //loop thru the template list
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                //add all matching templates
                matchingRoomTemplateList.Add(roomTemplate);
            }

        }
        //if list is empty return null
        if (matchingRoomTemplateList.Count == 0)
        {
            Debug.Log("null list??");
            return null;
        }
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }

    //get unconnected doorways
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }

    //create a room based on roomtemplate and layoutNode and return the created room
    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, roomNodeSO roomNode)
    {
        Room room = new Room();
        if (roomTemplate == null)
        {
            Debug.Log("null template?");
            return null;
        }
        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;


        //using CopyStringList method because we want another list not just a reference
        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

        if (roomNode.parentRoomNodeIDList.Count == 0)//entrance since entrance is only node with no parent
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;
            GameManager.Instance.SetCurrentRoom(room);
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];//[0] because each node only has 1 parent
        }
        return room;

    }

    private roomNodeGraphSO SelectRandomRoomNodeGraph(List<roomNodeGraphSO> roomNodeGraphList)
    {
        //if the list has members
        if (roomNodeGraphList.Count > 0)
        {
            //return a random list
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No room node graphs in List");
            return null;
        }
    }
    //instantiate the dungeon room gameobjects from the prefabs
    private void InstantiateRoomGameobjects()
    {
        //loop through dict
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            //calculate room position
            //remember the room instantiation position needs to be adjusted by the room template lower bounds
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            //instantiate room at the roomPosition
            GameObject roomGameObject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            //get instantiated room component from instantiated prefab
            InstantiatedRoom instantiatedRoom = roomGameObject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;

            //init the instantiated room
            //this will call Initialise in InstantiatedRoom class
            instantiatedRoom.Initialise(roomGameObject);

            //save gameobject reference
            room.instantiatedRoom = instantiatedRoom;
        }
    }
    //get a room template by room template ID , return null if id DNE
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {

        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    //get room by roomID , if no room exists with that ID return null
    public Room GetRoomByRoomID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }
    private void ClearDungeon()
    {

        //destroy instaniated dungeon gameobjects and then clear dungeon manager room dictionary
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            //loop through each item in dictionary
            foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
            {
                Room room = keyvaluepair.Value;
                if (room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }
            dungeonBuilderRoomDictionary.Clear();
        }
    }
    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();
        foreach (Doorway doorway in oldDoorwayList)
        {

            //for each doorway make a new doorway and copy its attributes
            //we need to set each value so it makes a new one and not just reference it
            Doorway newDoorway = new Doorway();
            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;
            newDoorwayList.Add(newDoorway);
        }
        return newDoorwayList;
    }
    //create a deep copy of string list
    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();
        foreach (string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);

        }
        return newStringList;
    }

}
