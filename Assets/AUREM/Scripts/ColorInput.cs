using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorInput : MonoBehaviour {
	public Color c;
	private InputField i;
	private TouchScreenKeyboard k;
	// Use this for initialization
	void Start () {
		i = this.GetComponent<InputField>();
		c = new Color32(50, 50, 50, 255);
	}

	public void ManageColor(){
		if (i.textComponent.color != c) {
			i.textComponent.color = c;
		}
	}

	// Update is called once per frame
	void Update () {
		if (i.isFocused) {
			if (!k.active) {
				k = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, true, true);
			}
		}
		else {
		}

	}


}
