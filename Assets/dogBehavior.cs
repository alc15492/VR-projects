using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

[RequireComponent(typeof(NavMeshAgent))]
public class dogBehavior : MonoBehaviour {

	//add randomness
	public GameObject target;
	public float randFactor = 0.7f;
	public float updateDelay = 15f;
	private float activationRadius = 40f;
	public GameObject debugMarker;

	private NavMeshAgent agent;
	private float dist;
	private Vector2 randV2;
	private Vector3 randOffset;

	private bool validDest;
	private int findDestAttempts;
	private int attemptsCap = 10;
	private bool dogActive = false;

	RaycastHit hit;
	NavMeshHit hit2;

	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent> ();
		Assert.IsTrue (agent != null, "navMeshAgent not found");

		target = sceneManager.instance.target;
	}

	void Update(){
		if ((target.transform.position - agent.transform.position).magnitude < activationRadius && dogActive == false) {
			dogActive = true;
			StartCoroutine (updateDestination ());
		}
	}

	//Set the dog to converge to the player's destination, with randomness proportional based on distance
	void setDestination(){
		validDest = false;
		findDestAttempts = 0;

		while (validDest == false && findDestAttempts < attemptsCap) {
			findDestAttempts++;

			randV2 = Random.insideUnitCircle;
			randOffset = (target.transform.position - agent.transform.position).magnitude * randFactor * (randV2.x * Vector3.right + randV2.y * Vector3.forward);

			validDest = agent.SetDestination (target.transform.position + randOffset) && (NavMesh.SamplePosition (target.transform.position + randOffset, out hit2, 5f, NavMesh.AllAreas));
			Debug.Log (agent.SetDestination (target.transform.position + randOffset) + " // " + NavMesh.SamplePosition (target.transform.position + randOffset, out hit2, 5f, NavMesh.AllAreas));
			Debug.Log (target.transform.position);

			//Debug.Log ("randOffset is " + randOffset + " // randV2 is " + randV2 + " // dist is " + (target.transform.position - agent.transform.position).magnitude);
		}

		if (findDestAttempts == attemptsCap) {
			Debug.Log ("Error! Could not find a valid destination");
		} 
		else {
			Debug.Log ("debug marker is active!");
			if (debugMarker != null) {
				debugMarker.SetActive (true);
				debugMarker.transform.position = target.transform.position + randOffset;
			}
		}
	}

	// Update is called once per frame
	IEnumerator updateDestination () {
		do {
			setDestination ();
			yield return new WaitForSecondsRealtime (updateDelay);
		} while(1 == 1);
	}

	public bool DogActive {get{ return dogActive; }}
}
