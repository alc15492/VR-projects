using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class movementManager : MonoBehaviour {
	public static movementManager instance;

	public float teleEnergy = 25f;
	public bool canMove = true;

	private static float teleEnergyIncrease = 3.5f;
	public static float teleEnergyCap = 25f;
	public static float yNudgeAmount = 0.1f; //specific to teleportAimerObject
	public static float laserDist = 10f;

	void Awake(){
		if (instance == null) {
			instance = this;
		} 
		else if (instance != this) {
			Destroy (gameObject);
		}
	}

	void Update(){
		teleEnergy = Mathf.Min(teleEnergy + teleEnergyIncrease * Time.deltaTime, teleEnergyCap);
	}

	public static void moveTo(Vector3 loc){
		instance.teleEnergy -= teleEnergyCostFunc ((loc - sceneManager.instance.rig.transform.position).magnitude);
		sceneManager.instance.rig.transform.position = new Vector3(loc.x, loc.y + yNudgeAmount, loc.z);
	}

	public static bool validLocation(Vector3 start, Vector3 dir, out RaycastHit hit, out NavMeshHit hit2){
		hit2 = new NavMeshHit();

		//Initial raycast only hits the layers "ground and blocked"
		if (Physics.Raycast (start, dir, out hit, laserDist, (1 << LayerMask.NameToLayer ("ground")) + (1 << LayerMask.NameToLayer ("blocked")))) {
			//Raycast hit locations that are 1) on the "ground layer", 2) in a navigable area not too close to walls, and 3) leave the user with enough energy are valid
			if (hit.transform.gameObject.layer == LayerMask.NameToLayer ("ground")
				&& (NavMesh.SamplePosition (hit.point, out hit2, .2f, NavMesh.AllAreas))
				&& (teleEnergyCostFunc ((hit.point - sceneManager.instance.rig.transform.position).magnitude)) < instance.teleEnergy) {
				return true;
			} else {
				return false;
			}
		}
		return false;
	}

	//Cost function penalizes farther teleports disproportionately 
	public static float teleEnergyCostFunc(float dist){
		return dist + Mathf.Pow (Mathf.Max (dist - 3f, 0), 1.6f);
	}
}
