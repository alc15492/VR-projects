using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class visibilityCheck : MonoBehaviour {

	//Setup
	public Camera _camera;
	private GameObject target;
	private float detectThresh = 1.2f;
	public RenderTexture rt;

	//Key output data
	public float visScore = 0f;
	public float screenSpaceFactor, colorContrastFactor, posPenFactor; //final factors multiplied together to give visScore, or are are -1 to indicate error
	public float screenSpace, colorContrast, positionPen; //components contributing to "factors"
	private bool detected = false;
	private Vector3 lastLocSpotted;
	private float lastTimeSpotted;

	//getScreenSpace()
	private bool graphicalDebug = false;

	Vector3 center, extents;
	float coefreduc = 0.8f;
	Vector3[] gobounds;
	MeshFilter meshfilter;

	private int hitsFound;
	private float mX, MX, mY, MY;
	private Vector3 tV3;

	//getColorContrast()
	Texture2D i, i2;
	float h, w;
	int xCP, xR, yCP, yR; //X center point, X range

	Color c1, c2;
	int pixCnt, diffPixCnt;
	float colorDiff;
	float rAvg, gAvg, bAvg;
	public bool written = false;

	//getVelPenFactor()
	Rigidbody rb;
	Vector3 perpVel;
	float minVel = 0.05f;

	void Start(){
		target = sceneManager.instance.target;

		Assert.IsTrue (target != null, "Target on VisibilityCheck cannot be null");
		Assert.IsTrue (_camera != null);

	}

	public float getVisScore(){
		return visScore;
	}

	public Vector3 getLastLocSpotted(){
		return lastLocSpotted;
	}

	public float getLastTimeSpotted(){
		return lastTimeSpotted;
	}

	public bool getDetected(){
		return detected;
	}

	void Update(){
		updateVisStatus ();
	}

	//Master function that updates the "visibility score" of the player, based on a specific enemy's perspective. 
	//Specifically, we look at how visible a player's HMD model (a gray sphere) is in a enemy's camera
	//*** - Fetch the compressed camera image from enemy's camera and process it 
	//	  1. Find the size of the player, as a % of the enemy's FOV
	//	  2. Determine how much the player contrasts with the background, from enemy's FOV
	//	  3. Factor to penalize players who are closer to FOV of enemy
	//***
	void updateVisStatus()
	{
		screenSpaceFactor = getScreenSpaceFactor ();

		if (screenSpaceFactor <= 0f) {
			colorContrastFactor = posPenFactor = -1; //these results are unavailable
			visScore = 0f;
			return;
		} 

		colorContrastFactor = getColorContrastFactor ();
		posPenFactor = getPosPenFactor ();

		visScore = screenSpaceFactor * colorContrastFactor * posPenFactor;

		if (visScore > detectThresh) {
			Debug.Log ("spotted! detectThresh is " + detectThresh);
			printState ();
			lastTimeSpotted = Time.time;
			lastLocSpotted = new Vector3 ((mX + MX) / 2, (mY + MY) / 2, (center - _camera.transform.position).magnitude);
			detected = true;

			if (written == false) {
				System.IO.File.WriteAllBytes(Application.dataPath + "/../SomeTestFileName1.png", i.EncodeToPNG());
				System.IO.File.WriteAllBytes(Application.dataPath + "/../SomeTestFileName2.png", i2.EncodeToPNG());
				written = true;
			}
		} else {
			detected = false;
		}

		//*** Notes for calibrating
		//	- Overall - 1 is medium probability of detection
		//	- Screen space - 2.5% is medium, 1% is minimum
		//  - colorContrast - 30% is medium
		//	- velocity - 20%/s is medium, penalize to 1.5th power, standstill should still be 15% speed
		//  - position - edge of screen should be 40% as harsh, so .40 + .60*(1-r)
	}

	//Find what % of the enemy's FOV is occupied by a player - from enemy's eyes the player's head is approximated by a dark gray sphere
	//Most of code for this individual function taken from: http://jeanmeyblum.weebly.com/scripts--tutorials/line-of-sight-for-ai-agent-using-camera-in-unity
	float getScreenSpaceFactor(){
		// if object has a renderer and visible by any camera and is in this camera frustum
		if (!GeometryUtility.TestPlanesAABB (GeometryUtility.CalculateFrustumPlanes (_camera), target.GetComponent<Renderer> ().bounds)) {
			return -2f;
		}
			
		hitsFound = 0;
		MY = MX = 0f;
		mX = mY = 1f;
		RaycastHit hitInfo;

		// by default we use the rough renderer bounds
		center = target.GetComponent<Renderer>().bounds.center;
		extents = target.GetComponent<Renderer>().bounds.extents;
		meshfilter = target.GetComponent<MeshFilter>();

		if(meshfilter != null) // Almost every interesting game object that is render has a mesh
		{
			center = target.transform.position;
			extents = meshfilter.mesh.bounds.extents;
			extents.Scale(target.transform.lossyScale);

			gobounds = new Vector3[33]{ // We can add more or remove some, it increase precision for not too much time or memory cost
				Vector3.zero,
				target.transform.TransformDirection(new Vector3(extents.x,extents.y,extents.z)*0.9f),
				target.transform.TransformDirection(new Vector3(extents.x,extents.y,-extents.z)*0.9f),
				target.transform.TransformDirection(new Vector3(extents.x,-extents.y,extents.z)*0.9f),
				target.transform.TransformDirection(new Vector3(extents.x,-extents.y,-extents.z)*0.9f),
				target.transform.TransformDirection(new Vector3(-extents.x,extents.y,extents.z)*0.9f),
				target.transform.TransformDirection(new Vector3(-extents.x,extents.y,-extents.z)*0.9f),
				target.transform.TransformDirection(new Vector3(-extents.x,-extents.y,extents.z)*0.9f),
				target.transform.TransformDirection(new Vector3(-extents.x,-extents.y,-extents.z)*0.9f),
				target.transform.TransformDirection(new Vector3(extents.x,extents.y,extents.z)*0.5f),
				target.transform.TransformDirection(new Vector3(extents.x,extents.y,-extents.z)*0.5f),
				target.transform.TransformDirection(new Vector3(extents.x,-extents.y,extents.z)*0.5f),
				target.transform.TransformDirection(new Vector3(extents.x,-extents.y,-extents.z)*0.5f),
				target.transform.TransformDirection(new Vector3(-extents.x,extents.y,extents.z)*0.5f),
				target.transform.TransformDirection(new Vector3(-extents.x,extents.y,-extents.z)*0.5f),
				target.transform.TransformDirection(new Vector3(-extents.x,-extents.y,extents.z)*0.5f),
				target.transform.TransformDirection(new Vector3(-extents.x,-extents.y,-extents.z)*0.5f),
				target.transform.TransformDirection(new Vector3(extents.x,extents.y,extents.z)*0.75f),
				target.transform.TransformDirection(new Vector3(extents.x,extents.y,-extents.z)*0.75f),
				target.transform.TransformDirection(new Vector3(extents.x,-extents.y,extents.z)*0.75f),
				target.transform.TransformDirection(new Vector3(extents.x,-extents.y,-extents.z)*0.75f),
				target.transform.TransformDirection(new Vector3(-extents.x,extents.y,extents.z)*0.75f),
				target.transform.TransformDirection(new Vector3(-extents.x,extents.y,-extents.z)*0.75f),
				target.transform.TransformDirection(new Vector3(-extents.x,-extents.y,extents.z)*0.75f),
				target.transform.TransformDirection(new Vector3(-extents.x,-extents.y,-extents.z)*0.75f),
				target.transform.TransformDirection(new Vector3(extents.x,extents.y,extents.z)*0.25f),
				target.transform.TransformDirection(new Vector3(extents.x,extents.y,-extents.z)*0.25f),
				target.transform.TransformDirection(new Vector3(extents.x,-extents.y,extents.z)*0.25f),
				target.transform.TransformDirection(new Vector3(extents.x,-extents.y,-extents.z)*0.25f),
				target.transform.TransformDirection(new Vector3(-extents.x,extents.y,extents.z)*0.25f),
				target.transform.TransformDirection(new Vector3(-extents.x,extents.y,-extents.z)*0.25f),
				target.transform.TransformDirection(new Vector3(-extents.x,-extents.y,extents.z)*0.25f),
				target.transform.TransformDirection(new Vector3(-extents.x,-extents.y,-extents.z)*0.25f)
			};
		}
		else // Only if gameobject has no mesh (= almost never) (Very approximately checking points using the renderer bounds and not the mesh bounds)
		{
			gobounds  = new Vector3[9]{
				Vector3.zero,
				new Vector3(extents.x,extents.y,extents.z)*coefreduc,
				new Vector3(extents.x,extents.y,-extents.z)*coefreduc,
				new Vector3(extents.x,-extents.y,extents.z)*coefreduc,
				new Vector3(extents.x,-extents.y,-extents.z)*coefreduc,
				new Vector3(-extents.x,extents.y,extents.z)*coefreduc,
				new Vector3(-extents.x,extents.y,-extents.z)*coefreduc,
				new Vector3(-extents.x,-extents.y,extents.z)*coefreduc,
				new Vector3(-extents.x,-extents.y,-extents.z)*coefreduc
			};
		}

		//Find maximum bounding box, on camera screen, of visible points
		foreach(Vector3 v in gobounds)
		{
			
			if(GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(_camera), new Bounds(v+center, Vector3.zero)) // if point in viewing frustrum
				&& (!Physics.Linecast (transform.position, v + center, out hitInfo, 1 << LayerMask.NameToLayer ("blocked"),  QueryTriggerInteraction.Ignore))) // if nothing between viewing position and point
			{
				if(graphicalDebug)
				{
					Debug.DrawLine(_camera.transform.position,  v+center, Color.red, 99f, false);
				}

				hitsFound++;

				tV3 = _camera.WorldToViewportPoint (v + center);

				mX = tV3.x < mX ? tV3.x : mX;
				mY = tV3.y < mY ? tV3.y : mY;
				MX = tV3.x > MX ? tV3.x : MX;
				MY = tV3.y > MY ? tV3.y : MY;
			}
		}

		if (hitsFound == 0) {
			return -1f;
		}

		screenSpace = (MX - mX) * (MY - mY);

		return Mathf.Max ((MX - mX) * (MY - mY), .003f)/.003f; 
	}

	//Calculate how much the player head's model (gray) contrasts with the background. More lighting produces higher contrast and higher detection scores. 
	float getColorContrastFactor(){
		//Get compressed camera image, and find the center point/bounds of go, in pixels
		i = _camera.GetComponent<cameraCompresser> ().tex2d;
		i2 = Instantiate(i);
		h = i.height;
		w = i.width;

		xCP = (int) ((MX + mX) * w / 2.0f); // x center point
		xR = (int) Mathf.Floor(xCP - (mX * w)); // x range from center
		yCP = (int) ((MY + mY) * h / 2.0f); // y center point
		yR = (int) Mathf.Floor(yCP - (mY * h)); // y range from center

		//Find the average color of the object being detected
		pixCnt = 0;
		diffPixCnt = 0;
		rAvg = gAvg = bAvg = 0;
		colorDiff = 0f;

		for (int x = xCP - xR; x < xCP + xR; x++) {
			for (int y = yCP - yR; y < yCP + yR; y++) {
				c1 = i.GetPixel (x, y);
				rAvg += c1.r;
				bAvg += c1.b;
				gAvg += c1.g;
				pixCnt++;
			}
		}

		rAvg /= pixCnt;
		bAvg /= pixCnt;
		gAvg /= pixCnt;
		c1 = new Color (rAvg, gAvg, bAvg);

		//Identify pixels in the bounding box
		for (int x = Mathf.Max(xCP - 2*xR, 0); x < Mathf.Min(xCP + 2*xR, w); x++){	
			for (int y = Mathf.Max(yCP - 2*yR, 0); y < Mathf.Min(yCP + 2*yR, h); y++) {
				if(x <= xCP - xR || x >= xCP + xR || y <= yCP - yR || y >= yCP + yR)
				{
					c2 = i.GetPixel (x, y);
					diffPixCnt++;
					colorDiff += Mathf.Sqrt((2 * (c2.r - rAvg)*(c2.r - rAvg) + 4 * (c2.g - gAvg) * (c2.g - gAvg) + 3 * (c2.b - bAvg)*(c2.b - bAvg))/9f); //max color distance is now 1
				}
			}
		}

		i.Apply ();
		if (diffPixCnt == 0) {
			colorDiff = .05f;
		} else {
			colorDiff /= diffPixCnt;
		}
		colorContrast = colorDiff;
	
		return colorContrast / 0.2f;
	}

	//Apply harsher penalty if target is closer to the center of camera's field of vision
	float getPosPenFactor(){
		positionPen = Mathf.Sqrt(((MX + mX)/2f - 0.5f)*((MX + mX)/2f - 0.5f) + ((MY + mY)/2f - 0.5f)*((MY + mY)/2f - 0.5f))/.70f;
		return (0.10f + 0.90f * (1 - positionPen));
	}

	void printState(){
		Debug.Log ("total: " + visScore + 
			" / screenspace: " + screenSpaceFactor + " " + screenSpace +
			" / colorContrast: " + colorContrastFactor + " " + colorContrast +
			" / posPenFactor: " + posPenFactor + " posPen" + positionPen);
	}
}
