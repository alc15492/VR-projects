using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class helpers : MonoBehaviour {
	private bool debugMode = true;
	private static string[] excludedParentNames = { "Guards", "SecurityCameras" };

	public static GameObject getTopLevelParent(GameObject go){
		while (go.transform.parent != null && System.Array.IndexOf(excludedParentNames, go.transform.parent.name) == -1) {
			go = go.transform.parent.gameObject;
		}

		return go;
	}
}