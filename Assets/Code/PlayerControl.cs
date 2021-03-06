﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {
	public float speed = 500;
	public float jumpSpeed = 800;
	public float gravity = 50;
	public float terminalXVelocity = 2;
	public float maximumGroundAngle = 45;
	public float gorundBias = 0.1f;
	public PhysicsMaterial2D airMaterial;
	public PhysicsMaterial2D groundMaterial;
	public float fallDeathTime = 3;

	private float hori;
	private bool jump;
	private Transform ground;
	private Transform wall;
	private bool isGrounded;
	private bool isOnWall;
	private float maxGroundAngle;
	private Vector2 groundNormal;
	private Vector2 wallNormal;

	private CircleCollider2D ledgeRadius;
	private ContactFilter2D ledgeContact;
	private bool ledging;
	private bool armsOut;
	private bool climbing;
	private Transform sprite;
	[HideInInspector]
	public Vector2 ledge;

	private float fallDeathTimer;

	public static bool win;

	Collider2D playerCol;
	Rigidbody2D rb;
	Rigidbody2D[] crb;
	TargetJoint2D[] tjs;
	Transform[] arms;
	Animator animator;

	void Start() {
		playerCol = GetComponent<BoxCollider2D>();
		rb = GetComponent<Rigidbody2D>();
		maxGroundAngle = Mathf.Cos(maximumGroundAngle * Mathf.Deg2Rad);

		ledgeRadius = transform.Find("Ledge").GetComponent<CircleCollider2D>();
		ledgeContact = new ContactFilter2D();
		ledgeContact.SetLayerMask(1);
		sprite = transform.Find("Sprite");
		crb = sprite.GetComponentsInChildren<Rigidbody2D>();
		tjs = sprite.GetComponentsInChildren<TargetJoint2D>();
		arms = new Transform[] { transform.Find("Sprite/Player arm back"), transform.Find("Sprite/Player arm front") };

		animator = GetComponentInChildren<Animator>();
	}

	void Update() {
		if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Climb") || win) return;

		animator.SetBool("Grounded", isGrounded);

		hori = Input.GetAxis("Horizontal");

		//Only update jump to true
		if ((Input.GetButtonDown("Jump") && isGrounded) || (Input.GetButton("Jump") && isOnWall)) {
			jump = true;
			animator.SetTrigger("Jump");
		}

		//Ledging
		if (Input.GetButton("Ledge")) {
			List<Collider2D> cols = new List<Collider2D>();
			if (ledgeRadius.OverlapCollider(ledgeContact, cols) != 0) {
				foreach (Collider2D col in cols) {
					if (col is BoxCollider2D && col.transform.rotation.eulerAngles.sqrMagnitude == 0) {
						Vector2 point0 = col.bounds.max;
						Vector2 point1 = (Vector2)col.bounds.max - col.bounds.size.x * Vector2.right;

						if (ledgeRadius.OverlapPoint(point0)) {
							ledging = true;
							Ledge(point0);
						} else if (ledgeRadius.OverlapPoint(point1)) {
							ledging = true;
							Ledge(point1);
						} else {
							Vector2 closestPoint = (point0 - (Vector2)arms[0].position).sqrMagnitude > (point1 - (Vector2)arms[0].position).sqrMagnitude ? point1 : point0;
							//Vector2 lookAt = (Vector3)closestPoint - arms[0].position;
							ledge = closestPoint;
						}

						armsOut = true;
					}
				}
			} else if (armsOut) {
				ArmsDown();
			}
		} else if (armsOut) {
			ArmsDown();
		}

		//Ledge climbing
		if (ledging) {
			if (Input.GetAxis("Vertical") > 0) {
				climbing = true;
				rb.simulated = false;
				animator.SetTrigger("Climbing");
			}
		}

		//Orientation
		if (ledging) {
			transform.localScale = ((ledge.x - transform.position.x) > 0 ? Vector3.right : Vector3.left) + transform.localScale.y * Vector3.up + transform.localScale.z * Vector3.forward;
		} else if (isOnWall) {
			transform.localScale = (wallNormal.x > 0 ? Vector3.right : Vector3.left) + transform.localScale.y * Vector3.up + transform.localScale.z * Vector3.forward;
		} else if (isGrounded && Vector2.Dot(groundNormal, Vector2.up) < 0.5) {
			transform.localScale = (groundNormal.x > 0 ? Vector3.right : Vector3.left) + transform.localScale.y * Vector3.up + transform.localScale.z * Vector3.forward;
		} else if (hori != 0) {
			transform.localScale = (hori > 0 ? Vector3.right : Vector3.left) + transform.localScale.y * Vector3.up + transform.localScale.z * Vector3.forward;
		}

		//Falldamage
		if (isGrounded && Vector2.Dot(groundNormal, Vector2.up) > maxGroundAngle) {
			if (fallDeathTimer > fallDeathTime) {
				GameObject.FindGameObjectWithTag("GameLoop").GetComponent<GameLoop>().RestartLevel();
			}
		}

		if (rb.velocity.y < 0) {
			fallDeathTimer += Time.deltaTime;
			animator.SetBool("Fall", true);
		} else {
			fallDeathTimer = 0;
			animator.SetBool("Fall", false);
		}
	}

	void FixedUpdate() {
		if (ledging || animator.GetCurrentAnimatorStateInfo(0).IsTag("Climb") || win) return;

		//Stick to ground
		if (!isGrounded) {
			RaycastHit2D[] hit = new RaycastHit2D[1];
			if (playerCol.Cast(Vector2.down, hit, gorundBias) != 0) {
				transform.position += hit[0].distance * Vector3.down;
			}
		}

		//Jump
		if (jump) {
			Vector2 jumpNormal = isOnWall ? wallNormal : groundNormal;

			rb.velocity += (new Vector2(jumpNormal.x * 1.1f, jumpNormal.y)) * jumpSpeed * Time.fixedDeltaTime;

			playerCol.sharedMaterial = airMaterial;
			isGrounded = false;
			isOnWall = false;
			jump = false;
		}

		//Movement
		if (isGrounded && Vector2.Dot(groundNormal, Vector2.up) > maxGroundAngle) {
			rb.velocity = hori * speed * Time.fixedDeltaTime * Vector2.right + rb.velocity.y * Vector2.up;
			animator.SetBool("Walking", Mathf.Abs(hori) > 0.01);
		} else {
			animator.SetBool("Walking", false);
			/*if (rb.velocity.x > terminalXVelocity && hori > 0) hori = 0;
			if (rb.velocity.x < -terminalXVelocity && hori < 0) hori = 0;

			rb.velocity += hori * speed * Time.fixedDeltaTime * Vector2.right;*/
		}

		//Gravity
		rb.velocity += gravity * Time.fixedDeltaTime * Vector2.down;
	}

	void OnCollisionEnter2D(Collision2D collision) {
		//if (collision.transform == ground) return;

		Vector2 normal = Vector2.zero;
		foreach (ContactPoint2D cp in collision.contacts) {
			normal += cp.normal;
		}
		normal /= collision.contactCount;

		//Check contact normal is within maxGroundAngle
		float dot = Vector2.Dot(normal, Vector2.up);
		if (dot > 0.01) {
			ground = collision.transform;
			isGrounded = true;
			playerCol.sharedMaterial = groundMaterial;

			groundNormal = normal;
		} else if (dot > -0.01) {
			isOnWall = true;
			wall = collision.transform;

			wallNormal = (normal + Vector2.up*3)/2;
		}
	}

	void OnCollisionExit2D(Collision2D collision) {
		if (collision.transform == ground) {
			ground = null;
			isGrounded = false;
			playerCol.sharedMaterial = airMaterial;
		} else if (collision.transform == wall) {
			wall = null;
			isOnWall = false;
			playerCol.sharedMaterial = airMaterial;
		}
	}

	private void ArmsDown() {
		transform.Find("Sprite/Player body").rotation = Quaternion.Euler(Vector3.zero);

		foreach (Transform arm in arms) {
			arm.rotation = Quaternion.Euler(Vector2.down);
		}

		armsOut = false;
		ledging = false;

		StopLedge();
	}

	private void Ledge(Vector2 point) {
		rb.simulated = false;
		rb.velocity = Vector2.zero;

		transform.position -= ((transform.position + new Vector3(0.137f, 0.891f)) - (Vector3)point);

		animator.SetBool("Holding", true);

		ledge = point;

		foreach (Rigidbody2D rb in crb) {
			rb.simulated = true;
		}

		foreach (TargetJoint2D tj in tjs) {
			tj.target = point;
		}
	}

	private void StopLedge() {
		rb.simulated = true;

		animator.SetBool("Holding", false);

		ledge = Vector2.zero * float.NaN;

		foreach (Rigidbody2D rb in crb) {
			rb.simulated = false;
		}

		transform.Find("Sprite/Player body").localPosition = Vector2.zero;
		transform.Find("Sprite/Player arm front").localPosition = new Vector2(0, 0.29f);
		transform.Find("Sprite/Player arm back").localPosition = new Vector2(0, 0.29f);
	}

	public void EndClimbingAnimation() {
		ArmsDown();
		climbing = false;
		transform.position = sprite.position;
		sprite.localPosition = Vector3.zero;
	}

	public void CenterPlayerBody() {
		//Offset sprite, so body is 0
		transform.position += sprite.Find("Player body").localPosition;
		sprite.Find("Player arm back").localPosition -= sprite.Find("Player body").localPosition;
		sprite.Find("Player arm front").localPosition -= sprite.Find("Player body").localPosition;
		sprite.Find("Player body").localPosition = Vector3.zero;
		sprite.transform.localPosition = Vector3.zero;

	}
}
