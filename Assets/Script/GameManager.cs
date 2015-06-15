using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public int wave = 0;
    public bool waveStarted = false;
    public LevelLoader levelLoader;

    public static GameManager instance;

    public void IncrementWave()
    {
        wave++;
    }

    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        if (GUILayout.Button("Start"))
        {
            waveStarted = true;
        }
        if (GUILayout.Button("Path"))
        {
            levelLoader.DrawWalkingPath();
        }
    }
}
