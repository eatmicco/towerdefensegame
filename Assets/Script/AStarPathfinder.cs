using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEditor.Experimental.GraphView;
using Vector2 = UnityEngine.Vector2;

// Reference: https://medium.com/@nicholas.w.swift/easy-a-star-pathfinding-7e6689c7f7b2

public class AStarPathfinder : MonoBehaviour {

    public LevelLoader levelLoader;
    public int tileX = -1;
    public int tileY = -1;

    //scoring system
    //G = movement cost from start point to current point (1 per tile for adjacent, 1.5 per tile for diagonal)
    //H = estimated cost from current to end (use "Manhattan Length")

    public class TileNode
    {
        public LevelLoader.TilePoint tile;
        public int F;
        public int H;
        public int G;
        public TileNode parent;
    }

    public List<LevelLoader.TilePoint> FindPath(LevelLoader.TilePoint from, LevelLoader.TilePoint to)
    {
	    //open set
	    List<TileNode> openSet = new List<TileNode>();
	    List<TileNode> closedSet = new List<TileNode>();

		openSet.Clear();
        closedSet.Clear();

        tileX = levelLoader.GetTileX();
        tileY = levelLoader.GetTileY();

        TileNode currentNode = new TileNode();
        currentNode.tile = from;
        currentNode.G = 0;
        currentNode.H = CalculateH(from, to);
        currentNode.F = CalculateF(currentNode.G, currentNode.H);
        openSet.Add(currentNode);

        bool arrive = false;
        while (openSet.Any())
        {
			// Find the lowest F score in openSet and put it to the closedSet, remove it from openSet
			int lowest = 1000;
			foreach (var node in openSet)
			{
				if (node.F < lowest)
				{
					lowest = node.F;
					currentNode = node;
				}
			}

			if (currentNode.tile.x == to.x && currentNode.tile.y == to.y)
			{
				arrive = true;
				break;
			}

			closedSet.Add(currentNode);
			openSet.Remove(currentNode);

			LevelLoader.TilePoint current = currentNode.tile;

			//list scores of adjacent tiles from current
			for (int x = current.x - 1; x < current.x + 2; ++x)
            {
                if (x < 0 || x > tileX-1) continue;

                for (int y = current.y - 1; y < current.y + 2; ++y)
                {
                    if (x == current.x && y == current.y) continue;

                    if (y < 0 || y > tileY-1) continue;

                    if (levelLoader.GetTileWalkable(x, y) != 1) continue;

                    if (closedSet.Any(n => n.tile.x == x && n.tile.y == y)) continue;

                    var tileNode = openSet.FirstOrDefault(n => n.tile.x == x && n.tile.y == y);
                    if (tileNode == null)
                    {
	                    tileNode = new TileNode();
	                    tileNode.tile = new LevelLoader.TilePoint(x, y);
	                    tileNode.parent = currentNode;
	                    tileNode.G = CalculateG(currentNode.G, currentNode.tile, tileNode.tile);
	                    tileNode.H = CalculateH(tileNode.tile, to);
	                    tileNode.F = CalculateF(tileNode.G, tileNode.H);
						openSet.Add(tileNode);
                    }
                    else
                    {
						var tempG = CalculateG(currentNode.G, currentNode.tile, tileNode.tile);
						if (tempG < tileNode.G)
						{
							tileNode.parent = currentNode;
							tileNode.G = tempG;
							tileNode.H = CalculateH(tileNode.tile, to);
							tileNode.F = CalculateF(tileNode.G, tileNode.H);
						}
					}
                }
            }
        }

        var resultPath = new List<LevelLoader.TilePoint>();
		if (arrive)
		{
			resultPath.Add(currentNode.tile);
	        while (currentNode.parent != null)
	        {
		        currentNode = currentNode.parent;
				resultPath.Add(currentNode.tile);
	        }
	        resultPath.Reverse();
		}

        return resultPath;
    }

    private int CalculateG(int originalG, LevelLoader.TilePoint oldPosition, LevelLoader.TilePoint currentPosition)
    {
	    if (currentPosition.x != oldPosition.x &&
	        currentPosition.y != oldPosition.y)
	    {
		    return originalG + 14;
	    }

	    return originalG + 10;
    }

    private int CalculateH(LevelLoader.TilePoint currentPosition, LevelLoader.TilePoint destinationPosition)
    {
		// Manhattan distance
		var xDiff = Mathf.Abs(destinationPosition.x - currentPosition.x);
		var yDiff = Mathf.Abs(destinationPosition.y - currentPosition.y);
		return (int)Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff) * 10;
	}

    private int CalculateF(int G, int H)
    {
	    return G + H;
    }
}
