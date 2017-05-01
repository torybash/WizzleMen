using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;

public class LevelObject : MonoBehaviour {


	[SerializeField] private LevelObjectStats stats;

	[SerializeField] private Collider2D groundColl;
	[SerializeField] ContactFilter2D groundFilter;


	public LevelObjectStats Stats { get { return stats; }}

	Coroutine routine;


	Collider2D[] groundColliders = new Collider2D[1];


	void Start(){
//		groundFilter = new ContactFilter2D();
//		groundFilter.SetDepth
	}

	void FixedUpdate(){
		if (routine != null) return;

		if (stats.usesGravity){
			if (groundColl.OverlapCollider(groundFilter, groundColliders) == 0){
				routine = StartCoroutine(AnimateFall());
			}
//			Debug.Log("colliders: " + groundColliders.Length);
		}
	}
		
	public bool CanBePushed(float xDir){
		//TODO!
//		if (Physics2D.OverlapPoint(transform.position + Vector3.right * xDir))
		return routine == null && Physics2D.OverlapPoint(transform.position + Vector3.right * xDir) == null;
	}

	public void StartPush(float xDir){
		routine = StartCoroutine(AnimatePush(Vector3.right * xDir));
	}

	private IEnumerator AnimateFall()
	{
		var startPos = transform.position;
		var goalPos = transform.position + Vector3.down;
		var waitFixed = new WaitForFixedUpdate();
		float t = 0;
		while (t < 1){
			t = Mathf.Clamp01(t + Time.fixedDeltaTime * World.FallSpeed);
			transform.position = Vector2.Lerp(startPos, goalPos, t);
			yield return waitFixed;
		}
		routine = null;
	}

	private IEnumerator AnimatePush(Vector3 dir)
	{
		var startPos = transform.position;
		var goalPos = transform.position + dir;
		var waitFixed = new WaitForFixedUpdate();
		float t = 0;
		while (t < 1){
			t = Mathf.Clamp01(t + Time.fixedDeltaTime * World.PushSpeed);
			transform.position = Vector2.Lerp(startPos, goalPos, t);
			yield return waitFixed;
		}
		routine = null;
	}
	
}

[Serializable]
public class LevelObjectStats{
	public bool isPushable;
	public bool usesGravity;
}