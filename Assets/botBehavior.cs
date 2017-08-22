using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Assertions;

[RequireComponent(typeof(visibilityCheck))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(laserBehavior))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(GvrAudioSource))]
public class botBehavior : MonoBehaviour
{
	public GameObject[] points;
	private int destPoint = 0;
	private NavMeshAgent agent;
	private laserBehavior lb;
	private GameObject target;

	private Animator anim;	// a reference to the animator on the character
	private Rigidbody rb;
	private visibilityCheck vC;
	private Vector3 destV3;

	private float deathDelayTimer = 0f;
	private float deathTriggerLimit = 5f;
	private bool playerSeen = false;

	private dogBehavior closestDog;
	public AudioClip[] audioArr;
	private GvrAudioSource audioSrc;
	private bool waypointUpdated = false;

	void Start () {
		for (int i = 0; i < points.Length; i++) {
			Assert.IsTrue (points [i] != null);
		}

		anim = GetComponent<Animator>();	
		rb = gameObject.GetComponent<Rigidbody>();
		vC = gameObject.GetComponent<visibilityCheck> ();
		lb = gameObject.GetComponentInChildren<laserBehavior> ();
		agent = GetComponent<NavMeshAgent>();
		audioSrc = GetComponent<GvrAudioSource> ();
		target = sceneManager.instance.target;

		// Disabling auto-braking allows for continuous movement
		// between points (ie, the agent doesn't slow down as it
		// approaches a destination point).
		agent.autoBraking = true;
		agent.speed = 0.1f;

		GotoNextWayPoint();
	}

	private Vector3 getDest(){
		return new Vector3(points[destPoint].transform.position.x, 0.1f, points[destPoint].transform.position.z);
	}

	void GotoNextWayPoint() {
		if (points.Length == 0)
			return;

		destV3 = getDest();
		agent.destination = destV3;
		gameObject.transform.LookAt (destV3);
		Debug.Log (agent.gameObject.name + " is heading to " + points [destPoint].gameObject.name);

		destPoint = (destPoint + 1) % points.Length;

	}

	void FixedUpdate ()
	{
		if (vC.getDetected() == true || playerSeen == true){
			playerSeen = true;

			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			anim.SetFloat("Speed", 0f);
			transform.LookAt (target.transform.position);

			if (deathDelayTimer == 0f) {
				audioSrc.clip = audioArr [0]; //sound clip of laser

				agent.isStopped = true;
				//agent.enabled = false;

				lb.LaserOn (target);
				movementManager.instance.canMove = false;
			} 
			else if (deathDelayTimer > deathTriggerLimit) {
				gameManager.death ();
			}

			deathDelayTimer += Time.fixedDeltaTime;
		}
		else { 
			closestDog = sceneManager.getClosestActiveDog (gameObject.transform.position);

			//If there is a dog nearby the Android follows the dog, otherwise it navigates along its set waypoints
			if (closestDog != null) {
				agent.destination = closestDog.transform.position;
				rb.velocity = rb.velocity.normalized / 1.5f;
			} 
			else {
				//Android slows down and then stops completely as it approaches its waypoint
				if (agent.remainingDistance > 2f) {
					rb.velocity = rb.velocity.normalized / 2f;
					waypointUpdated = false;
				} 
				else if (agent.remainingDistance <= 2f && agent.remainingDistance > 0.2f) {
					rb.velocity = rb.velocity.normalized * Mathf.Min (agent.remainingDistance / 4 + 0.1f, 1);
				}
				else if (agent.remainingDistance <= 0.2f && waypointUpdated == false) {
					Debug.Log (agent.name + " is too close to destination!");
					rb.velocity = Vector3.zero;
					rb.angularVelocity = Vector3.zero;

					GotoNextWayPoint ();
					waypointUpdated = true;
				} 
			}

			anim.SetFloat("Speed", rb.velocity.magnitude);		// set our animator's float parameter 'Speed' equal to the vertical input axis		
		}
	}
}

