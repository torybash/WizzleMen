using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizzPlayer : MonoBehaviour {

	private Rigidbody2D body;
	private float moveX;

	[SerializeField] private float speed;

	void Awake(){
		body = GetComponent<Rigidbody2D>();
	}

	void Update () {
		moveX = Input.GetAxisRaw("Horizontal");
	}

	void FixedUpdate(){
		body.velocity = new Vector2(moveX * speed, body.velocity.y);
	}
}
