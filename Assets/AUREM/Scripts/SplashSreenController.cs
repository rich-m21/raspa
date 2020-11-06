using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class SplashSreenController : MonoBehaviour {

	public GameObject load;
	public Text loadText;
	public RectTransform new_logo;
	public Image bg;


	// Use this for initialization
	void Start () {
		Invoke("LoadMainScene",3f);
	}
	
	// Update is called once per frame
	void Update () {
		LoadWheelSpin();

	}

	public void LoadMainScene(){
		SceneManager.LoadSceneAsync(1);
	}

	public void LoadWheelSpin(){
		float rot = load.transform.rotation.eulerAngles.z;
		iTween.RotateUpdate(load,iTween.Hash("rotation", new Vector3(0f,0f,rot-(150f*Time.deltaTime)),"time",0f));
	}


}
