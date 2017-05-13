using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelHelper {

	public static Vector2 RoundToTilePos(Vector2 pos){
		var tilePos = pos;
//		tilePos -= Vector2.one * 0.5f;
		tilePos.x = (int) tilePos.x + (tilePos.x < 0 ? -1 : 0);
		tilePos.y = (int) tilePos.y + (tilePos.y < 0 ? -1 : 0);
		tilePos += Vector2.one * 0.5f;
		return tilePos;
	}



	public static Collider2D ClosestCollider(Vector2 pos, Collider2D[] colliders)
	{
		Collider2D closestColl = null;
		float closestDist = float.MaxValue;
		for (int i = 0; i < colliders.Length; i++) {
			var coll = colliders[i];
			if (coll == null) continue;
			float dist = Vector2.Distance(pos, coll.transform.position);
			if (dist < closestDist){
				closestDist = dist;
				closestColl = coll;
			}
		}
		return closestColl;
	}
}
