using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	Camera cam;
	Transform player;

	void Start() {
		cam = Camera.main;
		player = GameObject.FindGameObjectWithTag("Player").transform.Find("Sprite/Player body");
	}

	void Update() {
		//Center camera
		cam.transform.position = player.position + (Vector3.back * 10);
	}
}
