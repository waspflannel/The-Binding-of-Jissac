using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]

public class DungeonLevelSO : ScriptableObject
{
    #region Header BASIC LEVEL DETAILS
    [Space(10)]
    [Header("Basic Level Details")]
    #endregion Header BASIC LEVEL DETAILS
    #region Tooltip
    [Tooltip("the name for the level")]
    #endregion Tooltip
    public string levelName;

    #region Header ROOM TEMPLATE FOR LEVEL
    [Space(10)]
    [Header("ROOM TEMPLATE FOR LEVEL")]
    #endregion Header ROOM TEMPLATE FOR LEVEL
    #region Tooltip
    [Tooltip("populate the list with the room templates that you want to be apart of the level" +
        "ensure that room templates are included for all room node types that are specified in the room node graphs for the level")]
    #endregion Tooltip
    public List<RoomTemplateSO> roomTemplateList;

    #region Header ROOM NODE GRAPHS FOR LEVEL
    [Space(10)]
    [Header("ROOM NODE GRAPHS FOR LEVEL")]
    #endregion Header ROOM GRAPHS FOR LEVEL
    #region Tooltip
    [Tooltip("populate this list with the room node graphs which should be randomly selected from for the level")]
    #endregion Tooltip
    public List<roomNodeGraphSO> roomNodeGraphList;

    #region Validation
    private void onValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);//make sure levelName is provided


        if (HelperUtilities.ValidateCheckEnumerateValues(this, nameof(roomTemplateList), roomTemplateList)) {//make sure roomTemplateList has values
            return;
        }
        if (HelperUtilities.ValidateCheckEnumerateValues(this, nameof(roomNodeGraphList), roomNodeGraphList))//make sure roomNodeGraphList has values
        {
            return;
        }
        //check that room templates are specified for all the node types in the specified node graphs
        //first check that n/s corridor , e/w corridor and entrance type have been specified
        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null)
            {
                return;
            }
            if (roomTemplateSO.roomNodeType.isCorridorEW)
            {
                isEWCorridor = true;
            }
            if (roomTemplateSO.roomNodeType.isCorridorNS)
            {
                isNSCorridor = true;
            }
            if (roomTemplateSO.roomNodeType.isEntrance)
            {
                isEntrance = true;
            }

        }
        if (isEWCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " : NO E/W Corridor Room Type Specified");
        }
        if (isNSCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " : NO N/S Corridor Room Type Specified");
        }
        if (isEntrance == false)
        {
            Debug.Log("In " + this.name.ToString() + " : NO Entrance Room Type Specified");
        }
        //loop through all node graphs
        //check that we have room templates for all the node types in the node graphs
        foreach (roomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if(roomNodeGraph == null)
            {
                return;
            }
            foreach (roomNodeSO roomNodeSO in roomNodeGraph.roomNodeList)//loop through the nodes in the graph
            {
                if (roomNodeSO == null)
                {
                    continue;
                }
                //check that a room template has been specified for each roomNode type

                //corridors and entrances already checked so if encountered just continue
                if (roomNodeSO.roomNodeType.isEntrance || roomNodeSO.roomNodeType.isCorridorEW || roomNodeSO.roomNodeType.isCorridorNS ||
                    roomNodeSO.roomNodeType.isCorridor || roomNodeSO.roomNodeType.isNone)
                {
                    continue;
                }
                bool isRoomNodeTypeFound = false;
                //loop through all room templates to check that this node type has been specified
                foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
                {
                    if (roomTemplateSO == null)
                    {
                        continue;
                    }
                    if (roomTemplateSO.roomNodeType == roomNodeSO.roomNodeType)//check if theres the correct template type for each roomNode type
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }
                }
                //if it breaks out of the nested foreach loop and a match isnt found then print error msg
                if (!isRoomNodeTypeFound)
                {
                    Debug.Log("in " + this.name.ToString() + " : no room template " + roomNodeSO.roomNodeType.name.ToString() + "found for node graph"
                        + roomNodeGraph.name.ToString());
                }
            }
            
        }
    }
    #endregion Validation


}

