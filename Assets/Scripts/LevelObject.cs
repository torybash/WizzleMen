using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;

public class LevelObject : MonoBehaviour, ILevelThing
{


	[SerializeField] private LevelObjectStats stats;

	[SerializeField] private Collider2D groundColl;
	[SerializeField] ContactFilter2D groundFilter;
	[SerializeField] ContactFilter2D playerFilter;


	public LevelObjectStats Stats { get { return stats; }}

    public Vector3 OriginalPosition{
        get; set;
    }

    Coroutine routine;


	Collider2D[] colliders = new Collider2D[4];

    private Vector3 position;

	void Start(){
        OriginalPosition = transform.localPosition;
//		groundFilter = new ContactFilter2D();
//		groundFilter.SetDepth
    }

	void FixedUpdate(){
		if (routine != null) return;

		if (stats.usesGravity){
			if (groundColl.OverlapCollider(groundFilter, colliders) == 0){
				routine = StartCoroutine(AnimateFall());
			}
		}
	}


    public void Reset()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        transform.localPosition = OriginalPosition;
    }

    public bool CanBePushed(float xDir){
		return routine == null && Physics2D.OverlapPoint(transform.position + Vector3.right * xDir) == null;
	}

	public void StartPush(float xDir){
		routine = StartCoroutine(AnimatePush(Vector3.right * xDir));
	}

	public bool CanBeLifted(){
		return routine == null && Physics2D.OverlapPoint(transform.position + Vector3.up) == null;
	}

	public bool CanBePlaced(Vector3 pos){
//		Debug.Log("CanBePlaced - xDir: "+ xDir + ", overlap: "+ Physics2D.OverlapPoint(transform.position + Vector3.down + Vector3.right * xDir));
		return Physics2D.OverlapPoint(pos) == null;
	}

	private IEnumerator AnimateFall()
	{
		Debug.Log("AnimateFall");

		if (groundColl.OverlapCollider(playerFilter, colliders) > 0){
			foreach (var coll in colliders) {
				if (coll != null)
					coll.GetComponent<WizzPlayer>().ObjectSquish();
			}
		}
			
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
		Debug.Log("AnimatePush - dir: " + dir);

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
	public bool isLiftable;
	public bool usesGravity;
}