using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using UnityEngine.UI;

//Handle transitions between scene loads
//Behavior within scenes is handled by the sceneManager
public class gameManager : MonoBehaviour {
	public static gameManager instance;
	private sceneManager sM;

	public int level = 0;
	private int checkpoint = 0;
	public bool debugMode = true;

	private bool firstStart = true;
	public string startTxt { get; set; }

	private bool voiceEnabled = false;
	private voiceInputManager voice;

	private string[] levelNames = { "camera_navigation_test", "NewCitySurvival", "level_three", "victory"};

	void Awake(){
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (instance);
		} 
		else if (instance != this) {
			Destroy (gameObject);
		}
	}
		
	//gameManager is always initialized from sceneManager, to guarantee it is called on every scene load
	public void initialize(){
		instance.sM = GameObject.Find ("SceneManager").GetComponent<sceneManager> ();
		Assert.IsTrue (instance.sM != null, "SceneManager not found");

		if (firstStart == true) {
			startTxt = "Welcome!";
			firstStart = false;
		}

		gameManager.instance.voice = sceneManager.instance.gameObject.GetComponent<voiceInputManager> ();
		Assert.IsTrue (voice != null, "voiceManager not found!");
	}
		
	public static bool setCheckPoint(int val){
		Debug.Log ("New checkpoint attempt: " + val);

		if (val > instance.checkpoint) {
			instance.checkpoint = val;
			instance.StartCoroutine(instance.sM.displayTextAtCheckPoint ("Checkpoint " + val));

			Debug.Log ("New checkpoint set: " + val);

			return true;
		} 
		else {
			return false;
		}
	}

	public static GameObject getCheckPointObj(){
		return instance.sM.checkPointArr [instance.checkpoint];
	}

	public static void antiCheat(){
		Debug.Log ("Moving through walls is not allowed!");
		instance.startTxt = "Moving through walls is not allowed!";
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public static void death(){
		Debug.Log ("You were discovered.");
		instance.startTxt = "You were discovered.";
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public static void restartLevel(){
		Debug.Log ("Welcome back again!");
		instance.startTxt = "Welcome back again!";
		instance.checkpoint = 0;
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public static void beatLevel(){
		if (sceneManager.instance.levelBeaten == false) {
			sceneManager.instance.levelBeaten = true;
			instance.startTxt = "You beat the previous level!";

			Debug.Log ("current level is " + instance.level + " " + SceneManager.GetActiveScene ().name);
			instance.level++;
			instance.checkpoint = 0;
			SceneManager.LoadScene (instance.levelNames [instance.level]);
		}
	}

	public void toggleVoiceEnabled(){
		voiceEnabled = !voiceEnabled;
		Debug.Log ("Voice toggled to " + voiceEnabled);
		StartCoroutine(sceneManager.instance.displayTextOnController ("Voice toggled to " + voiceEnabled));
		voice.enabled = true;
	}
}
