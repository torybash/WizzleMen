using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class WizzPlayer : MonoBehaviour {



	[SerializeField] private float runSpeed;
	[SerializeField] private float jumpForce = 50f;
	[SerializeField, Range(0f,1f)] private float airControl = 0.5f;

	[SerializeField] private Collider2D groundColl;
	[SerializeField] private ContactFilter2D groundFilter;

	[SerializeField] private Collider2D frontColl;
	[SerializeField] private ContactFilter2D frontFilter;

	[SerializeField] private int playerNum;

	private Rigidbody2D body;

	private float moveX;
	private bool jumpPressed;
	private bool pushPressed;
	private bool lastLiftTapped;
	private bool liftTapped;

	private bool isGrounded;


	private Collider2D[] colliders = new Collider2D[1];

	private Coroutine routine;

	private LevelObject liftedObject;


	void Awake(){
		body = GetComponent<Rigidbody2D>();
	}

	void Update () {
		var inputDevice = (InputManager.Devices.Count > playerNum) ? InputManager.Devices[playerNum] : null;

		if (inputDevice == null){	
			moveX = Input.GetAxisRaw("Horizontal");
			jumpPressed = Input.GetKey(KeyCode.Space);
			pushPressed = Input.GetKey(KeyCode.Z);
			liftTapped = Input.GetKey(KeyCode.X);
		}else{
			moveX = inputDevice.Direction.X;
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

		body.velocity = new Vector2(moveX * runSpeed * (isGrounded ? 1f : airControl), body.velocity.y);

		if (Mathf.Abs(body.velocity.x) > 0)
			transform.localScale = new Vector3(Mathf.Sign(body.velocity.x), transform.localScale.y, transform.localScale.z);

		if (isGrounded && jumpPressed && body.velocity.y == 0){
			body.AddForce(Vector2.up * jumpForce);
			jumpPressed = false;
		}

		bool hasFrontCollision = frontColl.OverlapCollider(frontFilter, colliders) != 0;


		float xDir = Mathf.Sign(body.velocity.x);

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
				if (obj != null && obj.Stats.isPushable && obj.CanBeLifted()){
					liftedObject = obj;
					routine = StartCoroutine(AnimateLiftObject());
				}
			}else{
				var goalPos = this.transform.position + Vector3.right * xDir;
				if (liftedObject.CanBePlaced(goalPos)){
					routine = StartCoroutine(AnimatePlaceObject(goalPos));
				}
			}
		}

		liftTapped = false;

		isGrounded = groundColl.OverlapCollider(groundFilter, colliders) != 0;
	}


	private IEnumerator AnimatePushObject(Vector3 dir)
	{
		Debug.Log("AnimatePushObject - dir: "+ dir);

		body.velocity = Vector2.zero;

		var startPos = this.transform.position;
		var goalPos = this.transform.position + dir;
		var waitFixed = new WaitForFixedUpdate();
		float t = 0;
		while (t < 1){
			t = Mathf.Clamp01(t + Time.fixedDeltaTime * World.PushSpeed);
			this.transform.position = Vector2.Lerp(startPos, goalPos, t);
			yield return waitFixed;
		}
		routine = null;
	}
		
	private IEnumerator AnimateLiftObject()
	{
		Debug.Log("AnimateLiftObject");

		liftedObject.enabled = false;
		body.isKinematic = true;

		liftedObject.transform.SetParent(this.transform);
		var startPos = liftedObject.transform.position;
		var goalPos = this.transform.position + Vector3.up;
		var waitFixed = new WaitForFixedUpdate();
		float t = 0;
		while (t < 1){
			t = Mathf.Clamp01(t + Time.fixedDeltaTime * World.LiftSpeed);
			liftedObject.transform.position = Vector2.Lerp(startPos, goalPos, t);
			yield return waitFixed;
		}
		body.isKinematic = false;
		routine = null;
	}

	private IEnumerator AnimatePlaceObject(Vector3 goalPos)
	{

		body.isKinematic = true;
		liftedObject.transform.SetParent(null);
		var startPos = liftedObject.transform.position;
		Debug.Log("AnimatePlaceObject - startPos: "+ startPos.ToString("F2") + ", goalPos: " + goalPos.ToString("F2"));

		var waitFixed = new WaitForFixedUpdate();
		float t = 0;
		while (t < 1){
			t = Mathf.Clamp01(t + Time.fixedDeltaTime * World.LiftSpeed);
			liftedObject.transform.position = Vector2.Lerp(startPos, goalPos, t);
			yield return waitFixed;
		}
		liftedObject.enabled = true;
		liftedObject = null;
		routine = null;
		body.isKinematic = false;
	}
}
