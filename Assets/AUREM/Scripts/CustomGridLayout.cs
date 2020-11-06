using UnityEngine;
using UnityEngine.UI;
public class CustomGridLayout : MonoBehaviour {
    public float divided = 0f;
    public float celly = 0f;
	private GridLayoutGroup glg;

    // Use this for initialization
    void Start() {
        glg = GetComponent<GridLayoutGroup>();
        RectTransform p = this.GetComponent<RectTransform>();
        int y = (int)celly;
        if (y == 0) {
            glg.cellSize = new Vector2(p.rect.size.x / divided, p.rect.size.x / divided);
        } else {
            glg.cellSize = new Vector2(p.rect.size.x / divided, celly);
        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
