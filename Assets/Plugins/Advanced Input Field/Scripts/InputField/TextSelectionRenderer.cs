//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Renderer for text selection</summary>
	[ExecuteInEditMode]
	[Serializable]
	public class TextSelectionRenderer: Graphic
	{
		/// <summary>The start position of the selection</summary>
		private int startPosition;

		/// <summary>The end position of the selection</summary>
		private int endPosition;

		private AdvancedInputField InputField { get; set; }

		public Text TextRenderer
		{
			get
			{
				if(InputField.LiveProcessing)
				{
					return InputField.ProcessedTextRenderer;
				}
				else
				{
					return InputField.TextRenderer;
				}
			}
		}

		public void Initialize(AdvancedInputField inputField)
		{
			InputField = inputField;
		}

		/// <summary>Updates the selection</summary>
		/// <param name="startPosition">The start position of the selection</param>
		/// <param name="endPosition">The end position of the selection</param>
		public void UpdateSelection(int startPosition, int endPosition, bool forceUpdate = false)
		{
			TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
			startPosition = Mathf.Clamp(startPosition, 0, textGenerator.characterCount - 1);
			endPosition = Mathf.Clamp(endPosition, 0, textGenerator.characterCount - 1);

			if(forceUpdate || this.startPosition != startPosition || this.endPosition != endPosition) //Only redraw if selection has changed
			{
				this.startPosition = startPosition;
				this.endPosition = endPosition;
				SetVerticesDirty();
			}
		}

		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			vertexHelper.Clear();

			if(startPosition == -1 || endPosition == -1 || startPosition == endPosition)
			{
				return;
			}

			TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
			int length = textGenerator.lineCount;
			bool foundStart = false;
			bool foundEnd = false;
			for(int i = 0; i < length; i++)
			{
				UILineInfo lineInfo = textGenerator.lines[i];

				int startCharIndex = GetLineStartCharIndex(textGenerator, i);
				int endCharIndex = GetLineEndCharIndex(textGenerator, i);
				if(endCharIndex < startPosition)
				{
					continue;
				}

				UICharInfo startCharInfo;
				UICharInfo endCharInfo;

				if(!foundStart)
				{
					foundStart = true;
					startCharInfo = textGenerator.characters[startPosition];
				}
				else
				{
					startCharInfo = textGenerator.characters[startCharIndex];
				}

				if(endPosition <= endCharIndex)
				{
					foundEnd = true;
					endCharInfo = textGenerator.characters[endPosition];
				}
				else
				{
					endCharInfo = textGenerator.characters[endCharIndex];
				}

				Vector2 bottomLeftCorner = startCharInfo.cursorPos;
				bottomLeftCorner.y -= lineInfo.height;
				Vector2 topRightCorner = endCharInfo.cursorPos;
				//topRightCorner.x += endCharInfo.charWidth;

				bottomLeftCorner /= canvas.scaleFactor;
				topRightCorner /= canvas.scaleFactor;

				AddRectangle(bottomLeftCorner, topRightCorner, ref vertexHelper);

				if(foundEnd)
				{
					break;
				}
			}
		}

		/// <summary>Gets the start char position of the line</summary>
		/// <param name="line">The line to use</param>
		private static int GetLineStartCharIndex(TextGenerator textGenerator, int line)
		{
			line = Mathf.Clamp(line, 0, textGenerator.lines.Count - 1);
			return textGenerator.lines[line].startCharIdx;
		}

		/// <summary>Gets the end char position of the line</summary>
		/// <param name="line">The line to use</param>
		private static int GetLineEndCharIndex(TextGenerator textGenerator, int line)
		{
			line = Mathf.Max(line, 0);
			if(line + 1 < textGenerator.lines.Count)
			{
				return textGenerator.lines[line + 1].startCharIdx - 1;
			}

			return textGenerator.characterCountVisible;
		}

		/// <summary>Adds a rectangle to VertexHelper</summary>
		/// <param name="bottomLeftCorner">The bottom left corner of the rectangle</param>
		/// <param name="topRightCorner">The top right corner of the rectangle</param>
		/// <param name="vertexHelper">The VertexHelper to use</param>
		public void AddRectangle(Vector2 bottomLeftCorner, Vector2 topRightCorner, ref VertexHelper vertexHelper)
		{
			UIVertex vertex = UIVertex.simpleVert;

			vertex.position = new Vector2(bottomLeftCorner.x, bottomLeftCorner.y);
			vertex.color = color;
			vertexHelper.AddVert(vertex);

			vertex.position = new Vector2(bottomLeftCorner.x, topRightCorner.y);
			vertex.color = color;
			vertexHelper.AddVert(vertex);

			vertex.position = new Vector2(topRightCorner.x, topRightCorner.y);
			vertex.color = color;
			vertexHelper.AddVert(vertex);

			vertex.position = new Vector2(topRightCorner.x, bottomLeftCorner.y);
			vertex.color = color;
			vertexHelper.AddVert(vertex);

			int vertexCount = vertexHelper.currentVertCount;
			vertexHelper.AddTriangle(vertexCount - 4, vertexCount - 3, vertexCount - 2);
			vertexHelper.AddTriangle(vertexCount - 2, vertexCount - 1, vertexCount - 4);
		}
	}
}