using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class cameraCompresser : MonoBehaviour {

	public RenderTexture rt;
	public Texture2D tex2d;
	public visibilityCheck vC;

	private bool written = false;

	void Start(){
		tex2d = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
		RenderTexture.active = rt;
	}

	// Take the image stored by a camera's target render texture, and store it in a 2D texture for later processing by visibilityCheck script
	// Note - OnPostRender is called only happens when camera is turned on by SceneManager function 
	void OnPostRender(){
		if(gameObject.name != "testRenderCamera" && gameObject.name != "testCamera" && vC._camera.enabled == false) return;

		if(Time.frameCount % 30 == 0){
			tex2d.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
			tex2d.Apply();

			/*if (written == false) {
				Debug.Log(helpers.getTopLevelParent(gameObject) + "is the file source!");
				System.IO.File.WriteAllBytes(Application.dataPath + "/../SomeTestFileName.png", tex2d.EncodeToPNG());
				written = true;
			}*/
		}
	}
}
