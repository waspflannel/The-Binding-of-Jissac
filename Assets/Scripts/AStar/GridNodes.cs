using UnityEngine;

public class GridNodes
{
    private int width;
    private int height;

    private Node[,] gridNode;

    public GridNodes(int width, int height)
    {
        this.width = width;
        this.height = height;

        gridNode = new Node[width, height];

        //make a grid of nodes
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //node constructor will save its grid position
                gridNode[i, j] = new Node(new Vector2Int(i, j));
            }
        }
    }

    public Node GetGridNode(int xPosition, int yPosition)
    {
        if (xPosition < width && yPosition < height)
        {
            return gridNode[xPosition, yPosition];
        }
        else
        {
            Debug.Log("requested grid node is out of range"); 
            return null;
        }
    }
}

