using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class MinionManager : MonoBehaviour {

    public class MinionMap
    {
        public int id;
        public int count;
    }

    public class MinionWaveDesign
    {
        public int wave;
        public List<MinionMap> minionMap;
    }

    public class MinionLevelDesign
    {
        public int id;
        public List<MinionWaveDesign> minionWaves;
    }

    public LevelLoader levelLoader;
    public MinionProperties[] minionPrefabs;
    public float timeToSpawn;
    public float minionThresholdPath;
    private List<MinionLevelDesign> levelDesign_ = new List<MinionLevelDesign>();
    private List<GameObject> minions_ = new List<GameObject>();
    private float tick_;
    private int currentMinionId_ = 0;
    private int currentMinionCount_ = 0;

    private void LoadMinionDesign(string data)
    {
        JSONNode node = JSON.Parse(data);
        JSONArray minionDesignArr = node["minion"].AsArray;
        for (int i = 0; i < minionDesignArr.Count; ++i)
        {
            MinionLevelDesign minionLevelDesign = new MinionLevelDesign();
            minionLevelDesign.id = i;
            minionLevelDesign.minionWaves = new List<MinionWaveDesign>();
            JSONArray waveDesignArr = minionDesignArr[i]["waves"].AsArray;
            for (int j = 0; j < waveDesignArr.Count; ++j)
            {
                MinionWaveDesign minionWaveDesign = new MinionWaveDesign();
                minionWaveDesign.wave = waveDesignArr[j]["wave"].AsInt;
                minionWaveDesign.minionMap = new List<MinionMap>();
                JSONArray minionListArr = waveDesignArr[j]["minion_list"].AsArray;
                for (int k = 0; k < minionListArr.Count; ++k)
                {
                    MinionMap minionMap = new MinionMap();
                    minionMap.id = minionListArr[k]["minion_id"].AsInt;
                    minionMap.count = minionListArr[k]["minion_count"].AsInt;
                    minionWaveDesign.minionMap.Add(minionMap);
                }
                minionLevelDesign.minionWaves.Add(minionWaveDesign);
            }
        }

    }

    private void UpdateMinions()
    {
        if (tick_ > timeToSpawn)
        {
            tick_ = 0;
            int wave = GameManager.instance.wave;
            if (currentMinionId_ < levelDesign_[0].minionWaves[wave].minionMap.Count)
            {
                if (currentMinionCount_ < levelDesign_[0].minionWaves[wave].minionMap[currentMinionId_].count)
                {
                    int minionId = levelDesign_[0].minionWaves[wave].minionMap[currentMinionId_].id;
                    GameObject minionGO = (GameObject)Instantiate(minionPrefabs[minionId].gameObject);
                    Vector3 pos = levelLoader.GetPathPosition(0);
                    minionGO.transform.position = pos;
                    minions_.Add(minionGO);
                    MinionProperties minionProp = minionGO.GetComponent<MinionProperties>();
                    minionProp.pathIndex = 0;
                    currentMinionCount_++;
                }
                else
                {
                    currentMinionCount_ = 0;
                    currentMinionId_++;
                    //skip this frame
                }
            }
        }
        tick_ += Time.deltaTime;

        for (int i = 0; i < minions_.Count; ++i)
        {
            MinionProperties minionProp = minions_[i].GetComponent<MinionProperties>();

            if (minionProp.pathIndex == levelLoader.GetPathCount())
                continue;

            Vector3 pos = minions_[i].transform.position;
            Vector3 dest = levelLoader.GetPathPosition(minionProp.pathIndex);
            Vector3 diff = dest - pos;
            if (diff.magnitude < minionThresholdPath)
            {
                minionProp.pathIndex += 1;
                if (minionProp.pathIndex == levelLoader.GetPathCount())
                    continue;

                dest = levelLoader.GetPathPosition(minionProp.pathIndex);
                diff = dest - pos;

                //rotate to target
                float dx = dest.x - pos.x;
                float dy = dest.y - pos.y;
                float angle = Mathf.Atan2(dy, dx) * (180 / Mathf.PI);
                Debug.Log(angle);

                minions_[i].transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            }

            diff.Normalize();
            minions_[i].transform.Translate(diff * (minionProp.speed * Time.deltaTime), Space.World);
        }
    }

    public GameObject GetMinionInsideTowerRadius(Vector3 position, float radius)
    {
        for (int i = 0; i < minions_.Count; ++i)
        {
            if (Vector3.Distance(position, minions_[i].transform.position) < radius)
            {
                return minions_[i];
            }
        }

        return null;
    }

    public bool CheckMinionCollisionWithBullet(Vector3 bulletPos, DamageType damageType, float damage)
    {
        for (int i = 0; i < minions_.Count; ++i)
        {
            BoxCollider2D collider = minions_[i].GetComponent<BoxCollider2D>();
            if (collider.bounds.Contains(bulletPos))
            {
                MinionProperties minionProp = minions_[i].GetComponent<MinionProperties>();
                float percentage = 100;
                switch (minionProp.armorType)
                {
                    case ArmorType.UNARMED:
                        {
                            switch (damageType)
                            {
                                case DamageType.NORMAL: percentage = 100; break;
                                case DamageType.PIERCE: percentage = 200; break;
                                case DamageType.SIEGE: percentage = 150; break;
                                case DamageType.MAGIC: percentage = 100; break;
                                case DamageType.CHAOS: percentage = 100; break;
                            }
                        }
                        break;
                    case ArmorType.LIGHT:
                        {
                            switch (damageType)
                            {
                                case DamageType.NORMAL: percentage = 100; break;
                                case DamageType.PIERCE: percentage = 150; break;
                                case DamageType.SIEGE: percentage = 100; break;
                                case DamageType.MAGIC: percentage = 125; break;
                                case DamageType.CHAOS: percentage = 100; break;
                            }
                        }
                        break;
                    case ArmorType.MEDIUM:
                        {
                            switch (damageType)
                            {
                                case DamageType.NORMAL: percentage = 150; break;
                                case DamageType.PIERCE: percentage = 75; break;
                                case DamageType.SIEGE: percentage = 50; break;
                                case DamageType.MAGIC: percentage = 75; break;
                                case DamageType.CHAOS: percentage = 100; break;
                            }
                        }
                        break;
                    case ArmorType.HEAVY:
                        {
                            switch (damageType)
                            {
                                case DamageType.NORMAL: percentage = 100; break;
                                case DamageType.PIERCE: percentage = 100; break;
                                case DamageType.SIEGE: percentage = 100; break;
                                case DamageType.MAGIC: percentage = 200; break;
                                case DamageType.CHAOS: percentage = 100; break;
                            }
                        }
                        break;
                    case ArmorType.FORTIFIED:
                        {
                            switch (damageType)
                            {
                                case DamageType.NORMAL: percentage = 70; break;
                                case DamageType.PIERCE: percentage = 35; break;
                                case DamageType.SIEGE: percentage = 150; break;
                                case DamageType.MAGIC: percentage = 35; break;
                                case DamageType.CHAOS: percentage = 100; break;
                            }
                        }
                        break;
                }
                minionProp.hp -= (damage * (percentage/100)) - minionProp.armor;
                if (minionProp.hp <= 0)
                {
                    minionProp.hp = 0;
                    Destroy(minions_[i]);
                    minions_.RemoveAt(i);
                }
                return true;
            }
        }

        return false;
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
        UpdateMinions();
	}
}
