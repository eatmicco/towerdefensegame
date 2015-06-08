using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerManager : MonoBehaviour {

    private List<TowerProperties> towers_ = new List<TowerProperties>();

    public void AddTower(TowerProperties tower)
    {
        towers_.Add(tower);
    }

    private void UpdateTowers()
    {

    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (!GameManager.instance.waveStarted)
        {
            return;
        }
	}
}
