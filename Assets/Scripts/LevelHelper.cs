using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelHelper {

	public static Vector2 RoundToTilePos(Vector2 pos){
		var tilePos = pos;
		tilePos -= Vector2.one * 0.5f;
		tilePos.x = (int) tilePos.x;
		tilePos.y = (int) tilePos.y;
		tilePos += Vector2.one * 0.5f;
		return tilePos;
	}
}
