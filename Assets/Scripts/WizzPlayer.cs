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
	private bool isGrounded;


	private Collider2D[] colliders = new Collider2D[1];

	private Coroutine routine;


	void Awake(){
		body = GetComponent<Rigidbody2D>();
	}

	void Update () {
		var inputDevice = (InputManager.Devices.Count > playerNum) ? InputManager.Devices[playerNum] : null;

		if (inputDevice == null){	
			moveX = Input.GetAxisRaw("Horizontal");
			jumpPressed = Input.GetKey(KeyCode.Space);
		}else{
			moveX = inputDevice.Direction.X;
			jumpPressed = inputDevice.Action1;
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


		if (isGrounded && Mathf.Abs(body.velocity.x) > 0 && frontColl.OverlapCollider(frontFilter, colliders) != 0){
			float xDir = Mathf.Sign(body.velocity.x);
			Debug.Log("Collider: "+ colliders[0] + ", xDir: "+ xDir);
			if (colliders[0].GetComponent<LevelObject>() != null){
				var obj = colliders[0].GetComponent<LevelObject>();
				if (obj.Stats.isPushable && obj.CanBePushed(xDir)){
					obj.StartPush(xDir);
					routine = StartCoroutine(AnimatePushObject(xDir * Vector3.right));
				}
			}
		}

		isGrounded = groundColl.OverlapCollider(groundFilter, colliders) != 0;
	}


	private IEnumerator AnimatePushObject(Vector3 dir){
		body.velocity = Vector2.zero;

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
