using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World{

	public static float FallSpeed = 5f;
	public static float PushSpeed = 2.5f;
	public static float LiftSpeed = 2.5f;
}

public class Level : MonoBehaviour 
{

    private ILevelThing[] things;

    [SerializeField] //<--- TODO!
    private List<WizzPlayer> players;

    [SerializeField] private Transform[] startPositions;


    void Awake(){
        //objects = GetComponentsInChildren<LevelObject>();
        //tiles = GetComponentsInChildren<LevelTile>();
        things = GetComponentsInChildren<ILevelThing>();


        //      foreach (var item in objects) {

        //}
        //foreach (var thing in things)
        //{
                
        //}
    }

    void Start()
    {
        StartLevel();
    }



    public void StartLevel()
    {
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            player.transform.position = startPositions[i].position;
			player.Reset();
        }
    }

	public void WonLevel(){
		Debug.Log("WON!!!!!!!!!!!!!!");
	}


    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
#endif
    }



    private void RestartLevel()
    {
        foreach (var thing in things)
        {
            thing.Reset();
        }
        StartLevel();
    }
}
