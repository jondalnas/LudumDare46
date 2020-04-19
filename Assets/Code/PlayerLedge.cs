using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLedge : MonoBehaviour {
	public PlayerControl pc;

	Animator anim;
	Transform[] arms;

	void Start() {
		anim = GetComponent<Animator>();
		arms = new Transform[] { transform.Find("Player arm back"), transform.Find("Player arm front") };
	}

	private void OnAnimatorMove() {
		if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Climb")) return;

		if (!float.IsNaN(pc.ledge.x)) {

			Debug.Log("Hello");
			Vector2 lookAt = (Vector3)pc.ledge - arms[0].position;
			foreach (Transform arm in arms) {
				arm.up = -lookAt;
			}
		}
	}
}
