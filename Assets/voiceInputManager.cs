using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySpeechToText.Services;
using UnityEngine.Assertions;
using UnityEngine.AI;

[RequireComponent(typeof(GoogleStreamingSpeechToTextService))]
public class voiceInputManager : MonoBehaviour {

	public GoogleStreamingSpeechToTextService m_SpeechToTextService;
	float delayTimer = 0f;
	float delayLimit = 1.5f;

	public LineRenderer laserPrefab;
	private LineRenderer laser;
	private bool laserEnabled = false;

	private Vector3 laserOriginOffset = 0.2f * Vector3.down;
	public GameObject target; //target is the player's HMD model

	private Vector3 teleportLocation;
	private bool teleportLocationSet = false;
	RaycastHit hit;
	NavMeshHit hit2;

	// Use this for initialization
	void Start () {
		m_SpeechToTextService = GetComponent<GoogleStreamingSpeechToTextService>();
		Assert.IsTrue (m_SpeechToTextService != null, "Can't find Google streaming service component");

		m_SpeechToTextService.RegisterOnError(OnError);
		m_SpeechToTextService.RegisterOnTextResult(OnTextResult);
		m_SpeechToTextService.RegisterOnRecordingTimeout(OnRecordingTimeout);

		Assert.IsTrue (laserPrefab != null, "Laser prefab is null");
		laser = Instantiate (laserPrefab) as LineRenderer;

		target = sceneManager.instance.target;
	}

	void Update(){
		if (laserEnabled == true) {
			
			if (movementManager.validLocation (target.transform.position, target.transform.forward, out hit, out hit2) == true) {
				laser.enabled = true;
				teleportLocation = hit.point;
				teleportLocationSet = true;

				laser.SetPosition (0, (target.transform.position + laserOriginOffset) + 0.8f * (teleportLocation - (target.transform.position + laserOriginOffset)));
				laser.SetPosition (1, teleportLocation);
			} else {
				laser.enabled = false;
				teleportLocationSet = false;
			}
		}

		//Need to add a short delay after timeout before restarting, to prevent IO errors
		if (m_SpeechToTextService.IsRecording == false) {
			if (delayTimer < delayLimit) {
				delayTimer += Time.deltaTime;
			} else {
				delayTimer = 0f;
				Debug.Log (m_SpeechToTextService.StartRecording ());
				Debug.Log ("Started recording!");
			}
		}
	}

	void OnError(string text)
	{
		Debug.LogError(text);
	}

	void OnTextResult(SpeechToTextResult result)
	{
		if (result.IsFinal)
		{

			Debug.Log("Final result:");
			for (int i = 0; i < result.TextAlternatives.Length; ++i)
			{
				StartCoroutine(sceneManager.instance.displayTextOnController ("Voice input detected: " + result.TextAlternatives [i].Text));
				Debug.Log("Alternative " + i + ": " + result.TextAlternatives[i].Text);
				if (result.TextAlternatives [i].Text == "restart") {
					gameManager.restartLevel ();
				}
				else if (result.TextAlternatives [i].Text == "laser pointer" || result.TextAlternatives [i].Text == "laser") {
					laserEnabled = !laserEnabled;
				}
				else if (result.TextAlternatives [i].Text == "move forward" || result.TextAlternatives [i].Text == "forward" || result.TextAlternatives [i].Text == "move") {
					processLookMovement ();
				}
			}
		}
		else
		{
			Debug.Log("Interim result:");
		}
	}

	void processLookMovement(){
		Debug.Log ("move attempted");

		if (teleportLocationSet == true) {
			Debug.Log ("move attempted 2");
			movementManager.moveTo (teleportLocation);
			laser.enabled = false;
		}
	}

	void OnRecordingTimeout()
	{
		Debug.Log("Timeout");
	}
}
