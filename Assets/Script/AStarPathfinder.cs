using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStarPathfinder : MonoBehaviour {

    public LevelLoader levelLoader;
    public int tileX;
    public int tileY;

    //scoring system
    //G = movement cost from start point to current point (1 per tile for adjacent, 1.5 per tile for diagonal)
    //H = estimated cost from current to end (use "Manhattan Length")

    public class TileScore
    {
        public LevelLoader.TilePoint tile;
        public int score;
    }

    //open set
    private List<TileScore> openSet_ = new List<TileScore>();
    private List<TileScore> closedSet_ = new List<TileScore>();

    private List<LevelLoader.TilePoint> FindPath(LevelLoader.TilePoint from, LevelLoader.TilePoint to)
    {
        LevelLoader.TilePoint current = from;

        while (current != to)
        {
            int G, H, F;
            //list scores of adjacent tiles from current
            for (int y = current.y - 1; y < current.y + 2; ++y)
            {
                for (int x = current.x - 1; x < current.x + 2; ++x)
                {
                    if (x == current.x && y == current.y) continue;

                    if (levelLoader.GetTileWalkable(x, y) == 1)
                    {
                        G = Mathf.Abs(from.x - x) + Mathf.Abs(from.y - y);
                        H = Mathf.Abs(to.x - x) + Mathf.Abs(to.y - y);
                        F = G + H;
                        //add to open set
                        TileScore tileScore = new TileScore();
                        tileScore.tile = new LevelLoader.TilePoint(x, y);
                        tileScore.score = F;
                        openSet_.Add(tileScore);
                    }
                }
            }

            //find the lowest score and add it to closed set
            int lowest = 100;
            int lowestId = -1;
            for (int i = 0; i < openSet_.Count; ++i)
            {
                if (openSet_[i].score < lowest)
                {
                    //check if the tile is close to current tile
                    if (Mathf.Abs(openSet_[i].tile.x = current.x) == 1 ||
                        Mathf.Abs(openSet_[i].tile.y = current.y) == 1)
                    {
                        lowest = openSet_[i].score;
                        lowestId = i;
                    }
                }
                //check if the destination is in open set
                if (openSet_[i].tile.x == to.x && openSet_[i].tile.y == to.y)
                {
                    lowestId = i;
                    break;
                }
            }
            if (lowestId != -1)
            {
                //add to closed set
                closedSet_.Add(openSet_[lowestId]);
                //set current value
                current = openSet_[lowestId].tile;
                //remove it from open set
                openSet_.RemoveAt(lowestId);
            }
        }

        return null;
    }
}
