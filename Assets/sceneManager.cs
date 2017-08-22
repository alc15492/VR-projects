using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(voiceInputManager))]
[RequireComponent(typeof(movementManager))]
public class sceneManager : MonoBehaviour {

	public static sceneManager instance;
	private gameManager gM;

	public GameObject[] checkPointArr;
	public GameObject dogArrContainer;
	public static float dogDistanceCutoff = 25f;

	private dogBehavior[] dogArr;

	public Text timerObj;
	private bool timerActive = false;
	private float timerCurrent = 180f; //this should be 180f for non-debugging purposes

	public visibilityCheck[] vCs;

	//NEW VARIABLES
	public GameObject target;
	public GameObject cIMHolder;
	public controllerInputManager cIM;
	public GameObject rig;
	public GameObject controllerText;

	//turnCamerasOnOff
	private Camera c;

	public bool levelBeaten = false;

	void Awake(){
		if (instance == null) {
			instance = this;
		} 
		else if (instance != this) {
			Destroy (gameObject);
		}
	}

	void Start(){
		gameManager.instance.initialize();
		initialize ();
	}

	void Update(){
		if(timerActive == true)
			updateTimer ();
		turnCamerasOnOff ();
	}

	public void setupVisibility(){
		vCs = UnityEngine.Object.FindObjectsOfType<visibilityCheck>() ;
		for (int i = 0; i < vCs.Length; i++) {
			vCs [i].rt = new RenderTexture (128, 128, 24, RenderTextureFormat.Default);
			vCs [i]._camera.targetTexture = vCs [i].rt;
			vCs[i]._camera.GetComponent<cameraCompresser>().rt = vCs [i].rt;
			vCs[i]._camera.GetComponent<cameraCompresser>().vC = vCs [i];
		}
	}

	public void turnCamerasOnOff(){
		for (int i = 0; i < vCs.Length; i++) {
			c = vCs [i]._camera;

			//Turn off camera if target is not within frustum
			if (!GeometryUtility.TestPlanesAABB (GeometryUtility.CalculateFrustumPlanes (c), target.GetComponent<Renderer> ().bounds) 
				&& c.enabled == true) {
				c.enabled = false;
			}
			else if (GeometryUtility.TestPlanesAABB (GeometryUtility.CalculateFrustumPlanes (c), target.GetComponent<Renderer> ().bounds) 
				&& c.enabled == false) {
				c.enabled = true;
			}
		}
	}

	//Called from LevelStart to ensure execution on every level re-load
	public void initialize(){
		Debug.Log ("Initialize start!");

		gM = gameManager.instance;
		Assert.IsTrue (gM != null, "gameManager cannot be null");

		//Find necessary components with CameraRig
		instance.rig = GameObject.Find ("ModCameraRig");
		Assert.IsTrue (instance.rig != null, "rig cannot be null");

		instance.target = (instance.rig.transform.Find ("Camera (eye)").gameObject).transform.Find ("headModel").gameObject;
		Assert.IsTrue (instance.target != null, "Target on GameManager cannot be null");

		instance.cIMHolder = instance.rig.transform.Find ("Controller (right)").gameObject;
		instance.cIM = instance.cIMHolder.GetComponent<controllerInputManager>();
		Assert.IsTrue (instance.cIM != null, "ControllerInputManager not found");

		//Set up initial starting point for player
		Vector3 checkPointObjLoc = adjustHeight(gameManager.getCheckPointObj ().transform.position);
		instance.rig.transform.position = checkPointObjLoc;

		instance.StartCoroutine(displayTextAtCheckPoint(gM.startTxt));

		//Initialize dogArr, if only the scene has dogs (that is if dogArrContainer != null)
		if (dogArrContainer != null) {
			dogArr = new dogBehavior[dogArrContainer.transform.childCount];
			int i = 0;

			foreach (Transform child in dogArrContainer.transform) {
				dogArr [i] = child.GetComponent<dogBehavior> ();
				i++;
			}
		}

		//Enable the timer if timer object reference is found and scene is "survival" type
		if (timerObj != null){
			//&& (SceneManager.GetActiveScene ().name.Contains ("survival") || SceneManager.GetActiveScene ().name.Contains ("Survival") || gameManager.instance.level == 1)) {
			timerActive = true;
		}

		setupVisibility ();

		controllerText = instance.rig.transform.Find ("Controller (left)").transform.Find ("Canvas").transform.Find ("CanvasText").gameObject;
		Assert.IsTrue (controllerText != null, "controllerText cannot be null");
	}

	public IEnumerator displayTextAtCheckPoint (string txt){
		GameObject go = gameManager.getCheckPointObj ();

		GameObject textGo = go.transform.Find ("FloatingText").transform.Find ("CanvasText").gameObject;
		Assert.IsTrue (textGo != null, "Text not found!");

		textGo.SetActive (true);
		textGo.GetComponent<Text>().text = txt;

		yield return new WaitForSecondsRealtime (5f);

		if (textGo != null) {
			textGo.SetActive (false);
		}
	}

	public IEnumerator displayTextOnController (string txt){
		controllerText.SetActive (true);
		controllerText.GetComponent<Text>().text = txt;

		yield return new WaitForSecondsRealtime (5f);

		if (controllerText != null) {
			controllerText.SetActive (false);
		}
	}

	public static dogBehavior getClosestActiveDog (Vector3 loc){
		if (instance.dogArr == null) 
			return null;
		

		float dist, minDist = -1;
		dogBehavior retVal = null;

		foreach (dogBehavior dog in instance.dogArr) {
			//Debug.Log ("dog is " + dog.name);
			dist = (dog.gameObject.transform.position - loc).magnitude;

			//Debug.Log ("dog.DogActive: " + dog.DogActive + " / dist is " + dist + dog.DogActive + " / dogDistanceCutoff is: " + dogDistanceCutoff + " / minDist is:" + minDist);

			if (dog.DogActive == true && dist < dogDistanceCutoff &&
				(minDist == -1 || dist < minDist)) {
				retVal = dog;
				minDist = dist;
			}
		}

		return retVal;
	}

	void updateTimer(){
		timerCurrent -= Time.deltaTime;
		timerObj.text = timerCurrent.ToString ("#");

		if (timerCurrent <= 0) {
			gameManager.beatLevel ();
		}
	}

	Vector3 adjustHeight(Vector3 v3){
		if (SceneManager.GetActiveScene ().name != "level_three") {
			return (v3.x * Vector3.right + v3.z * Vector3.forward);
		} else {
			return v3;
		}
	}
}
