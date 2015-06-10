using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerManager : MonoBehaviour {

    public LevelLoader levelLoader;
    public MinionManager minionManager;
    public BulletManager bulletManager;
    public GameObject[] towerPrefabs;
    private List<GameObject> towers_ = new List<GameObject>();

    public void AddTower(int id, Vector3 position)
    {
        LevelLoader.TilePoint tile = levelLoader.GetTileFromPosition(position);
        if (tile.x != -1 && tile.y != -1)
        {
            if (levelLoader.GetTilePlaceable(tile.x, tile.y) == 1)
            {
                GameObject tower = (GameObject)Instantiate(towerPrefabs[id]);
                Vector3 snapPos = levelLoader.GetTilePosition(tile.x, tile.y);
                tower.transform.position = snapPos;
                TowerProperties towerProp = tower.GetComponent<TowerProperties>();
                towerProp.tickToShoot = 0;
                towerProp.lockedMinion = null;
                towers_.Add(tower);
            }
        }
    }

    private void UpdateTowers()
    {
        for (int i = 0; i < towers_.Count; ++i)
        {
            TowerProperties towerProp = towers_[i].GetComponent<TowerProperties>();
            if (towerProp.lockedMinion == null)
            {
                towerProp.lockedMinion = minionManager.GetMinionInsideTowerRadius(towers_[i].transform.position, towerProp.towerRadius);
                if (towerProp.lockedMinion != null)
                    return;
            }
            else
            {
                if (Vector3.Distance(towers_[i].transform.position, towerProp.lockedMinion.transform.position) > towerProp.towerRadius)
                {
                    towerProp.lockedMinion = null;
                    return;
                }
                else
                {
                    Vector3 dest = towerProp.lockedMinion.transform.position;
                    Vector3 pos = towers_[i].transform.position;
                    float dx = dest.x - pos.x;
                    float dy = dest.y - pos.y;
                    float angle = Mathf.Atan2(dy, dx) * (180 / Mathf.PI);

                    towers_[i].transform.rotation = Quaternion.Euler(0, 0, angle - 90);
                }

                //tick to shoot
                if (towerProp.tickToShoot >= towerProp.timeToShoot)
                {
                    bulletManager.Shoot(towers_[i].transform.position, towers_[i].transform.rotation, towerProp.damageType, towerProp.damage);
                    towerProp.tickToShoot = 0;
                }
                towerProp.tickToShoot += Time.deltaTime;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (!GameManager.instance.waveStarted)
        {
            return;
        }

        UpdateTowers();
	}
}
