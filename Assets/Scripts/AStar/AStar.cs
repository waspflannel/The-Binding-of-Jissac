using System.Collections.Generic;
using UnityEngine;

public static class AStar 
{
    //builds a path for the room.
    //adds movement steps to the returned stack
    public static Stack<Vector3> BuildPath(Room room , Vector3Int startGridPosition , Vector3Int endGridPosition)
    {
        //adjust grid 
        startGridPosition -= (Vector3Int)room.templateLowerBounds;
        endGridPosition -= (Vector3Int)room.templateLowerBounds;

        List<Node> openNodeList = new List<Node>();
        HashSet<Node> closedNodeList = new HashSet<Node>();


        //instantiate node grid
        GridNodes gridNodes = new GridNodes(room.templateUpperBounds.x - room.templateLowerBounds.x + 1,
            room.templateUpperBounds.y - room.templateLowerBounds.y + 1);

        Node startNode = gridNodes.GetGridNode(startGridPosition.x, startGridPosition.y);
        Node targetNode = gridNodes.GetGridNode(endGridPosition.x, endGridPosition.y);

        //this runs the algo
        Node endPathNode = FindShortestPath(startNode, targetNode, gridNodes, openNodeList, closedNodeList, room.instantiatedRoom);

        if(endPathNode != null)
        {
            return CreatePathStack(endPathNode, room);
        }
        return null;

    }

    private static Node FindShortestPath(Node startNode , Node targetNode , GridNodes gridNodes , List<Node> openNodeList , 
                        HashSet<Node> closedNodeHashSet , InstantiatedRoom instantiatedRoom)
    {
        //add start node to open list
        openNodeList.Add(startNode);
        //loop through open node list
        while(openNodeList.Count > 0)
        {

            //sort list
            openNodeList.Sort();

            //get the node in the list with lowest fCost(wil be at the top after sort)
            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            //if the node is the target then finish
            if(currentNode == targetNode)
            {
                return currentNode;
            }
            //add node to the closed list
            closedNodeHashSet.Add(currentNode);

            //evaluate fcost for each neighbor of the current node
            EvaluateCurrentNodeNeighbors(currentNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, instantiatedRoom);

        }
        return null;
    }

    //create a stack of vector3's containing the movement path
    //converts the grid position of each grid node to a world position and adjusts it so its in the center of the node
    //then adds it to the stack
    public static Stack<Vector3> CreatePathStack(Node targetNode , Room room)
    {
        Stack<Vector3> movementPathStack = new Stack<Vector3>();

        Node nextNode = targetNode;

        //get mid point of cell
        Vector3 cellMidPoint = room.instantiatedRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0f;

        while(nextNode != null)
        {
            //convert grid position to world position
            Vector3 worldPosition = room.instantiatedRoom.grid.CellToWorld(new Vector3Int(nextNode.gridPosition.x + room.templateLowerBounds.x,
                nextNode.gridPosition.y + room.templateLowerBounds.y, 0));
            //set the world position to the middle of the cell
            worldPosition += cellMidPoint;
            movementPathStack.Push(worldPosition);

            nextNode = nextNode.parentNode;

        }
        return movementPathStack;
    }

    private static void EvaluateCurrentNodeNeighbors(Node currentNode , Node targetNode , GridNodes gridNodes ,
                                             List<Node> openNodeList, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;
        Node validNeighborNode;
        //loop through each neighbor of the currentNode
        for(int i=-1; i <= 1; i++)
        {
            for(int j=-1; j <= 1; j++)
            {
                //skip the currentNode
                if(i==0 && j == 0)
                {
                    continue;
                }
                validNeighborNode = GetValidNodeNeighbor(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j,
                    gridNodes, closedNodeHashSet, instantiatedRoom);

                if(validNeighborNode != null)
                {
                    //get new gCost
                    int newCostToNeighbor;
                    //get the movement penalty, unwalkable paths have a value of 0
                    int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[validNeighborNode.gridPosition.x, validNeighborNode.gridPosition.y];

                    newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, validNeighborNode) + movementPenaltyForGridSpace;
                    bool isValidNeighborNodeInOpenList = openNodeList.Contains(validNeighborNode); 

                    //if it is not in the open list or gcost is less than current
                    if(newCostToNeighbor < validNeighborNode.gCost || !isValidNeighborNodeInOpenList)
                    {
                        validNeighborNode.gCost = newCostToNeighbor;
                        validNeighborNode.hCost = GetDistance(validNeighborNode, targetNode);
                        validNeighborNode.parentNode = currentNode;

                        if (!isValidNeighborNodeInOpenList)
                        {
                            openNodeList.Add(validNeighborNode);
                        }
                    }
                }
            }
        }
    }

    private static int GetDistance(Node A , Node B)
    {
        int distx = Mathf.Abs(A.gridPosition.x - B.gridPosition.x);
        int distY = Mathf.Abs(A.gridPosition.y - B.gridPosition.y);

        if(distx > distY)
        {
            return (14*distY) + (10 *(distx - distY));
        }
        return (14 * distx) + (10 * (distY - distx));
    }
    
    private static Node GetValidNodeNeighbor(int neighborNodeXPosition, int neighborNodeYPosition , GridNodes gridNodes ,
        HashSet<Node> closedNodeHashSet , InstantiatedRoom instantiatedRoom)
    {
        //if node position is valid
        //if the node is outside the instantiated room then return null
        if(neighborNodeXPosition >= instantiatedRoom.room.templateUpperBounds.x - instantiatedRoom.room.templateLowerBounds.x
            || neighborNodeXPosition < 0
            ||neighborNodeYPosition >= instantiatedRoom.room.templateUpperBounds.y - instantiatedRoom.room.templateLowerBounds.y
            ||neighborNodeYPosition < 0)
        {
            return null;
        }
        //get the neighbor node from grid
        Node neighborNode = gridNodes.GetGridNode(neighborNodeXPosition, neighborNodeYPosition);

        int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[neighborNodeXPosition, neighborNodeYPosition];


        //if its in the closed list then dont return it
        //or if its a collision tile(obstacle)
        if (closedNodeHashSet.Contains(neighborNode) || movementPenaltyForGridSpace == 0)
        {
            return null;
        }
        //if all else is good return it
        else
        {
            return neighborNode;
        }
    }

}
