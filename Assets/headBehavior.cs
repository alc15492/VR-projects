using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class headBehavior : MonoBehaviour {
	//Looking through walls is considered cheating
	void OnTriggerEnter(Collider coli){
		if (coli.tag == "wall") {
			gameManager.antiCheat ();
		}
	}
}
