using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(visibilityCheck))]
[RequireComponent(typeof(laserBehavior))]
public class cameraBehavior : MonoBehaviour {
	public GameObject angle;
	private Vector3 q;
	private float rotateRangeDeg = 35f;

	private visibilityCheck vC;
	private laserBehavior lb;
	private GvrAudioSource audioSrc;

	private bool playerSeen = false;
	private float deathDelayTimer = 0f;
	private float deathTriggerLimit = 5f;

	// Use this for initialization
	void Start () {
		Assert.IsTrue (angle != null);
		q = angle.transform.eulerAngles; //get initial angle of camera

		vC = GetComponent<visibilityCheck> ();
		lb = GetComponent<laserBehavior> ();

		audioSrc = GetComponent<GvrAudioSource> ();

		Assert.IsTrue (audioSrc.clip != null);
		audioSrc.playOnAwake = false;
		audioSrc.loop = true;
	}
	
	// Camera kills player (via laser) if detected, otherwise it scans the scene
	void FixedUpdate () {
		if (vC.getDetected () == true || playerSeen == true) {

			playerSeen = true;

			if (deathDelayTimer == 0f) {
				lb.LaserOn (sceneManager.instance.target);
				movementManager.instance.canMove = false;
			} 
			else if (deathDelayTimer > deathTriggerLimit) {
				gameManager.death ();
			}

			deathDelayTimer += Time.fixedDeltaTime;
		} 
		else {
			angle.transform.eulerAngles = new Vector3(q.x, q.y + rotateRangeDeg * Mathf.Sin (18*Time.time*(2*Mathf.PI)/180), q.z);
		}
	}
}
