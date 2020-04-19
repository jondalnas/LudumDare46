using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour {
	public PlayerControl pc;

	public void EndClimbingAnimation() {
		pc.EndClimbingAnimation();
	}

	public void CenterPlayerBody() {
		pc.CenterPlayerBody();
	}
}
