using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class towerBehavior : MonoBehaviour {
	public GameObject lightObj;
	private Vector3 q;

	// Use this for initialization
	void Start () {
		lightObj = gameObject.transform.Find ("Spotlight").gameObject;
		Assert.IsTrue (lightObj != null, "spotlight not found");

		q = lightObj.transform.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
		lightObj.transform.eulerAngles = new Vector3(q.x, q.y + ((Time.time * 360f / 10f) % 360f), q.z);
	}
}
