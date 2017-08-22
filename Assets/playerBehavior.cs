using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerBehavior : MonoBehaviour {

	void OnTriggerEnter(Collider coli){
		if (coli.tag == "wall") {
			gameManager.antiCheat ();
		}
			
		else if (coli.tag == "Checkpoint") {
			gameManager.setCheckPoint (System.Convert.ToInt32(coli.gameObject.name));
		}

		else if (coli.tag == "Finish") {
			gameManager.beatLevel ();
			Debug.Log ("Trying to finish level");
		}
	}
}
