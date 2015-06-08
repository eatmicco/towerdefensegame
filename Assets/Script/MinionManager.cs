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
        
    }

    private MinionProperties SpawnMinion(int id)
    {
        MinionProperties minion = (MinionProperties)Instantiate(minionPrefabs[id]);
        
        return minion;
    }

    private void UpdateMinions()
    {
        if (tick_ > timeToSpawn)
        {
            tick_ = 0;
            int wave = GameManager.instance.wave;
            if (currentMinionCount_ < levelDesign_[0].minionWaves[wave].minionMap[currentMinionId_].count)
            {
                int minionId = levelDesign_[0].minionWaves[wave].minionMap[currentMinionId_].id;
                GameObject minionGO = (GameObject)Instantiate(minionPrefabs[minionId].gameObject);
                Vector3 pos = levelLoader.GetPathPosition(0);
                minionGO.transform.position = pos;
                minions_.Add(minionGO);
                MinionProperties minionProp = minionGO.GetComponent<MinionProperties>();
                minionProp.pathIndex = 0;
            }
            else
            {
                currentMinionCount_ = 0;
                currentMinionId_++;
                //skip this frame
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
	}
}
