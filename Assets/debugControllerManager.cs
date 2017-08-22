using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

//Script to help understand the player's detection scores from various enemies, as calculcated by visibilityCheck script
public class debugControllerManager : MonoBehaviour {

	//input
	public SteamVR_TrackedObject trackedObj;
	public SteamVR_Controller.Device device;

	public Text text;
	public GameObject monitor;
	public int visIndex = 0;
	private visibilityCheck vC;

	// Use this for initialization
	void Start () {
		trackedObj = GetComponent<SteamVR_TrackedObject>();
		Assert.IsTrue (text != null, "Debug text cannot null");
		Assert.IsTrue (monitor != null, "Debug monitor cannot null");

		text.enabled = true;
		monitor.SetActive(true);

		vC = sceneManager.instance.vCs [visIndex];
	}
	
	// Update is called once per frame
	void Update () {
		processDebugControls ();
	}

	void processDebugControls(){
		device = SteamVR_Controller.Input ((int)trackedObj.index);

		if (device.GetPressDown (SteamVR_Controller.ButtonMask.Trigger) && gameObject.name == "Controller (left)") {

			visIndex = (visIndex + 1) % sceneManager.instance.vCs.Length;
			vC = sceneManager.instance.vCs [visIndex];
		}

		monitor.GetComponent<Renderer> ().material.mainTexture = vC.rt;

		//Make sure updates are slow enough to be readable
		if (Time.frameCount % 90 == 0) {
			text.text = vC.gameObject.name +
			"\nVisScore: " + vC.visScore +
			"\nscreenSpace: " + vC.screenSpaceFactor + " " + vC.screenSpace +
			"\ncolorContrast: " + vC.colorContrastFactor + " " + vC.colorContrast +
			"\nposition: " + vC.posPenFactor + " " + vC.positionPen;
		}
	}
}
