﻿using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	public static class TextExtensions
	{
		public static void UpdateImmediately(this Text textRenderer, bool generateOutOfBounds = true)
		{
			if(textRenderer.cachedTextGenerator != null)
			{
				Vector2 extents = textRenderer.rectTransform.rect.size;
				TextGenerationSettings settings = textRenderer.GetGenerationSettings(extents);
				settings.generateOutOfBounds = generateOutOfBounds;
				textRenderer.cachedTextGenerator.Populate(textRenderer.text, settings);
			}
		}

		public static void UpdateImmediately(this Text textRenderer, string text, bool generateOutOfBounds = true)
		{
			if(textRenderer.cachedTextGenerator != null)
			{
				Vector2 extents = textRenderer.rectTransform.rect.size;
				TextGenerationSettings settings = textRenderer.GetGenerationSettings(extents);
				settings.generateOutOfBounds = generateOutOfBounds;
				textRenderer.cachedTextGenerator.Populate(text, settings);
			}
		}
	}
}
