using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using UnityEditor;
using UnityEngine.EventSystems;

public class WizzPlayer : MonoBehaviour, ILevelThing
{

	public enum State{
		NORMAL,
		CLIMBING_LADDER,
		DEAD
	}


	[SerializeField] private float runSpeed;
	[SerializeField] private float climbSpeed = 2f;
	[SerializeField] private float jumpForce = 50f;
	[SerializeField, Range(0f,1f)] private float airControl = 0.5f;

	[SerializeField] private ContactFilter2D ladderFilter;

	[SerializeField] private Collider2D groundColl;
	[SerializeField] private ContactFilter2D groundFilter;

	[SerializeField] private Collider2D frontColl;
	[SerializeField] private ContactFilter2D frontFilter;

	[SerializeField] private int playerNum;
	
	private Collider2D _selfColl;

	private Rigidbody2D _body;

	private Vector2 _move;
	private bool _jumpPressed;
	private bool _pushPressed;
	private bool _lastLiftTapped;
	private bool _liftTapped;

	private bool _isGrounded;

	private State _state;

	public Vector3 OriginalPosition
	{
		get; set;
	}

	private Collider2D[] _colliders = new Collider2D[4];

	[SerializeField] private string currentRoutineName;
	private Coroutine _routine;
	private Coroutine routine{
		get{ return _routine;}
		set{ _routine = value; currentRoutineName = value == null ? "NONE" : value.ToString();} 
	}

	private LevelObject _liftedObj;

	private WaitForFixedUpdate _waitFixed = new WaitForFixedUpdate();

	private Animator _anim;


	void Awake(){
		_selfColl = GetComponent<Collider2D>();
		_body = GetComponent<Rigidbody2D>();
		_anim = GetComponent<Animator>();
	}

	void Update () {
		var inputDevice = (InputManager.Devices.Count > playerNum) ? InputManager.Devices[playerNum] : null;

		if (inputDevice == null){	
			_move.x = Input.GetAxisRaw("Horizontal");
			_move.y = Input.GetAxisRaw("Vertical");
			_jumpPressed = Input.GetKey(KeyCode.Space);
			_pushPressed = Input.GetKey(KeyCode.Z);
			_liftTapped = Input.GetKey(KeyCode.X);
		}else{
			_move = inputDevice.Direction;
			_jumpPressed = inputDevice.Action1;
			_pushPressed = inputDevice.Action2;
			if (!_lastLiftTapped && inputDevice.Action3) 
				_liftTapped = true;
			_lastLiftTapped = inputDevice.Action3;
		}
	}

	void FixedUpdate()
	{
		if (routine != null) return;



		switch (_state) {
		case State.NORMAL:
			_body.gravityScale = 3f;
			NormalMovement();
			break;
		case State.CLIMBING_LADDER:
			_body.gravityScale = 0f;
			LadderMovement();
			break;
		case State.DEAD:

			break;
		}
			
		_liftTapped = false;
	}


	public void Reset()
	{
		_state = State.NORMAL;
		_anim.SetTrigger("Default");
	}

	private void NormalMovement()
	{
		bool isNearLadder = _selfColl.OverlapCollider(ladderFilter, _colliders) > 0;
		if (_liftedObj == null && _isGrounded && Mathf.Abs(_move.y) > 0 && isNearLadder){
			var ladderTile = LevelHelper.ClosestCollider(this.transform.position, _colliders).GetComponent<LevelTile>();
			routine = StartCoroutine(AnimateClimbLadder(ladderTile));
			return;
		}

		_body.velocity = new Vector2(_move.x * runSpeed * (_isGrounded ? 1f : airControl), _body.velocity.y);

		if (Mathf.Abs(_body.velocity.x) > 0){
			transform.localScale = new Vector3(Mathf.Sign(_body.velocity.x), transform.localScale.y, transform.localScale.z);
		}

		if (_isGrounded && _jumpPressed && _body.velocity.y == 0){
			_body.AddForce(Vector2.up * jumpForce);
			_jumpPressed = false;
		}

		bool hasFrontCollision = frontColl.OverlapCollider(frontFilter, _colliders) > 0;

		float xDir = Mathf.Sign(transform.localScale.x);

		if (_pushPressed){
			var obj = _colliders[0].GetComponent<LevelObject>();
			if (_isGrounded && hasFrontCollision && Mathf.Abs(_body.velocity.x) > 0 && obj != null){
				//					Debug.Log("Collider: "+ colliders[0] + ", xDir: "+ xDir);
				if (obj.Stats.isPushable && obj.CanBePushed(xDir)){
					obj.StartPush(xDir);
					routine = StartCoroutine(AnimatePushObject(xDir * Vector3.right));
				}
			}

		}else if (_liftTapped){
			if (_liftedObj == null){
				var obj = _colliders[0].GetComponent<LevelObject>();
				if (_isGrounded && obj != null && obj.Stats.isPushable && obj.CanBeLifted()){
					routine = StartCoroutine(AnimateLiftObject(xDir, obj));
				}
			}else{
				var goalPos = LevelHelper.RoundToTilePos((Vector2)this.transform.position + Vector2.right * xDir);
				if (_isGrounded && _liftedObj.CanBePlaced(goalPos)){
					routine = StartCoroutine(AnimatePlaceObject(xDir, goalPos));
				}
			}
		}

		_isGrounded = groundColl.OverlapCollider(groundFilter, _colliders) > 0;

		_anim.SetBool("IsGrounded", _isGrounded);
		_anim.SetFloat("MoveSpeed", _move.x);
	}

