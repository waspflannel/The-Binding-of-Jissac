
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;//create GUIStyle
    private GUIStyle roomNodeSelectedStyle;//create a GUISTYLE for selected nodes
    private static roomNodeGraphSO currentRoomNodeGraph;
    private roomNodeSO currentRoomNode = null;
    private roomNodeTypeListSO roomNodeTypeList;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

    private const float gridLarge = 100f;
    private const float gridSmall = 25f;


    //fields
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;
    

    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6;
    [MenuItem("Room Node Graph Editor",menuItem="Window/Dungeon Editor/Room Node Graph Editor")]//make a menu item for window tab

    private static void openWindow()//for opening window
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");//make title of generic type and give it a window title
    }

    //open the room node graph editor window if a node graph SO is double clicked in the inspector
    [OnOpenAsset(0)]//callback Attribute, will call when unity is about to open an asset
    public static bool OnDoubleClickAsset(int instanceID , int line)
    {
        roomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as roomNodeGraphSO;//takes instanceID and set it as a roomNodeGraphSO
        //checks to see if it is empty if it isnt then set it to the current node being used otherwise retur nfalse
        if(roomNodeGraph != null)
        {
            openWindow();
            currentRoomNodeGraph = roomNodeGraph; 
            return true;
        }
        return false;
    }


    //just sets up how the nodes look
    private void OnEnable()//onEnable function to set up GUIStyle
    {

        //for highlighting multiple objects on inspector selection change event
        //this is for changing the roomNodeGraph you see in the editor when you select another one
        Selection.selectionChanged += InspectorSelectionChanged;

        roomNodeStyle = new GUIStyle();//make new GUIStyle
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;//load inbuilt texture node1
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        //define for selected style
        roomNodeSelectedStyle = new GUIStyle();//make new GUIStyle
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;//load inbuilt texture node1
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);


        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;//retrieve roomNodeTypeList from GameResources class
    }
    private void OnDisable()
    {
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    //draws editor GUI
    private void OnGUI()
    {
        DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
        DrawBackgroundGrid(gridLarge, 0.2f, Color.gray);
        //if a SO of type roomNodeGraph has been selected
        if (currentRoomNodeGraph != null)
        {
            DrawDraggedLine();//called before DrawRoomNodes() so the dragged line appears behind the room nodes
            //if not null call functions
            ProcessEvents(Event.current);

            DrawRoomConnections();
            DrawRoomNodes();
        }

        //update gui on change
        if (GUI.changed)
        {
            Repaint();
        }

    }
    //methdo to draw the bg grid in the editor
    private void DrawBackgroundGrid(float gridSize , float gridOpacity , Color gridColor)
    {
        //position.width and position.height is the size of the screen
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
        graphOffset += graphDrag * 0.5f;//graph Drag is a vec2
        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        for(int i =0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize , gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j , 0f) + gridOffset);
        }

        Handles.color = Color.white;

    }
    private void DrawDraggedLine()
    {
        if(currentRoomNodeGraph.linePosition != Vector2.zero)//make sure the line position isnt 0
        {
            //draws line from room node rect center to mouse position

            //start position = staringNode position
            //end position end of the line
            //tangents are just the start and end position so we dont draw a curve
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
            currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    private void ProcessEvents(Event currentEvent)

    {
        graphDrag = Vector2.zero;
        //get room node that mouse is over or if its null or not being currently dragged
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }
        if(currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom !=null)//currentRoomNodeGraph.roomNodeToDrawLineFrom refers to a room node and !=null means if a line is trying to be drawn
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);//calls processEvents function in roomNodeSO
        }
        

    }

    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            //process mouse down events
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
    private roomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for(int i=currentRoomNodeGraph.roomNodeList.Count-1; i>=0; i--)//loop through nodes
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))//detects if mouse is on a room node
            {
                return currentRoomNodeGraph.roomNodeList[i];//if it is return it
            }
        }
        return null;
    }

    //process mouse down events on the room node graph(not over a node)
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        //process right click mouse down on graph event and show context menu
        if(currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
        else if(currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }


    //if ProcessMouseDownEvent reads a right click it sends currentEvent.mousePosition to this function as a vec2(x,y)
    //it then makes a generic menu at the mouse position
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);//will run CreateRoomNode function if menu item is selected
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Deleted Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Deleted Selected Room Nodes"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }
    

    //when the menu item is selected it will run this function and take the mousePosiiton as an object
    //
    private void CreateRoomNode(object mousePositionObject)
    {
        if(currentRoomNodeGraph.roomNodeList.Count == 0)//see if the list is empty
        {
            //if it is make a room node of type entrance 
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));//Find(x => x.isEntrance) means find a roomNode in the typeList that is entrance
        }
        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));//will create an isNone node

    }
    private void ClearAllSelectedRoomNodes()
    {
        foreach(roomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;
                GUI.changed = true;
            }
        }
    }
    private void SelectAllRoomNodes()
    {
        foreach(roomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    //create a room node aswell but overloaded to also take in roomNodeType
    private void CreateRoomNode(object mousePositionObject, roomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;
        //create room node SO asset
        roomNodeSO roomNode = ScriptableObject.CreateInstance<roomNodeSO>();//create instance of roomNodeSO


        //access the roomNodeList in the roomNodeGraphSO class and add the node to the list
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        //set node values
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);
        //add room node graph SO to object asset databse
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();//writes saved assets to disk

        currentRoomNodeGraph.OnValidate();
    }
    private void DeleteSelectedRoomNodes()
    {
        //Initialize a Queue to hold the roomNodes we want to delete so it does not cause
        //any issues from deleting straight from a list we are iterating through
        Queue<roomNodeSO> roomNodeDeletionQueue = new Queue<roomNodeSO>();

        foreach(roomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if(roomNode.isSelected && !roomNode.roomNodeType.isEntrance)//we shouldnt delete an entrance node
            {
                roomNodeDeletionQueue.Enqueue(roomNode);//add node we wnat to delete to queue

                //now iterate through all its children
                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    //get the child
                    roomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);

                    if (childRoomNode != null)
                    {
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);//remove the parent node from the child parent list
                    }
                }


                //iterate through its parents
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    //get the parent
                    roomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);
                    if(parentRoomNode!= null)
                    {
                        //remove it as a child of its parent
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

            }
        }
        while(roomNodeDeletionQueue.Count > 0)
        {
            //get the roomNode from the queue and dequeue it
            roomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            //remove it from the dictionary
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            //remove it from the list
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            DestroyImmediate(roomNodeToDelete, true);//remove from the asset database

            AssetDatabase.SaveAssets();
        }

    }


    //delete the links between selected room nodes
    private void DeleteSelectedRoomNodeLinks()
    {
        //loop through all roomNodes
        foreach(roomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            //if the node is selected and it has a link (roomNode.childRoomNodeIDList.Count > 0 checks if it has a link)
            //since if the childRoomNodeIDList.Count was 0 or less it means no children which means no link
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                //loop through all the children of the selected nodes
                for(int i = roomNode.childRoomNodeIDList.Count -1; i >=0; i--)
                {
                    //get the child room node
                    roomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    //if the child is selected
                    if(childRoomNode !=null && childRoomNode.isSelected)
                    {
                        //remove the childId from the roomNode childID list
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                        //remove the roomNOde from the child room node parentID List
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }
        //unselect all the selected roomNodes
        ClearAllSelectedRoomNodes();
    }


    //process mouse up
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        //if releasing the right mouse button and currently dragging a line
        if(currentEvent.button ==1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            roomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);//check if over a roomNode

            //IsMouseOverRoomNode() will return null if its not over a room node so checking if roomNode != null will tell us if its over a room node
            if (roomNode != null)
            {
                //get the room node we started drawing the line from and add the id to children list
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);//makes the starting node the parent
                }
            }
            ClearLineDrag();
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        //process right click drag event for drawing line
        if(currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        //process left click drag event for dragging the node graph
        else if(currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    //process right mouse drag event to draw a line
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null)//makes sure the node to draw from is not null
        {
            DragConnectingLine(currentEvent.delta);//get its vec2 position changed
            GUI.changed = true;//tell system gui has been changed
        }
    }
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        graphDrag = dragDelta;
        //loop through each node
        for(int i=0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            //for each node being moved when you drag the editor, call DragNode method using the dragDelta
            //dragDelta is how much the screen has moved
            currentRoomNodeGraph.roomNodeList[i].DragNode(dragDelta);
        }
        GUI.changed = true;
    }
    public void DragConnectingLine(Vector2 delta)//takes a delta
    {
        currentRoomNodeGraph.linePosition += delta;//update the line position member variable in the roomNodeGraphSO class with the delta
    }

    private void ClearLineDrag()
    {
        //reset roomNodeGraphSO member variables when done drawing the line
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    //draw connections in the graph window between room nodes
    private void DrawRoomConnections()
    {
        foreach (roomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            //loop through child room nodes
            if(roomNode.childRoomNodeIDList.Count > 0)
            {
                
                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    //get the room node from dictionary
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        //passing into DrawConectionLine the current roomNode and the child room node
                        DrawConectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);
                        GUI.changed = true;
                    }
                }
            }
        }
    }
    private void DrawConectionLine(roomNodeSO parentRoomNode , roomNodeSO childRoomNode)
    {
        //get line start and end positions

        //we know where the start and end positions because the
        //parent node will be the start and the child will be the end position
        //we create this relationship in part of the ProcessMouseUpEvent() method
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        //get mid point of line
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        //get the direction of the line
        Vector2 direction = endPosition - startPosition;

        //the arrow tail points above and below perpendicular to the line
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        //arrow point head on the line of connection
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        //draw arrow
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);
        //draws the line with startposition and end position
        //they are reused as tangents so the line does not curve

        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }


    //draw room nodes in the graph window
    private void DrawRoomNodes()
    {
        //loop through all room nodes in the roomNodeSO list
        foreach (roomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);//if selected use the selected gui style to show selection
            }
            else
            {
                roomNode.Draw(roomNodeStyle);//draw GUIStyle
            }
        }
        GUI.changed = true;//notify that gui has been changed
    }


    //this is for changing the roomNodeGraph you see in the editor when you select another one
    private void InspectorSelectionChanged()
    {
        roomNodeGraphSO roomNodeGraph = Selection.activeObject as roomNodeGraphSO;
        if(roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }

}


