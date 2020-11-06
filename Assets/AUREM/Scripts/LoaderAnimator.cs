using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoaderAnimator : MonoBehaviour {

	public Sprite[] frames;
	private int pos = 0;
	public Image load;
	// Use this for initialization
	void Start () {
		StartCoroutine("LoaderAnimation");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator LoaderAnimation(){
		while (App_Manager.instance.authing) {
			yield return new WaitForSeconds(0.5f);
			if (pos >= frames.Length-1) {
				pos = 0;
				load.sprite = frames[pos];
			}
			else {
				pos++;
				load.sprite = frames[pos];
			}
		}
	}
}
