using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class LevelLoader : MonoBehaviour {

    public class TilePoint
    {
        public TilePoint(int x, int y) { this.x = x; this.y = y; }
        public int x;
        public int y;
    }

    public class LevelDesign
    {
        public int tileX;
        public int tileY;
        public List<int> map;
        public List<int> placeable;
        public List<int> walkable;
        public List<TilePoint> path;
    }

    public int pixelsPerUnit;
    public TextAsset levelDesignTA;
    public int levelId;
    public GameObject[] tileSprites;
    public AStarPathfinder pathFinder;
    public GameObject pathSprite;
    private List<LevelDesign> levelDesignList_ = new List<LevelDesign>();
    private List<Vector3> positionList_ = new List<Vector3>();
    private List<GameObject> tileSpriteList_ = new List<GameObject>();
    private List<GameObject> walkingPath_ = new List<GameObject>();
    private int tileX_;
    private int tileY_;

    private void LoadLevelDesigns(string data)
    {
        levelDesignList_.Clear();

        JSONNode node = JSON.Parse(data);
        JSONArray levelArr = node["level"].AsArray;
        for (int i = 0; i < levelArr.Count; ++i)
        {
            LevelDesign level = new LevelDesign();
            level.tileX = levelArr[i]["tileX"].AsInt;
            level.tileY = levelArr[i]["tileY"].AsInt;
            level.map = new List<int>();
            JSONArray mapArr = levelArr[i]["map"].AsArray;
            for (int j = 0; j < mapArr.Count; ++j)
            {
                level.map.Add(mapArr[j].AsInt);
            }
            level.placeable = new List<int>();
            JSONArray placeableArr = levelArr[i]["placeable"].AsArray;
            for (int j = 0; j < placeableArr.Count; ++j)
            {
                level.placeable.Add(placeableArr[j].AsInt);
            }
            level.walkable = new List<int>();
            JSONArray walkableArr = levelArr[i]["walkable"].AsArray;
            for (int j = 0; j < walkableArr.Count; ++j)
            {
                level.walkable.Add(walkableArr[j].AsInt);
            }
            level.path = new List<TilePoint>();
            JSONArray pathArr = levelArr[i]["path"].AsArray;
            for (int j = 0; j < pathArr.Count; ++j)
            {
                JSONArray pointArr = pathArr[j].AsArray;
                level.path.Add(new TilePoint(pointArr[0].AsInt, pointArr[1].AsInt));
            }

            levelDesignList_.Add(level);
        }
    }

    private void PopulateLevel(int index)
    {
        if (tileSprites.Length == 0)
            return;

        if (tileSprites[0] == null)
            return;

        GameObject spritePrefabGO = tileSprites[0].transform.GetChild(0).gameObject;

        float tileSize = (float)spritePrefabGO.GetComponent<SpriteRenderer>().sprite.texture.width / (float)pixelsPerUnit;

        positionList_.Clear();
        tileSpriteList_.Clear();
        LevelDesign design = levelDesignList_[index];
        tileX_ = design.tileX;
        tileY_ = design.tileY;

        float halfX = (tileSize * tileX_) / 2;
        float halfY = (tileSize * tileY_) / 2;

        float posX;
        float posY = halfY - (tileSize/2);
        for (int y = 0; y < tileY_; ++y)
        {
            posX = 0 - (halfX - (tileSize/2));
            for (int x = 0; x < tileX_; ++x)
            {
                int idx = x + (tileX_ * y);
                int val = design.map[idx];
                if (val >= 0)
                {
                    GameObject spriteGO = (GameObject)Instantiate(tileSprites[val]);
                    spriteGO.name = "tile " + x + "_" + y;
                    spriteGO.transform.parent = transform;
                    spriteGO.transform.position = new Vector3(posX, posY, 0);
                    tileSpriteList_.Add(spriteGO);
                    positionList_.Add(spriteGO.transform.localPosition);
                }
                posX += tileSize;
            }
            posY -= tileSize;
        }

        for (int i = 0; i < GetPathCount(); ++i)
        {
            GameObject pathTileGO = Instantiate(pathSprite) as GameObject;
            Vector3 pos = GetPathPosition(i);
            pos.z = -1;
            pathTileGO.transform.position = pos;
        }
    }

    public int GetTileX() { return tileX_; }

    public int GetTileY() { return tileY_; }

    public Vector3 GetTilePosition(TilePoint tilePoint)
    {
        return GetTilePosition(tilePoint.x, tilePoint.y);
    }

    public Vector3 GetTilePosition(int x, int y)
    {
        return positionList_[x + (y * tileX_)];
    }

    public TilePoint GetTileFromPosition(Vector3 pos)
    {
        for (int y = 0; y < tileY_; ++y)
        {
            for (int x = 0; x < tileX_; ++x)
            {
                int idx = x + (y * tileX_);
                if (tileSpriteList_[idx].GetComponent<BoxCollider2D>().OverlapPoint(pos))
                {
                    return new TilePoint(x, y);
                }
            }
        }

        return new TilePoint(-1, -1);
    }

    public int GetTileValue(int x, int y)
    {
        int idx = x + (y * tileX_);
        return levelDesignList_[levelId].map[idx];
    }

    public int GetTilePlaceable(int x, int y)
    {
        int idx = x + (y * tileY_);
        return levelDesignList_[levelId].placeable[idx];
    }

    public int GetTileWalkable(int x, int y)
    {        
        int idx = x + (y * tileY_);
        return levelDesignList_[levelId].walkable[idx];
    }

    public TilePoint GetPathPoint(int idx)
    {
        return levelDesignList_[levelId].path[idx];
    }

    public Vector3 GetPathPosition(int idx)
    {
        TilePoint point = levelDesignList_[levelId].path[idx];
        return GetTilePosition(point.x, point.y);
    }

    public int GetPathCount()
    {
        return levelDesignList_[levelId].path.Count;
    }

    public void DrawWalkingPath()
    {
        for (int i = 0; i < walkingPath_.Count; ++i)
        {
            Destroy(walkingPath_[i]);
        }
        walkingPath_.Clear();

        for (int i = 0; i < GetPathCount()-1; ++i)
        {
            TilePoint from = GetPathPoint(i);
            TilePoint to = GetPathPoint(i + 1);
            List<TilePoint> path = pathFinder.FindPath(from, to);
            for (int j = 0; j < path.Count; ++j)
            {
                GameObject pathTileGO = Instantiate(pathSprite) as GameObject;
                GameObject child = pathTileGO.transform.GetChild(0).gameObject;
                child.GetComponent<TextMesh>().text = j.ToString();
                Vector3 pos = GetTilePosition(path[j]);
                pos.z = -1;
                pathTileGO.transform.position = pos;
                walkingPath_.Add(pathTileGO);
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        LoadLevelDesigns(levelDesignTA.text);
        PopulateLevel(levelId);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
