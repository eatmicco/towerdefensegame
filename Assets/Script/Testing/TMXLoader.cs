using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TMXLoader : MonoBehaviour {

    public int pixelPerUnit = 100;
    public TextAsset tmxTA;
    public GameObject[] tileSprites;
    private int tileX_ = 0;
    private int tileY_ = 0;
    private float tileSize_;
    private List<int> map = new List<int>();
    private List<Vector3> posList = new List<Vector3>();

    private void LoadTMXData(string textData)
    {
        var parser = new XMLParser();
        var node = parser.Parse(textData);
        int width, height, tileSize;
        int.TryParse(node.GetValue("map>0>@width"), out width);
        int.TryParse(node.GetValue("map>0>@height"), out height);
        int.TryParse(node.GetValue("map>0>@tilewidth"), out tileSize);
        Debug.Log("Width : " + width + "; Height : " + height);
        tileX_ = width;
        tileY_ = height;
        tileSize_ = (float)tileSize / (float)pixelPerUnit;

        Debug.Log(node.GetValue("map>0>layer>0>data>0>tile>0>@gid"));

        for (int i = 0; i < (width * height); ++i)
        {
            int tile;
            int.TryParse(node.GetValue("map>0>layer>0>data>0>tile>" + i + ">@gid"), out tile);
            map.Add(tile);
        }
    }

    private void PopulateMap()
    {
        float halfX = (tileSize_ * tileX_) / 2;
        float halfY = (tileSize_ * tileY_) / 2;

        float posX;
        float posY = halfY - (tileSize_ / 2);

        for (int y = 0; y < tileY_; ++y)
        {
            posX = 0 - (halfX - (tileSize_ / 2));
            for (int x = 0; x < tileX_; ++x)
            {
                int idx = x + (tileX_ * y);
                int val = map[idx]-1;
                Debug.Log(val);
                if (val >= 0)
                {
                    GameObject spriteGO = (GameObject)Instantiate(tileSprites[val]);
                    spriteGO.name = "tile " + x + "_" + y;
                    spriteGO.transform.parent = transform;
                    spriteGO.transform.position = new Vector3(posX, posY, 0);

                    posList.Add(spriteGO.transform.localPosition);
                }
                posX += tileSize_;
            }
            posY -= tileSize_;
        }
    }

    // Use this for initialization
    void Start()
    {
        LoadTMXData(tmxTA.text);
        PopulateMap();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
