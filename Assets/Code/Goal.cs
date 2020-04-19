using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {
	[HideInInspector]	
	public GameLoop gl;

	public Transform winText;

	void Start() {
		gl = GameObject.FindGameObjectWithTag("GameLoop").GetComponent<GameLoop>();
	}

	void OnTriggerEnter2D(Collider2D collision) {
		if (!collision.CompareTag("Player")) return;

		if (!gl.NextLevel()) {
			winText.gameObject.SetActive(true);
			Time.timeScale = 0;
		}
	}
}
