using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

public class roomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;//id of room node
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();//list of parent room node ids
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();//list of child room node ids
    [HideInInspector] public roomNodeGraphSO roomNodeGraph;//holds graph of room nodes
    public roomNodeTypeSO roomNodeType;//holds room type at a node
    [HideInInspector] public roomNodeTypeListSO roomNodeTypeList;//holds lists of the types.

    #region Editor Code
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;


    //initialize nodes could be consired as a node constructor. Called in roomNodeGraphEditor
    public void Initialise(Rect rect, roomNodeGraphSO nodeGraph, roomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();//get unique id
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        //load room node type list
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;

    }

    //used to draw the nodes in a loop Called in roomNodeGraphEditor
    public void Draw(GUIStyle nodeStyle)
    {
        
        GUILayout.BeginArea(rect, nodeStyle);//draw the node box using the nodestyle defined 

        EditorGUI.BeginChangeCheck();//start region to detect popup selection changes


        //if the node has a parent or the roomType is a parent
        //this is used to disable the dropdown menu when you connect two room nodes.
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance){
            //instead of displaying the selection menu display its name
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);//display its name
        }
        else
        {

            //display a popup using the roomNodeType name values that can be selected from
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);//if it has a selected roomnodetype

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());//shows the selected node graph
            roomNodeType = roomNodeTypeList.list[selection];

            //if the room type selection has changed making child connnections invalid
            //for example if a corridor is originally selected and now it isnt a corridor
            //if a room was selected and now a corridor is selected
            //if a not boss room was selected and now a boss room is selected
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isCorridor
                && roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                //if the list is not empty
                if(childRoomNodeIDList.Count > 0)
                {
                    //loop through child room nodes
                    for(int i = childRoomNodeIDList.Count-1; i >=0; i--)
                    {
                        //get the child
                        roomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        //if the child node is not null
                        if(childRoomNode != null)
                        {
                            //remove child from parent room node children list
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                            //remove parent id from child parent id list
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }
                }

            }
        }


        //if a change is detected between the two check area make sure the changes are saved
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
        GUILayout.EndArea();

    }

    //fills an array of strings with room node types to display and what can be selected
    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];//initialize list to number of different room node types in list

        for(int i=0; i<roomNodeTypeList.list.Count; i++)
        {
            //displayInNodeGraphEditor and  roomNodeTypeName are defined in roomNodeTypeSO
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)//loop through list
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
            {

            }
        }
        return roomArray;

    }


    //process events for the node
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    //process mouse down events
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();

        }
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    //handles left click down event
    private void ProcessLeftClickDownEvent()
    {
        Selection.activeObject = this;//will object highlight in the unity editor

        //handles node selection
        if (isSelected == true)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }
    }

    //processes right click down
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        //if left click is up
        if(currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();//call ProcessLeftClickUpEvent
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        //when the left click gets raised you can no longer drag so dragging is false
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    //for dragging
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        //if left click is down
        if(currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);//call ProcessLeftMouseDragEvent
        }
    }
    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
       
        isLeftClickDragging = true;//if draggigng then isLeftClickDragging is true

        DragNode(currentEvent.delta);//currentEvent.delta gives us how much our mouse is moving by
        GUI.changed = true;//tells that gui has been changed
    }

    public void DragNode(Vector2 delta)
    {
        rect.position += delta;//takes the delta and updates the rect position
        EditorUtility.SetDirty(this);//tells unity something happend and to save it
    }

    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        //check if child node can be added validly to parent
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;

    }
    //check if the child node can be validly added to the parent



    public bool IsChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;
        //check if there is already a connected boss room in the node graph
        foreach (roomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            //check if the node is a bossRoom and the node is already connected
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                //set to true
                isConnectedBossNodeAlready = true;
            }
        }
        //if the child node is a bossRoom and there is already a connected boss room then return false
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
        {
            return false;
        }

        //makes sure a node of type none cant be connected
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
        {
            return false;
        }

        //makes sure you cant keep connecting the same node
        if (childRoomNodeIDList.Contains(childID))
        {
            return false;
        }

        //makes sure you cant connect a node to itself
        if (id == childID)
        {
            return false;
        }
        //dont want to connect a node who is already a parent of the node
        if (parentRoomNodeIDList.Contains(childID))
        {
            return false;
        }
        //makes sure every node has 1 parent
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0) {
            return false;
        }
        //make sure you dont connect corridors together
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
        {
            return false;
        }

        /////////////THIS ONE WILL BE COMMENTED OUT FOR THE SAKE OF MY GAME DESIGN//////////////////
        //if child is not corridor and this node is not a corridor return false
        //makes sure you dont connect two rooms without a corridor in between
        if(!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
        {
            return false;
        }

        //if adding a corridor check that this node has less than the maximum permitted child corridors
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
        {
            return false;
        }
        //cannot connect to an entrance node because entrance must always be top level parent
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
        {
            return false;
        }

        //if adding a room to a corridor we check that this corridor doesnt have a room added
        if(!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
        {
            return false;
        }
        return true;

    }


    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    //used to remove childID from the node
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        //if the list has the ID then remove it from the list
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }

    //used to remove parentID from the node
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        //if the list has the ID then remove it from the list
        if (parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }
        return false;
    }

    #endregion Editor Code

}

