using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName="roomNodeGraph",menuName="Scriptable Objects/Dungeon/Room Node Graph")]

public class roomNodeGraphSO : ScriptableObject
{
    [HideInInspector] public roomNodeTypeListSO roomNodeTypeList;
    [HideInInspector] public List<roomNodeSO> roomNodeList = new List<roomNodeSO>();
    //the dictionary is to have a string guid to reference each node too
    //used with the getter function GetRoomNode that takes a string and searches in the dictionary for the string
    [HideInInspector] public Dictionary<string, roomNodeSO> roomNodeDictionary = new Dictionary<string, roomNodeSO>();

    private void Awake()
    {
        LoadRoomNodeDictionary();
    }

    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();
        //populate dictionary
        foreach (roomNodeSO node in roomNodeList)
        {
            roomNodeDictionary[node.id] = node;
        }
    }
    //get room node by roomNodeType
    public roomNodeSO GetRoomNode(roomNodeTypeSO roomNodeType)
    {
        foreach(roomNodeSO node in roomNodeList)//loop through all nodes in the list
        {
            if(node.roomNodeType == roomNodeType)//if specified node is found
            {
                return node;//return it
            }
        }
        return null;
    }


    //a getter method that gets roomNode id
    public roomNodeSO GetRoomNode(string roomNodeID)
    {
        //trygetvalue from the dictionary class 
        //pass in the key and if it finds the key
        //put it in the out value as a roomNode
        if(roomNodeDictionary.TryGetValue(roomNodeID, out roomNodeSO roomNode))
        {
            return roomNode;
        }
        return null;
    }


    //get a child room node for supplied parent room node
    public IEnumerable<roomNodeSO> GetChildRoomNodes(roomNodeSO parentRoomNode)
    {
        foreach(string childNodeID in parentRoomNode.childRoomNodeIDList)
        {
            yield return GetRoomNode(childNodeID);
        }
    }


    #region Editor Code
    [HideInInspector] public roomNodeSO roomNodeToDrawLineFrom = null;
    [HideInInspector] public Vector2 linePosition;


    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    //takes a room node and position and updates the variables with the values
    public void SetNodeToDrawConnectionLineFrom(roomNodeSO node , Vector2 position)
    {
        roomNodeToDrawLineFrom = node;
        linePosition = position;
    }


    #endregion Editor Code

}
