//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Utility class with helper methods</summary>
	public class Util
	{
		/// <summary>The average thumb size of the user in inches</summary>
		private const float PHYSICAL_THUMB_SIZE = 1;

		public static float DeterminePhysicalScreenSize()
		{
			if(Screen.dpi <= 0)
			{
				return -1;
			}

			float width = Screen.width / Screen.dpi;
			float height = Screen.height / Screen.dpi;
			float screenSize = Mathf.Sqrt(Mathf.Pow(width, 2) + Mathf.Pow(height, 2));

			return screenSize;
		}

		/// <summary>The thumb size in screen pixels (diagonal)</summary>
		public static int DetermineThumbSize()
		{
			float physicalScreenSize = DeterminePhysicalScreenSize();
			if(physicalScreenSize <= 0)
			{
				return -1;
			}
			else
			{
				float aspectRatio = (float)Screen.width / (float)Screen.height;
				float worldHeight = Camera.main.orthographicSize * 2;
				float worldWidth = worldHeight * aspectRatio;

				float normalizedThumbSize = (PHYSICAL_THUMB_SIZE / physicalScreenSize);
				float pixelScreenSize = Mathf.Sqrt(Mathf.Pow(Screen.width, 2) + Mathf.Pow(Screen.height, 2));
				float pixels = (pixelScreenSize * normalizedThumbSize) / 2f;

				return Mathf.RoundToInt(pixels);
			}
		}
	}
}
