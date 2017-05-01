using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World{

	public static float FallSpeed = 5f;
	public static float PushSpeed = 2.5f;
}

public class Level : MonoBehaviour {

	private LevelObject[] objects;
	private LevelTile[] tiles;

	void Awake(){
		objects = GetComponentsInChildren<LevelObject>();
		tiles = GetComponentsInChildren<LevelTile>();


		foreach (var item in objects) {

		}
		foreach (var item in tiles) {

		}
	}

}
