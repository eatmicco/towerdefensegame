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
    public AStarPathfinder pathFinder;
    public MinionProperties[] minionPrefabs;
    public float timeToSpawn;
    public float minionThresholdPath;
    public TextAsset minionDesignTA;
    public float maxVelocity;
    public float maxAvoidForce;
    public List<Transform> obstacles;
    public float obstacleRadius;
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
            levelDesign_.Add(minionLevelDesign);
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
                    minionProp.currentTile = levelLoader.GetPathPoint(0);
                    minionProp.pathIndex = 1;
                    minionProp.walkPath = pathFinder.FindPath(minionProp.currentTile, levelLoader.GetPathPoint(minionProp.pathIndex));
                    minionProp.walkPathIndex = 1;
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
            Vector3 dest = levelLoader.GetTilePosition(minionProp.walkPath[minionProp.walkPathIndex]);
            Vector3 diff = dest - pos;
            if (diff.magnitude < minionThresholdPath)
            {
                minionProp.walkPathIndex += 1;
                if (minionProp.walkPathIndex == minionProp.walkPath.Count)
                {
                    minionProp.walkPathIndex = 0;

                    minionProp.pathIndex += 1;
                    if (minionProp.pathIndex == levelLoader.GetPathCount())
                        continue;

                    minionProp.walkPath = pathFinder.FindPath(minionProp.currentTile, levelLoader.GetPathPoint(minionProp.pathIndex));
                    if (minionProp.walkPath.Count <= 0)
                        continue;
                }
                else
                {
                    minionProp.currentTile = minionProp.walkPath[minionProp.walkPathIndex];
                }
            }

            Vector3 moveVector;
            Quaternion rotation;
            Seek(pos, dest, minionThresholdPath, minionProp.speed, minionProp.rotationSpeed, 
                minions_[i].transform.rotation, out moveVector, out rotation);

            Debug.Log(i + " rotation : " + rotation);

            minions_[i].transform.rotation = rotation;
            minions_[i].transform.position += moveVector;
        }
    }

    private Vector3 Truncate(Vector3 v, float max)
    {
        float i = v.magnitude != 0 ? max / v.magnitude : 0;
        if (i < 1)
        {
            v *= i;
        }

        return v;
    }

    private Transform FindMostThreateningObstacle(Vector3 ahead, Vector3 ahead2)
    {
        Transform mostThreatening = null;

        for (int i = 0; i < obstacles.Count; ++i)
        {
            bool collision = Vector3.Distance(obstacles[i].position, ahead) <= obstacleRadius ||
                Vector3.Distance(obstacles[i].position, ahead2) <= obstacleRadius;

            if (collision && (mostThreatening == null || Vector3.Distance(transform.position, obstacles[i].position) < Vector3.Distance(transform.position, mostThreatening.position)))
            {
                mostThreatening = obstacles[i];
            }
        }

        return mostThreatening;
    }

    private Vector3 CollisionAvoidance(Vector3 position, Vector3 moveVector)
    {
        float dynamic_length = moveVector.magnitude / maxVelocity;
        Vector3 ahead = position + moveVector.normalized * dynamic_length;
        Vector3 ahead2 = ahead * 0.5f;

        Transform mostThreatening = FindMostThreateningObstacle(ahead, ahead2);
        Vector3 avoidance = new Vector3(0, 0, 0);

        if (mostThreatening != null)
        {
            avoidance.x = ahead.x - mostThreatening.position.x;
            avoidance.y = ahead.y - mostThreatening.position.y;

            avoidance.Normalize();
            avoidance *= maxAvoidForce;
            Debug.Log(avoidance);
        }
        else
        {
            avoidance *= 0;//nullify the avoidance force
        }

        return avoidance;
    }

    private void Seek(Vector3 position, Vector3 target, float minDistance, float moveSpeed, float rotationSpeed, Quaternion oldRotation,
        out Vector3 moveVector, out Quaternion rotation)
    {
        Vector3 direction = target - position;

        moveVector = direction;

        if (direction.magnitude > minDistance)
        {
            moveVector = direction.normalized * moveSpeed * Time.deltaTime;

            Truncate(moveVector, maxVelocity);

            Vector3 avoidance = CollisionAvoidance(position, moveVector);

            moveVector = (moveVector.normalized + avoidance) * moveSpeed * Time.deltaTime;
        }

        float angle = Mathf.Atan2(moveVector.y, moveVector.x) * (180 / Mathf.PI);

        rotation = Quaternion.Slerp(oldRotation, Quaternion.Euler(0, 0, angle - 90), rotationSpeed * Time.deltaTime);
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
        LoadMinionDesign(minionDesignTA.text);
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
