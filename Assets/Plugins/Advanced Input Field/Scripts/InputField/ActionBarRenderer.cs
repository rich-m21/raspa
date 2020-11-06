using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	[RequireComponent(typeof(RectTransform))]
	public class ActionBarRenderer: MonoBehaviour
	{
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
		private RectTransform rectTransform;
		private Canvas canvas;
		private bool initialized;

		public void Initialize()
		{
			rectTransform = GetComponent<RectTransform>();
			canvas = GetComponentInParent<Canvas>();

			if(canvas != null)
			{
				rectTransform.SetParent(canvas.transform);
				rectTransform.localScale = Vector3.one;
				initialized = true;
			}
		}

		public void SyncTransform(RectTransform actionBarRectTransform)
		{
			if(!initialized)
			{
				Initialize();
			}

			Vector3[] corners = new Vector3[4]; //BottomLeft, TopLeft, TopRight, BottomRight
			actionBarRectTransform.GetWorldCorners(corners);

			RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
			for(int i = 0; i < 4; i++)
			{
				corners[i] = canvasRectTransform.InverseTransformPoint(corners[i]);
			}

			Vector2 size = corners[2] - corners[0];
			Vector2 center = Vector3.Lerp(corners[0], corners[2], 0.5f);

			rectTransform.anchoredPosition = center;
			rectTransform.sizeDelta = size;
			rectTransform.SetAsLastSibling();
		}
#endif
	}
}
