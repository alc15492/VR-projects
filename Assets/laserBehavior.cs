using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

// Require the following components when using this script
[RequireComponent(typeof(GvrAudioSource))]
public class laserBehavior : MonoBehaviour
{
	//Laser script takes in a target

	public LineRenderer laserPrefab; 	// public variable for Laser prefab 

	private botBehavior botBehavior;	// control script
	public GameObject[] sourceArr;
	private LineRenderer[] laserArr;
	private GvrAudioSource audioSrc;
	private bool shot = false;
	private Vector3 bodyOffset; //body offset from HMD model

	void Start()
	{		
		Assert.IsTrue(sourceArr.Length > 0);
		Assert.IsTrue (laserPrefab != null);

		botBehavior = transform.GetComponent<botBehavior>(); 
		audioSrc = GetComponent<GvrAudioSource> ();

		laserArr = new LineRenderer[sourceArr.Length];
		bodyOffset = new Vector3 (0f, -0.3f, 0f);
	}


	public void LaserOn (GameObject go)
	{
		if (shot == false) {
			shot = true;

			audioSrc.volume = 1.0f;
			audioSrc.Play ();

			for (int i = 0; i < sourceArr.Length; i++) {
				laserArr [i] = Instantiate (laserPrefab) as LineRenderer;
				laserArr[i].SetPosition(0, sourceArr[i].transform.position);
				laserArr[i].SetPosition(1, go.transform.position + bodyOffset);
			}
		}
	}

	public void LaserOff()
	{
		if (shot == true) {
			shot = false;
			audioSrc.Stop ();

			for (int i = 0; i < sourceArr.Length; i++) {
				Destroy (laserArr[i]);
			}
		}
	}
}
