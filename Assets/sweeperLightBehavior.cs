using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class sweeperLightBehavior : MonoBehaviour {

	public GameObject angle;
	private Vector3 q;
	private float rotateRangeDeg = 45f;

	// Use this for initialization
	void Start () {
		Assert.IsTrue (angle != null);
		q = angle.transform.eulerAngles; //get initial angle of camera
	}

	// Update is called once per frame
	void FixedUpdate () {
		angle.transform.eulerAngles = new Vector3 (q.x + 45f * Mathf.Sin (18 * Time.time * (2 * Mathf.PI) / 180), q.y, q.z);
	}
}
