using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class GameLoop : MonoBehaviour {
	public float[] levelTimes;
	public Transform timerFillTransform;

	private float currentLevelTime;
	private RawImage timerFill;

	private byte currentLevel = 0;

	void Start() {
		currentLevel = (byte) SceneManager.GetActiveScene().buildIndex;

		timerFill = timerFillTransform.GetComponent<RawImage>();
	}

	void Update() {
		currentLevelTime += Time.deltaTime;

		timerFillTransform.localScale = (1f - currentLevelTime / levelTimes[currentLevel]) * Vector3.right + timerFillTransform.localScale.y * Vector3.up + timerFillTransform.localScale.z * Vector3.forward;
		timerFill.uvRect = new Rect(timerFill.uvRect.min, (1f - currentLevelTime / levelTimes[currentLevel]) * Vector2.right + Vector2.up);

		if (currentLevelTime > levelTimes[currentLevel]) {
			RestartLevel();
		}
	}

	public bool NextLevel() {
		if (SceneManager.sceneCountInBuildSettings <= ++currentLevel) return false;

		SceneManager.LoadScene(currentLevel);
		currentLevelTime = 0;
		return true;
	}

	public void RestartLevel() {
		SceneManager.LoadScene(currentLevel);
		currentLevelTime = 0;
	}
}
