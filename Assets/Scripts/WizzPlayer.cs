using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class WizzPlayer : MonoBehaviour {



	[SerializeField] private float runSpeed;
	[SerializeField] private float jumpForce = 50f;
	[SerializeField, Range(0f,1f)] private float airControl = 0.5f;

	[SerializeField] private ContactFilter2D ladderFilter;

	[SerializeField] private Collider2D groundColl;
	[SerializeField] private ContactFilter2D groundFilter;

	[SerializeField] private Collider2D frontColl;
	[SerializeField] private ContactFilter2D frontFilter;

	[SerializeField] private int playerNum;
	
	private Collider2D selfColl;

	private Rigidbody2D body;

	private Vector2 move;
	private bool jumpPressed;
	private bool pushPressed;
	private bool lastLiftTapped;
	private bool liftTapped;

	private bool isGrounded;


	private Collider2D[] colliders = new Collider2D[1];

	[SerializeField] private string currentRoutineName;
	private Coroutine _routine;
	private Coroutine routine{
		get{ return _routine;}
		set{ _routine = value; currentRoutineName = value == null ? "NONE" : value.ToString();} 
	}

	private LevelObject liftedObject;

	WaitForFixedUpdate waitFixed = new WaitForFixedUpdate();

	void Awake(){
		selfColl = GetComponent<Collider2D>();
		body = GetComponent<Rigidbody2D>();
	}

	void Update () {
		var inputDevice = (InputManager.Devices.Count > playerNum) ? InputManager.Devices[playerNum] : null;

		if (inputDevice == null){	
			move.x = Input.GetAxisRaw("Horizontal");
			move.y = Input.GetAxisRaw("Vertical");
			jumpPressed = Input.GetKey(KeyCode.Space);
			pushPressed = Input.GetKey(KeyCode.Z);
			liftTapped = Input.GetKey(KeyCode.X);
		}else{
			move = inputDevice.Direction;
			jumpPressed = inputDevice.Action1;
			pushPressed = inputDevice.Action2;
			if (!lastLiftTapped && inputDevice.Action3) 
				liftTapped = true;
			lastLiftTapped = inputDevice.Action3;
		}
	}

	void FixedUpdate()
	{
		if (routine != null) return;

		//bool isNearLadder = selfColl.OverlapCollider(ladderFilter, colliders) > 0; //TODO!
		

		body.velocity = new Vector2(move.x * runSpeed * (isGrounded ? 1f : airControl), body.velocity.y);

		if (Mathf.Abs(body.velocity.x) > 0)
			transform.localScale = new Vector3(Mathf.Sign(body.velocity.x), transform.localScale.y, transform.localScale.z);

		if (isGrounded && jumpPressed && body.velocity.y == 0){
			body.AddForce(Vector2.up * jumpForce);
			jumpPressed = false;
		}

		bool hasFrontCollision = frontColl.OverlapCollider(frontFilter, colliders) > 0;
		

		float xDir = Mathf.Sign(transform.localScale.x);

		if (pushPressed){
			var obj = colliders[0].GetComponent<LevelObject>();
			if (isGrounded && hasFrontCollision && Mathf.Abs(body.velocity.x) > 0 && obj != null){
//					Debug.Log("Collider: "+ colliders[0] + ", xDir: "+ xDir);
				if (obj.Stats.isPushable && obj.CanBePushed(xDir)){
					obj.StartPush(xDir);
					routine = StartCoroutine(AnimatePushObject(xDir * Vector3.right));
				}
			}
		
		}else if (liftTapped){
			if (liftedObject == null){
				var obj = colliders[0].GetComponent<LevelObject>();
				if (isGrounded && obj != null && obj.Stats.isPushable && obj.CanBeLifted()){
					liftedObject = obj;
					routine = StartCoroutine(AnimateLiftObject());
				}
			}else{
				var goalPos = LevelHelper.RoundToTilePos((Vector2)this.transform.position + Vector2.right * xDir);
				if (liftedObject.CanBePlaced(goalPos)){
					routine = StartCoroutine(AnimatePlaceObject(goalPos));
				}
			}
		}

		liftTapped = false;

		isGrounded = groundColl.OverlapCollider(groundFilter, colliders) > 0;
	}


	private IEnumerator AnimateMoveToPos(Vector3 goalPos)
	{		
		Debug.Log("AnimateMoveToPos - goalPos: " + goalPos);

		float t = 0;
		while (this.transform.position != goalPos){
			this.transform.position = Vector2.MoveTowards(this.transform.position, goalPos, Time.fixedDeltaTime * runSpeed);
			yield return waitFixed;
		}
	}

	private IEnumerator AnimatePushObject(Vector2 dir)
	{
		Debug.Log("AnimatePushObject - dir: "+ dir);

		var startPos = (Vector2)this.transform.position;
		var goalPos = startPos + dir;

		body.velocity = Vector2.zero;
		body.isKinematic = true;

		float t = 0;
		while (t < 1){
			t = Mathf.Clamp01(t + Time.fixedDeltaTime * World.PushSpeed);
			this.transform.position = Vector2.Lerp(startPos, goalPos, t);
			yield return waitFixed;
		}

		body.isKinematic = false;
		routine = null;
	}
		
	private IEnumerator AnimateLiftObject()
	{
		Debug.Log("AnimateLiftObject");

		body.velocity = Vector2.zero;
		body.isKinematic = true;

		var tilePos = LevelHelper.RoundToTilePos(this.transform.position);
		yield return StartCoroutine(AnimateMoveToPos(tilePos));

		var startPos = liftedObject.transform.position;
		var goalPos = this.transform.position + Vector3.up;

		liftedObject.transform.SetParent(this.transform);
		liftedObject.enabled = false;

		float t = 0;
		while (t < 1){
			t = Mathf.Clamp01(t + Time.fixedDeltaTime * World.LiftSpeed);
			liftedObject.transform.position = Vector2.Lerp(startPos, goalPos, t);
			yield return waitFixed;
		}

		body.isKinematic = false;
		routine = null;
	}

	private IEnumerator AnimatePlaceObject(Vector2 goalPos)
	{
		body.velocity = Vector2.zero;
		body.isKinematic = true;

		var tilePos = LevelHelper.RoundToTilePos(this.transform.position);
		yield return StartCoroutine(AnimateMoveToPos(tilePos));

		liftedObject.transform.SetParent(null);
		var startPos = liftedObject.transform.position;
		Debug.Log("AnimatePlaceObject - startPos: "+ startPos.ToString("F2") + ", goalPos: " + goalPos.ToString("F2"));

		float t = 0;
		while (t < 1){
			t = Mathf.Clamp01(t + Time.fixedDeltaTime * World.LiftSpeed);
			liftedObject.transform.position = Vector2.Lerp(startPos, goalPos, t);
			yield return waitFixed;
		}
		liftedObject.enabled = true;
		liftedObject = null;

		body.isKinematic = false;
		routine = null;
	}
}
