using UnityEngine;
using UnityEngine.UI;

public class ScratchDemoUI : MonoBehaviour {
	public GameObject ProgressCamera;
	public Texture[] Brushes;
	public Material Eraser;
//	public Toggle[] BrushToggles;
	public float Progreso;
	public EraseProgress EraseProgress;
//	public Text LB_premioGanador;

	void Update() {
	}

    public void ProgressFix(){
        EraseProgress.isCompleted = false;
    }

	void Awake() {
		Application.targetFrameRate = 60;
		EraseProgress.OnProgress += OnEraseProgress;
	}

	public void OnChange(bool val) {
		Eraser.mainTexture = Brushes[0];
		PlayerPrefs.SetInt("Brush", 0);
	}

	public void OnCheck(bool check) {
		ProgressCamera.SetActive(true);
		PlayerPrefs.SetInt("Toggle", 0);
	}



	public void OnEraseProgress(float progress) {
		Progreso = Mathf.Round(progress * 100f);
	}
}