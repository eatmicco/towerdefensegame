using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStarPathfinder : MonoBehaviour {

    public LevelLoader levelLoader;
    public int tileX = -1;
    public int tileY = -1;

    //scoring system
    //G = movement cost from start point to current point (1 per tile for adjacent, 1.5 per tile for diagonal)
    //H = estimated cost from current to end (use "Manhattan Length")

    public class TileScore
    {
        public LevelLoader.TilePoint tile;
        public int F;
        public int H;
        public int G;
    }

    //open set
    private List<TileScore> openSet_ = new List<TileScore>();
    private List<TileScore> closedSet_ = new List<TileScore>();

    public List<LevelLoader.TilePoint> FindPath(LevelLoader.TilePoint from, LevelLoader.TilePoint to)
    {
        openSet_.Clear();
        closedSet_.Clear();

        tileX = levelLoader.GetTileX();
        tileY = levelLoader.GetTileY();

        List<LevelLoader.TilePoint> resultPath = new List<LevelLoader.TilePoint>();

        LevelLoader.TilePoint current = from;

        TileScore tileScore = new TileScore();
        tileScore.tile = from;
        tileScore.G = 0;
        tileScore.H = Mathf.Abs(to.x - from.x) + Mathf.Abs(to.y - from.y);
        tileScore.F = tileScore.G + tileScore.H;
        closedSet_.Add(tileScore);
        resultPath.Add(from);

        Debug.Log("from (" + from.x + ", " + from.y + ") and to (" + to.x + ", " + to.y + ")");
        bool arrive = false;
        while (current != to)
        //for (int k = 0; k < 20; ++k)
        {
            //Debug.Log("K : " + k);
            int G, H, F;
            //list scores of adjacent tiles from current
            for (int x = current.x - 1; x < current.x + 2; ++x)
            {
                if (x < 0 || x > tileX-1) continue;

                for (int y = current.y - 1; y < current.y + 2; ++y)
                {
                    if (x == current.x && y == current.y) continue;

                    if (y < 0 || y > tileY-1) continue;

                    if (x == current.x - 1 && y == current.y - 1) continue;
                    if (x == current.x + 1 && y == current.y - 1) continue;
                    if (x == current.x - 1 && y == current.y + 1) continue;
                    if (x == current.x + 1 && y == current.y + 1) continue;

                    if (levelLoader.GetTileWalkable(x, y) == 1)
                    {
                        G = (Mathf.Abs(from.x - x) + Mathf.Abs(from.y - y));
                        int xd = Mathf.Abs(to.x - x);
                        int yd = Mathf.Abs(to.y - y);
                        if (xd > yd)
                            H = 14 * yd + 10 * (xd - yd);
                        else
                            H = 14 * xd + 10 * (yd - xd);
                        F = G + H;
                        //add to open set
                        tileScore = new TileScore();
                        tileScore.tile = new LevelLoader.TilePoint(x, y);
                        tileScore.G = G;
                        tileScore.H = H;
                        tileScore.F = F;
                        openSet_.Add(tileScore);
                        Debug.Log("F : " + F + "; G : " + G + "; H : " + H);
                    }
                }
            }

            //find the lowest score and add it to closed set
            int lowest = 1000;
            int lowestId = -1;
            for (int i = 0; i < openSet_.Count; ++i)
            {
                if (openSet_[i].F < lowest)
                {
                    //check if the tile is close to current tile
                    if ((Mathf.Abs(openSet_[i].tile.x - current.x) == 1 &&
                        Mathf.Abs(openSet_[i].tile.y - current.y) == 0) ||
                        (Mathf.Abs(openSet_[i].tile.x - current.x) == 0 &&
                        Mathf.Abs(openSet_[i].tile.y - current.y) == 1))
                    {

                        //check if the tile is already inside the closed set
                        bool inside = false;
                        for (int j = 0; j < closedSet_.Count; ++j)
                        {
                            if (closedSet_[j].tile.x == openSet_[i].tile.x &&
                                closedSet_[j].tile.y == openSet_[i].tile.y)
                            {
                                inside = true;
                                break;
                            }
                        }

                        if (!inside)
                        {
                            lowest = openSet_[i].F;
                            lowestId = i;
                        }
                    }
                }
                //check if the destination is in open set
                if (openSet_[i].tile.x == to.x && openSet_[i].tile.y == to.y)
                {
                    arrive = true;
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
                //add to result path
                resultPath.Add(current);
            }

            if (arrive) break;
        }

        Debug.Log("Path Count : " + resultPath.Count);
        for (int i = 0; i < resultPath.Count; ++i)
        {
            Debug.Log("Point " + resultPath[i].x + ", " + resultPath[i].y);
        }
        return resultPath;
    }
}