	private void LadderMovement()
	{
		bool isNearLadder = _selfColl.OverlapCollider(ladderFilter, _colliders) > 0;

		if (isNearLadder || _move.y < 0)
			_body.velocity = new Vector2(0, _move.y * climbSpeed);
		else 
			_body.velocity = new Vector2(0, 0);

		if (Mathf.Abs(_move.x) > 0){
			_state = State.NORMAL;
		}
	}

	public void ObjectSquish()
	{
		Debug.Log("ObjectSquish");
		_anim.SetTrigger("Squish");
	}


	private IEnumerator AnimateMoveToPos(Vector3 goalPos)
	{		
		Debug.Log("AnimateMoveToPos - goalPos: " + goalPos);

		float t = 0;
		while (this.transform.position != goalPos){
			this.transform.position = Vector2.MoveTowards(this.transform.position, goalPos, Time.fixedDeltaTime * runSpeed);
			yield return _waitFixed;
		}
	}

	private IEnumerator AnimatePushObject(Vector2 dir)
	{
		Debug.Log("AnimatePushObject - dir: "+ dir);

		var startPos = (Vector2)this.transform.position;
		var goalPos = startPos + dir;

		_body.velocity = Vector2.zero;
		_body.isKinematic = true;

		_anim.SetTrigger("Pushing");

		float t = 0;
		while (t < 1){
			t = Mathf.Clamp01(t + Time.fixedDeltaTime * World.PushSpeed);
			this.transform.position = Vector2.Lerp(startPos, goalPos, t);
			yield return _waitFixed;
		}

		_body.isKinematic = false;
		routine = null;

		_anim.SetTrigger("Default");
	}
		
	private IEnumerator AnimateLiftObject(float xDir, LevelObject obj)
	{
		Debug.Log("AnimateLiftObject - obj: " + obj);

		_liftedObj = obj;

		_body.velocity = Vector2.zero;
		_body.isKinematic = true;

		var liftTilePos = LevelHelper.RoundToTilePos(this.transform.position);
		Debug.Log("this.transform.position: "+ this.transform.position.ToString("F2") + ", liftTilePos: "+ liftTilePos.ToString("F2"));
		yield return StartCoroutine(AnimateMoveToPos(liftTilePos));

		var objStartPos = _liftedObj.transform.position;
		var objGoalPos = this.transform.position + Vector3.up * 0.9f;

		_liftedObj.transform.SetParent(this.transform);
		_liftedObj.enabled = false;

		float t = 0;
		while (t < 1){
			t = Mathf.Clamp01(t + Time.fixedDeltaTime * World.LiftSpeed);
			_liftedObj.transform.position = Vector2.Lerp(objStartPos, objGoalPos, t);
			yield return _waitFixed;
		}

		_body.isKinematic = false;
		routine = null;
	}

	private IEnumerator AnimatePlaceObject(float xDir, Vector2 goalPos)
	{
		_body.velocity = Vector2.zero;
		_body.isKinematic = true;

		var tilePos = LevelHelper.RoundToTilePos(this.transform.position);
		yield return StartCoroutine(AnimateMoveToPos(tilePos));

		_liftedObj.transform.SetParent(null);
		var startPos = _liftedObj.transform.position;
		Debug.Log("AnimatePlaceObject - startPos: "+ startPos.ToString("F2") + ", goalPos: " + goalPos.ToString("F2"));

		float t = 0;
		while (t < 1){
			t = Mathf.Clamp01(t + Time.fixedDeltaTime * World.LiftSpeed);
			_liftedObj.transform.position = Vector2.Lerp(startPos, goalPos, t);
			yield return _waitFixed;
		}
		_liftedObj.enabled = true;
		_liftedObj = null;

		_body.isKinematic = false;
		routine = null;

		_anim.SetTrigger("Default");
	}

	private IEnumerator AnimateClimbLadder(LevelTile ladderTile)
	{
		_body.velocity = Vector2.zero;
		_body.isKinematic = true;

		var tilePos = LevelHelper.RoundToTilePos(ladderTile.transform.position);
		yield return StartCoroutine(AnimateMoveToPos(tilePos));

		_state = State.CLIMBING_LADDER;
		_body.isKinematic = false;
		routine = null;
	}






}
