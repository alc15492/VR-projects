using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class controllerInputManager : MonoBehaviour {

	//input
	public SteamVR_TrackedObject trackedObj;
	public SteamVR_Controller.Device device;

	//teleporter
	public LineRenderer laserPrefab;
	private LineRenderer laser;
	public GameObject player;
	public LayerMask laserMask;

	private Vector3 teleportLocation;
	private bool teleportLocationSet = false;

	GameObject attachedObject;
	Vector3 tempVector;

	//score updating
	public GameObject visCheckHolder;
	public GameObject hmdModel;

	public Text text;

	void Start () {
		Assert.IsTrue (laserPrefab != null, "Laser prefab cannot be null");

		trackedObj = GetComponent<SteamVR_TrackedObject>();
		laser = Instantiate (laserPrefab) as LineRenderer;

		text.enabled = false;
	}

	void Update () {
		if (movementManager.instance.canMove == true) {
			processTeleporting ();
		}
		processVoiceToggle ();
	}

	void processTeleporting(){
		device = SteamVR_Controller.Input((int)trackedObj.index);
		RaycastHit hit;
		NavMeshHit hit2;

		text.enabled = true;
		text.text = (movementManager.instance.teleEnergy/movementManager.teleEnergyCap).ToString ("#%");

		if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad) && gameObject.name == "Controller (right)")
		{
			if(movementManager.validLocation(transform.position, transform.forward, out hit, out hit2) == true){
					laser.enabled = true;
					teleportLocation = hit.point;
					teleportLocationSet = true;

					laser.SetPosition (0, gameObject.transform.position);
					laser.SetPosition (1, teleportLocation);
			} 
			else {
					laser.enabled = false;
					teleportLocationSet = false;
			}

		}

		if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad) && gameObject.name == "Controller (right)" && teleportLocationSet == true)
		{
			laser.enabled = false;
			movementManager.moveTo (teleportLocation);
		}
	}

	void processVoiceToggle(){
		if (device.GetPressDown (SteamVR_Controller.ButtonMask.Trigger) && gameObject.name == "Controller (right)") {
			gameManager.instance.toggleVoiceEnabled ();
		}
	}

	void OnTriggerEnter(Collider coli){
		if (coli.tag == "wall") {
			gameManager.antiCheat ();
		}
	}
}
