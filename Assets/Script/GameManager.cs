using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public int wave = 0;
    public bool waveStarted = false;
    public LevelLoader levelLoader;

    public static GameManager instance;

    private bool _showPath = false;

    public void IncrementWave()
    {
        wave++;
    }

    void Awake()
    {
        instance = this;
    }
	
	// Update is called once per frame
	void Update () {
        if (Time.frameCount % 30 == 0)
        {
            System.GC.Collect();
        }
	}

    void OnGUI()
    {
        if (GUILayout.Button("Start"))
        {
            waveStarted = true;
        }

        _showPath = GUILayout.Toggle(_showPath, _showPath ? "Hide Path" : "Show Path");
		if (_showPath)
        {
            levelLoader.DrawWalkingPath();
        }
		else
		{
			levelLoader.ClearWalkingPath();
		}
    }
}
