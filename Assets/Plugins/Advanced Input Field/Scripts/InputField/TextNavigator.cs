//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	/// <summary>The base class for text navigation using the caret and selecting text</summary>
	public class TextNavigator
	{
		/// <summary>The characters used for word separation</summary>
		protected readonly char[] WORD_SEPARATOR_CHARS = { ' ', '.', ',', '\t', '\r', '\n' };

		/// <summary>The main renderer of the text</summary>
		public Text TextRenderer { get { return InputField.TextRenderer; } }

		public Text ProcessedTextRenderer { get { return InputField.ProcessedTextRenderer; } }

		/// <summary>The renderer of the caret</summary>
		public Image CaretRenderer { get; private set; }

		/// <summary>The renderer for text selection</summary>
		public TextSelectionRenderer SelectionRenderer { get; private set; }

		/// <summary>The InputField</summary>
		public AdvancedInputField InputField { get; private set; }

		/// <summary>The caret position</summary>
		protected int caretPosition;

		/// <summary>The caret position of the processed text when using live processing filters</summary>
		protected int processedCaretPosition;

		/// <summary>The start position of the text selection</summary>
		protected int selectionStartPosition;

		/// <summary>The start position of the processed text selection when using live processing filters</summary>
		protected int processedSelectionStartPosition;

		/// <summary>The end position of the text selection</summary>
		protected int selectionEndPosition;

		/// <summary>The end position of the processed text selection when using live processing filters</summary>
		protected int processedSelectionEndPosition;

		/// <summary>The current time for caret blink</summary>
		protected float caretBlinkTime;

		/// <summary>The text offset used for text scroll</summary>
		protected int textOffset;

		/// <summary>The text offset used for text scroll</summary>
		protected int textDrawEnd;

		/// <summary>Indicates if edit mode is enabled</summary>
		public bool EditMode { get; protected set; }

		/// <summary>Stack of previously rendered lines (used for text scroll)</summary>
		private Stack<int> previousLineWidthStack;

		/// <summary>The main text string</summary>
		public string Text { get { return InputField.Text; } }

		/// <summary>The processed text string (for live processing)</summary>
		public string ProcessedText { get { return InputField.ProcessedText; } }

		/// <summary>The text currently being rendered</summary>
		public string RenderedText { get { return TextRenderer.text; } }

		/// <summary>The processed text currently being rendered (for live processing)</summary>
		public string ProcessedRenderedText { get { return ProcessedTextRenderer.text; } }

		/// <summary>The Canvas</summary>
		public Canvas Canvas { get { return InputField.Canvas; } }

		/// <summary>The text offset used for text scroll</summary>
		public int TextOffset
		{
			get
			{
				return textOffset;
			}
			protected set
			{
				if(textOffset != value)
				{
					textOffset = value;
					RefreshRenderedText();
				}
			}
		}

		/// <summary>The caret position</summary>
		public virtual int CaretPosition
		{
			get { return caretPosition; }
			set
			{
				caretPosition = value;
				if(InputField.LiveProcessing)
				{
					string text = InputField.Text;
					string processedText = InputField.ProcessedText;
					ProcessedCaretPosition = InputField.LiveProcessingFilter.DetermineProcessedCaret(text, caretPosition, processedText);
				}
				else
				{
					MoveTextIfNeeded();
					UpdateCaret();
				}
			}
		}

		/// <summary>The processed caret position (for live processing)</summary>
		public virtual int ProcessedCaretPosition
		{
			get { return processedCaretPosition; }
			set
			{
				processedCaretPosition = value;

				string text = InputField.Text;
				string processedText = InputField.ProcessedText;
				caretPosition = InputField.LiveProcessingFilter.DetermineCaret(text, processedText, processedCaretPosition);
				UpdateCaret();
			}
		}

		/// <summary>The caret position in currently rendered text</summary>
		public virtual int VisibleCaretPosition
		{
			get
			{
				if(InputField.LiveProcessing)
				{
					return ProcessedCaretPosition - TextOffset;
				}
				else
				{
					return CaretPosition - TextOffset;
				}
			}
			set
			{
				if(InputField.LiveProcessing)
				{
					ProcessedCaretPosition = TextOffset + value;
				}
				else
				{
					CaretPosition = TextOffset + value;
				}
			}
		}

		/// <summary>The start position of the text selection</summary>
		public virtual int SelectionStartPosition
		{
			get { return selectionStartPosition; }
			protected set
			{
				selectionStartPosition = value;
				if(InputField.LiveProcessing)
				{
					string text = InputField.Text;
					string processedText = InputField.ProcessedText;
					processedSelectionStartPosition = InputField.LiveProcessingFilter.DetermineProcessedCaret(text, selectionStartPosition, processedText);
				}

				SelectionRenderer.UpdateSelection(VisibleSelectionStartPosition, VisibleSelectionEndPosition);
			}
		}

		/// <summary>The start position of the processed text selection (for live processing)</summary>
		public virtual int ProcessedSelectionStartPosition
		{
			get { return processedSelectionStartPosition; }
			protected set
			{
				processedSelectionStartPosition = value;

				string text = InputField.Text;
				string processedText = InputField.ProcessedText;
				selectionStartPosition = InputField.LiveProcessingFilter.DetermineCaret(text, processedText, processedSelectionStartPosition);
				SelectionRenderer.UpdateSelection(VisibleSelectionStartPosition, VisibleSelectionEndPosition);
			}
		}

		/// <summary>The start position of the text selection in currently rendered text</summary>
		public virtual int VisibleSelectionStartPosition
		{
			get
			{
				if(InputField.LiveProcessing)
				{
					return ProcessedSelectionStartPosition - TextOffset;
				}
				else
				{
					return SelectionStartPosition - TextOffset;
				}
			}
			set
			{
				if(InputField.LiveProcessing)
				{
					ProcessedSelectionStartPosition = TextOffset + value;
				}
				else
				{
					SelectionStartPosition = TextOffset + value;
				}
			}
		}

		/// <summary>The end position of the text selection</summary>
		public virtual int SelectionEndPosition
		{
			get { return selectionEndPosition; }
			protected set
			{
				selectionEndPosition = value;
				if(InputField.LiveProcessing)
				{
					string text = InputField.Text;
					string processedText = InputField.ProcessedText;
					processedSelectionEndPosition = InputField.LiveProcessingFilter.DetermineProcessedCaret(text, selectionEndPosition, processedText);
				}

				SelectionRenderer.UpdateSelection(VisibleSelectionStartPosition, VisibleSelectionEndPosition);
			}
		}

		/// <summary>The end position of the processed text selection (for live processing)</summary>
		public virtual int ProcessedSelectionEndPosition
		{
			get { return processedSelectionEndPosition; }
			protected set
			{
				processedSelectionEndPosition = value;

				string text = InputField.Text;
				string processedText = InputField.ProcessedText;
				selectionEndPosition = InputField.LiveProcessingFilter.DetermineCaret(text, processedText, processedSelectionEndPosition);
				SelectionRenderer.UpdateSelection(VisibleSelectionStartPosition, VisibleSelectionEndPosition);
			}
		}

		/// <summary>The end position of the text selection in currently rendered text</summary>
		public virtual int VisibleSelectionEndPosition
		{
			get
			{
				if(InputField.LiveProcessing)
				{
					return ProcessedSelectionEndPosition - TextOffset;
				}
				else
				{
					return SelectionEndPosition - TextOffset;
				}
			}
			set
			{
				if(InputField.LiveProcessing)
				{
					ProcessedSelectionEndPosition = TextOffset + value;
				}
				else
				{
					SelectionEndPosition = TextOffset + value;
				}
			}
		}

		/// <summary>The currently selected text</summary>
		public string SelectedText
		{
			get
			{
				if(HasSelection)
				{
					return Text.Substring(SelectionStartPosition, SelectionEndPosition - SelectionStartPosition);
				}
				return string.Empty;
			}
		}

		/// <summary>The character count of the rendered text</summary>
		public int CharacterCount
		{
			get { return TextRenderer.cachedTextGenerator.characterCount; }
		}

		/// <summary>The character count of the whole text</summary>
		public int TotalCharacterCount
		{
			get { return InputField.Text.Length; }
		}

		/// <summary>The line count</summary>
		public int LineCount
		{
			get { return TextRenderer.cachedTextGenerator.lineCount; }
		}

		/// <summary>Indicates if some text is currently selected</summary>
		public bool HasSelection
		{
			get { return (SelectionEndPosition - SelectionStartPosition) != 0; }
		}

		/// <summary>Indicates whether to currrently block selection change events from being send to the native bindings</summary>
		public bool BlockNativeSelectionChange { get; set; }

		/// <summary>Initializes this class</summary>
		public virtual void Initialize(AdvancedInputField inputField, Image caretRenderer, TextSelectionRenderer selectionRenderer)
		{
			InputField = inputField;
			CaretRenderer = caretRenderer;
			SelectionRenderer = selectionRenderer;

			CaretRenderer.rectTransform.sizeDelta = new Vector2(InputField.CaretWidth, TextRenderer.fontSize);
			CaretRenderer.color = InputField.CaretColor;
			SelectionRenderer.color = InputField.SelectionColor;
			previousLineWidthStack = new Stack<int>();

			SelectionRenderer.Initialize(inputField);
		}

		public void UpdateSelection()
		{
			SelectionRenderer.UpdateSelection(VisibleSelectionStartPosition, VisibleSelectionEndPosition, true);
		}

		public virtual void OnCanvasScaleChanged(float canvasScaleFactor)
		{
			UpdateSelection();
		}

		/// <summary>Gets the character index from position</summary>
		/// <param name="position">The position to use</param>
		public int GetCharacterIndexFromPosition(Text textRenderer, Vector2 position)
		{
			TextGenerator textGenerator = textRenderer.cachedTextGenerator;

			if(textGenerator.lineCount == 0)
			{
				return 0;
			}

			int line = GetUnclampedCharacterLineFromPosition(position, textGenerator);
			if(line < 0)
			{
				return 0;
			}
			else if(line >= textGenerator.lineCount)
			{
				return textGenerator.characterCountVisible;
			}

			int startCharIndex = textGenerator.lines[line].startCharIdx;
			int endCharIndex = GetLineEndCharIndex(textGenerator, line);

			for(int i = startCharIndex; i < endCharIndex; i++)
			{
				if(i >= textGenerator.characterCountVisible)
				{
					break;
				}

				UICharInfo charInfo = textGenerator.characters[i];
				Vector2 charPos = charInfo.cursorPos / textRenderer.pixelsPerUnit;

				float distToCharStart = position.x - charPos.x;
				float distToCharEnd = charPos.x + (charInfo.charWidth / textRenderer.pixelsPerUnit) - position.x;
				if(distToCharStart < distToCharEnd)
				{
					return i;
				}
			}

			return endCharIndex;
		}

		/// <summary>Gets the unclamped character line from position</summary>
		/// <param name="position">The position to use</param>
		/// <param name="textGenerator">The text generator to use</param>
		private int GetUnclampedCharacterLineFromPosition(Vector2 position, TextGenerator textGenerator)
		{
			if(!InputField.Multiline)
			{
				return 0;
			}

			float y = position.y * TextRenderer.pixelsPerUnit;
			float lastBottomY = 0.0f;

			for(int i = 0; i < textGenerator.lineCount; ++i)
			{
				float topY = textGenerator.lines[i].topY;
				float bottomY = topY - textGenerator.lines[i].height;

				if(y > topY)
				{
					float leading = topY - lastBottomY;
					if(y > topY - (0.5f * leading))
					{
						return i - 1;
					}
					else
					{
						return i;
					}
				}

				if(y > bottomY)
				{
					return i;
				}

				lastBottomY = bottomY;
			}

			return textGenerator.lineCount;
		}

		/// <summary>Refreshes the rendered text</summary>
		public void RefreshRenderedText()
		{
			string text = null;
			if(InputField.LiveProcessing)
			{
				text = ProcessedText;

				if(ProcessedRenderedText != text)
				{
					InputField.RenderedText = text;
				}
			}
			else
			{
				if(Text.Length > 0)
				{
					if(TextOffset >= Text.Length)
					{
						TextOffset = 0;
						previousLineWidthStack.Clear();
					}

					text = Text.Substring(TextOffset);

					if(InputField.Secure)
					{
						text = new string('*', text.Length);
					}
				}
				else
				{
					text = Text;
				}

				if(RenderedText != text)
				{
					if(!InputField.LiveProcessing)
					{
						Text textRenderer = InputField.TextRenderer;
						textRenderer.text = text;

						if(InputField.LineType == LineType.SingleLine)
						{
							textRenderer.UpdateImmediately(true);
							TextGenerator textGenerator = textRenderer.cachedTextGenerator;

							if(textGenerator.lineCount > 1) //We have text overflowing
							{
								int textDrawStart, textDrawEnd;
								DetermineDrawRange(textGenerator, out textDrawStart, out textDrawEnd);
								textOffset = textDrawStart;
								text = Text.Substring(textDrawStart, textDrawEnd - textDrawStart);
								if(InputField.Secure)
								{
									text = new string('*', text.Length);
								}
							}
						}
						else
						{
							textRenderer.UpdateImmediately(false);
						}
					}

					InputField.RenderedText = text;
				}
			}
		}

		public void DetermineDrawRange(TextGenerator textGenerator, out int textDrawStart, out int textDrawEnd)
		{
			textDrawStart = -1;
			textDrawEnd = -1;

			if(InputField.LineType == LineType.SingleLine)
			{
				int firstLineEndCharIndex = TextOffset + GetLineEndCharIndex(textGenerator, 0);
				IList<UICharInfo> characters = textGenerator.characters;
				float width = 0;
				float maxWidth = textGenerator.rectExtents.size.x;
				int charAmount = 0;

				if(VisibleCaretPosition > firstLineEndCharIndex)
				{
					textDrawEnd = CaretPosition;

					for(int i = VisibleCaretPosition; i > 0; i--) //Caret position should be last visible character
					{
						UICharInfo charInfo = characters[i];
						if(width + charInfo.charWidth > maxWidth)
						{
							break;
						}

						width += charInfo.charWidth;
						charAmount++;
					}

					textDrawStart = textDrawEnd - charAmount;
				}
				else
				{
					textDrawStart = TextOffset;

					int length = characters.Count;
					for(int i = 0; i < length; i++) //Try to fit as much characters as possible on first line (ignoring the word wrap)
					{
						UICharInfo charInfo = characters[i];
						if(width + charInfo.charWidth > maxWidth)
						{
							break;
						}

						width += charInfo.charWidth;
						charAmount++;
					}

					textDrawEnd = TextOffset + charAmount;
				}
			}
			else
			{
				Debug.LogError("This shouldn't be called, multiline is not supported properly yet");
			}
		}

		/// <summary>Gets the character index  of the line start</summary>
		/// <param name="position">The line to check</param>
		public int GetLineStartCharIndex(TextGenerator textGenerator, int line)
		{
			line = Mathf.Clamp(line, 0, textGenerator.lines.Count - 1);
			return textGenerator.lines[line].startCharIdx;
		}

		/// <summary>Gets the character index  of the line end</summary>
		/// <param name="position">The line to check</param>
		public int GetLineEndCharIndex(TextGenerator textGenerator, int line)
		{
			line = Mathf.Max(line, 0);
			if(line + 1 < textGenerator.lines.Count)
			{
				return textGenerator.lines[line + 1].startCharIdx - 1;
			}

			return textGenerator.characterCountVisible;
		}

		private void MoveTextIfNeeded()
		{
			if(CaretOutOfBounds())
			{
				TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
				int firstLineStartCharIndex = TextOffset + GetLineStartCharIndex(textGenerator, 0);
				int endLineEndCharIndex = TextOffset + GetLineEndCharIndex(textGenerator, 0);
				if(InputField.LineType == LineType.SingleLine)
				{
					if(caretPosition < firstLineStartCharIndex)
					{
						MoveLineLeft(firstLineStartCharIndex - CaretPosition);
					}
					else
					{
						MoveLineRight(CaretPosition - endLineEndCharIndex);
					}
				}
				else
				{
					if(caretPosition < firstLineStartCharIndex)
					{
						MoveLineUp();
					}
					else
					{
						MoveLineDown();
					}
				}
			}
		}

		/// <summary>Updates the caret position</summary>
		public virtual void UpdateCaret()
		{
			if(InputField.LiveProcessing)
			{
				UpdateProcessedCaret();
			}
			else
			{
				UpdateDefaultCaret();
			}
		}

		public virtual void UpdateDefaultCaret()
		{
			TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
			bool fixEmptyCaret = false;
			if(textGenerator.characterCount == 0) //Workaround to make sure the text generator will give a correct position for the first character
			{
				fixEmptyCaret = true;
				TextRenderer.UpdateImmediately(" ");
			}

			int index = Mathf.Clamp(VisibleCaretPosition, 0, textGenerator.characterCount - 1);
			UICharInfo charInfo = textGenerator.characters[index];

			Vector2 cursorPosition = charInfo.cursorPos;
			if(VisibleCaretPosition >= textGenerator.characterCountVisible)
			{
				cursorPosition.x += charInfo.charWidth;
			}

			if(Canvas != null)
			{
				CaretRenderer.rectTransform.anchoredPosition = cursorPosition / Canvas.scaleFactor;
			}

			if(fixEmptyCaret)
			{
				TextRenderer.UpdateImmediately(string.Empty);
			}
		}

		public virtual void UpdateProcessedCaret()
		{
			TextGenerator textGenerator = ProcessedTextRenderer.cachedTextGenerator;
			bool fixEmptyCaret = false;
			if(textGenerator.characterCount == 0) //Workaround to make sure the text generator will give a correct position for the first character
			{
				fixEmptyCaret = true;
				ProcessedTextRenderer.UpdateImmediately(" ");
			}

			int index = Mathf.Clamp(ProcessedCaretPosition, 0, textGenerator.characterCount - 1);
			UICharInfo charInfo = textGenerator.characters[index];

			Vector2 cursorPosition = charInfo.cursorPos;
			if(ProcessedCaretPosition >= textGenerator.characterCountVisible)
			{
				cursorPosition.x += charInfo.charWidth;
			}

			if(Canvas != null)
			{
				CaretRenderer.rectTransform.anchoredPosition = cursorPosition / Canvas.scaleFactor;
			}

			if(fixEmptyCaret)
			{
				ProcessedTextRenderer.UpdateImmediately(string.Empty);
			}
		}

		/// <summary>Begins Edit mode</summary>
		public virtual void BeginEditMode()
		{
			if(!EditMode)
			{
				EditMode = true;
				caretBlinkTime = 0;
				CaretRenderer.enabled = true;
			}
		}

		/// <summary>Ends Edit mode</summary>
		public virtual void EndEditMode()
		{
			EditMode = false;
			caretBlinkTime = InputField.CaretBlinkRate;
			CaretRenderer.enabled = false;
			CaretPosition = 0;
			CancelSelection();

			TextOffset = 0;
		}

		/// <summary>Update method when selected</summary>
		public virtual void UpdateSelected()
		{
			if(EditMode)
			{
				if(!HasSelection)
				{
					UpdateCaretBlink();
				}
				else
				{
					CaretRenderer.enabled = false; //Don't show caret if we have text selected
				}
			}
		}

		/// <summary>Updates the caret blink</summary>
		private void UpdateCaretBlink()
		{
			caretBlinkTime += Time.deltaTime;
			caretBlinkTime = Mathf.Min(caretBlinkTime, InputField.CaretBlinkRate);
			float progress = caretBlinkTime / InputField.CaretBlinkRate;
			if(progress <= 0.5f)
			{
				CaretRenderer.enabled = true;
			}
			else
			{
				CaretRenderer.enabled = false;
			}

			if(progress == 1)
			{
				caretBlinkTime = 0;
			}
		}

		/// <summary>Moves caret to start of the text</summary>
		public void MoveToStart()
		{
			CaretPosition = 0;
		}

		/// <summary>Moves caret to end of the text</summary>
		public void MoveToEnd()
		{
			CaretPosition = Text.Length;
		}

		/// <summary>Select current word at caret position</summary>
		public void SelectCurrentWord()
		{
			string renderedText = null;
			if(InputField.LiveProcessing)
			{
				renderedText = ProcessedRenderedText;
			}
			else
			{
				renderedText = RenderedText;
			}

			VisibleSelectionStartPosition = FindPreviousWordStart(VisibleCaretPosition, renderedText);
			VisibleSelectionEndPosition = FindNextWordStart(VisibleCaretPosition, renderedText);

			string selectedString = renderedText.Substring(VisibleSelectionStartPosition, VisibleSelectionEndPosition - VisibleSelectionStartPosition);
			int separatorIndex = selectedString.IndexOfAny(WORD_SEPARATOR_CHARS);
			if(separatorIndex != -1) //There a 2 words selected
			{
				int word1EndIndex = VisibleSelectionStartPosition + separatorIndex;
				int word2StartIndex = VisibleSelectionStartPosition + 1 + selectedString.LastIndexOfAny(WORD_SEPARATOR_CHARS);

				if(VisibleCaretPosition - word1EndIndex < word2StartIndex - VisibleCaretPosition) //Previous word is closer
				{
					VisibleSelectionEndPosition = word1EndIndex;
				}
				else //Next word is closer
				{
					VisibleSelectionStartPosition = word2StartIndex;
				}
			}
		}

		/// <summary>Selects all text</summary>
		public void SelectAll()
		{
			SelectionStartPosition = 0;
			SelectionEndPosition = Text.Length;
		}

		/// <summary>Finds the start of previous word</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <param name="text">The text to use</param>
		/// <returns>The start position of previous word</returns>
		public int FindPreviousWordStart(int position, string text)
		{
			if(position - 2 < 0)
			{
				return 0;
			}

			int wordSeparatorPosition = text.LastIndexOfAny(WORD_SEPARATOR_CHARS, position - 2);
			if(wordSeparatorPosition == -1)
			{
				wordSeparatorPosition = 0;
			}
			else
			{
				wordSeparatorPosition++;
			}

			return wordSeparatorPosition;
		}

		/// <summary>Finds the start of next word</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <param name="text">The text to use</param>
		/// <returns>The start position of next word</returns>
		public int FindNextWordStart(int position, string text)
		{
			if(position + 1 >= text.Length)
			{
				return text.Length;
			}

			int wordSeparatorPosition = text.IndexOfAny(WORD_SEPARATOR_CHARS, position + 1);
			if(wordSeparatorPosition == -1)
			{
				wordSeparatorPosition = text.Length;
			}
			else
			{
				wordSeparatorPosition++;
			}

			return wordSeparatorPosition;
		}

		/// <summary>Finds the character position of next new line</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <returns>The character position of next new line</returns>
		public int NewLineDownPosition(int position)
		{
			if(InputField.LineType == LineType.SingleLine)
			{
				return Text.Length;
			}

			if(position + 1 >= Text.Length)
			{
				return Text.Length - 1;
			}

			int newLinePosition = Text.IndexOf('\n', position + 1);
			if(newLinePosition == -1)
			{
				return Text.Length - 1;
			}

			return newLinePosition;
		}

		/// <summary>Finds the character position of next line</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <returns>The character position of next line</returns>
		public int LineDownPosition(int position)
		{
			if(InputField.LineType == LineType.SingleLine)
			{
				return Text.Length;
			}

			TextGenerator textGenerator = TextRenderer.cachedTextGenerator;

			UICharInfo originChar = textGenerator.characters[position];
			int originLine = DetermineCharacterLine(textGenerator, position);

			if(originLine + 1 >= textGenerator.lineCount) // We are on the last line return last character
			{
				return CharacterCount - 1;
			}

			int endCharIdx = GetLineEndCharIndex(textGenerator, originLine + 1); // Need to determine end line for next line.

			for(int i = textGenerator.lines[originLine + 1].startCharIdx; i < endCharIdx; ++i)
			{
				if(textGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
				{
					return i;
				}
			}
			return endCharIdx;
		}

		/// <summary>Finds the character position of previous new line</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <returns>The character position of previous new line</returns>
		public int NewLineUpPosition(int position)
		{
			if(InputField.LineType == LineType.SingleLine)
			{
				return 0;
			}

			if(position - 1 <= 0)
			{
				return 0;
			}

			int newLinePosition = Text.LastIndexOf('\n', position - 1, position);
			if(newLinePosition == -1)
			{
				return 0;
			}

			return newLinePosition;
		}

		/// <summary>Finds the character position of previous line</summary>
		/// <param name="position">The character position to start checking from</param>
		/// <returns>The character position of previous line</returns>
		public int LineUpPosition(int position)
		{
			if(InputField.LineType == LineType.SingleLine)
			{
				return 0;
			}

			TextGenerator textGenerator = TextRenderer.cachedTextGenerator;

			UICharInfo originChar = textGenerator.characters[position];
			int originLine = DetermineCharacterLine(textGenerator, position);

			if(originLine <= 0) // We are on the first line return first character
			{
				return 0;
			}

			int endCharIdx = textGenerator.lines[originLine].startCharIdx - 1;

			for(int i = textGenerator.lines[originLine - 1].startCharIdx; i < endCharIdx; ++i)
			{
				if(textGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
				{
					return i;
				}
			}
			return endCharIdx;
		}

		/// <summary>Cancels current text selection</summary>
		public void CancelSelection()
		{
			SelectionStartPosition = 0;
			SelectionEndPosition = 0;
		}

		/// <summary>Determines the character line of character position</summary>
		/// <param name="textGenerator">The TextGenerator to use</param>
		/// <param name="charPosition">The character position to check</param>
		/// <returns>The character line</returns>
		protected int DetermineCharacterLine(TextGenerator textGenerator, int charPosition)
		{
			for(int i = 0; i < textGenerator.lineCount - 1; ++i)
			{
				if(textGenerator.lines[i + 1].startCharIdx > charPosition)
				{
					return i;
				}
			}

			return textGenerator.lineCount - 1;
		}

		/// <summary>Moves caret</summary>
		/// <param name="amount">The amount to move the caret</param>
		public void MoveCaret(int amount)
		{
			CaretPosition += amount;
		}

		/// <summary>Sets caret position to selection start</summary>
		public void CaretToSelectionStart()
		{
			CaretPosition = SelectionStartPosition;
		}

		/// <summary>Sets caret position to selection end</summary>
		public void CaretToSelectionEnd()
		{
			CaretPosition = SelectionEndPosition;
		}

		/// <summary>Sets selection end position to selection start</summary>
		public void SelectionEndToSelectionStart()
		{
			SelectionEndPosition = SelectionStartPosition;
		}

		/// <summary>Resets the caret based on position</summary>
		/// <param name="position">The position to check</param>
		public void ResetCaret(Vector2 position)
		{
			if(CharacterCount > Text.Length + 1) //Text hasn't been updated yet, probably was disabled
			{
				VisibleCaretPosition = 0;
			}
			else
			{
				if(InputField.LiveProcessing)
				{
					VisibleCaretPosition = GetCharacterIndexFromPosition(ProcessedTextRenderer, position);
				}
				else
				{
					VisibleCaretPosition = GetCharacterIndexFromPosition(TextRenderer, position);
				}
			}

			VisibleSelectionStartPosition = VisibleCaretPosition;
			VisibleSelectionEndPosition = VisibleCaretPosition;
		}

		/// <summary>Updates the selection area when dragging</summary>
		/// <param name="currentPosition">The current position</param>
		/// <param name="pressPosition">The position when press started</param>
		/// <param name="autoSelectWord">Indicates whether current word should be selected automatically</param>
		public void UpdateSelectionArea(int currentPosition, int pressPosition, bool autoSelectWord)
		{
			int adjustedPosition = currentPosition + TextOffset;
			if(adjustedPosition < pressPosition)
			{
				if(InputField.LiveProcessing)
				{
					ProcessedSelectionStartPosition = adjustedPosition;
				}
				else
				{
					SelectionStartPosition = adjustedPosition;
					if(autoSelectWord)
					{
						SelectionEndPosition = FindNextWordStart(pressPosition, Text);
					}
				}
			}
			else
			{
				if(InputField.LiveProcessing)
				{
					ProcessedSelectionEndPosition = adjustedPosition;
				}
				else
				{
					if(autoSelectWord)
					{
						SelectionStartPosition = FindPreviousWordStart(pressPosition, Text);
					}
					SelectionEndPosition = adjustedPosition;
				}
			}
		}

		/// <summary>Method called when InputField gets destroyed</summary>
		public virtual void OnDestroy()
		{
		}

		/// <summary>Scrolls text on current line to the left</summary>
		public void MoveLineLeft(int moveAmount)
		{
			TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
			if(textGenerator.characterCount == 0 || textGenerator.lineCount != 1 || TextOffset == 0)
			{
				return;
			}

			TextOffset -= moveAmount;

			if(HasSelection)
			{
				SelectionRenderer.UpdateSelection(VisibleSelectionStartPosition, VisibleSelectionEndPosition);
			}
		}

		/// <summary>Scrolls text on current line to the right</summary>
		public void MoveLineRight(int moveAmount)
		{
			TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
			if(textGenerator.characterCount == 0 || textGenerator.lineCount != 1 || TextOffset + textGenerator.characterCountVisible == InputField.Text.Length)
			{
				return;
			}

			TextOffset += moveAmount;

			if(HasSelection)
			{
				SelectionRenderer.UpdateSelection(VisibleSelectionStartPosition, VisibleSelectionEndPosition);
			}
		}

		/// <summary>Scrolls one line down</summary>
		public void MoveLineDown()
		{
			TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
			if(textGenerator.characterCount == 0 || textGenerator.lineCount <= 1
				|| TextOffset + textGenerator.characterCountVisible == InputField.Text.Length)
			{
				return;
			}

			int firstLineLength = GetLineEndCharIndex(textGenerator, 0) + 1;
			previousLineWidthStack.Push(firstLineLength);

			TextOffset += firstLineLength;

			if(HasSelection)
			{
				SelectionRenderer.UpdateSelection(VisibleSelectionStartPosition, VisibleSelectionEndPosition);
			}
		}

		/// <summary>Scrolls one line up</summary>
		public void MoveLineUp()
		{
			TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
			if(textGenerator.characterCount == 0 || textGenerator.lineCount <= 1
				|| previousLineWidthStack.Count == 0)
			{
				return;
			}

			int previousLineLength = previousLineWidthStack.Pop();
			TextOffset -= previousLineLength;

			if(HasSelection)
			{
				SelectionRenderer.UpdateSelection(VisibleSelectionStartPosition, VisibleSelectionEndPosition);
			}
		}

		/// <summary>Keeps moving text up until given character position is in the first/top line</summary>
		/// <param name="targetCharPosition">The character position that should be in top line</param>
		public void MoveLineUpTill(int targetCharPosition)
		{
			TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
			if(textGenerator.characterCount == 0 || textGenerator.lineCount <= 1
				|| previousLineWidthStack.Count == 0)
			{
				return;
			}

			int firstLineStartCharIndex = TextOffset + GetLineStartCharIndex(textGenerator, 0);
			int firstLineEndCharIndex = TextOffset + GetLineEndCharIndex(textGenerator, 0);

			if(targetCharPosition > firstLineEndCharIndex)
			{
				return;
			}

			while(previousLineWidthStack.Count > 0)
			{
				if(targetCharPosition >= firstLineStartCharIndex && targetCharPosition <= firstLineEndCharIndex)
				{
					break;
				}

				int previousLineLength = previousLineWidthStack.Pop();
				firstLineEndCharIndex = firstLineStartCharIndex - 1;
				firstLineStartCharIndex -= previousLineLength;
				TextOffset -= previousLineLength;
			}

			if(HasSelection)
			{
				SelectionRenderer.UpdateSelection(VisibleSelectionStartPosition, VisibleSelectionEndPosition);
			}
		}

		/// <summary>Checks whether caret is out of bounds</summary>
		/// <returns>true when caret is out of bounds</returns>
		public bool CaretOutOfBounds()
		{
			if(InputField.LineType == LineType.SingleLine)
			{
				TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
				if(textGenerator.characterCount == 0 || textGenerator.lineCount != 1)
				{
					return false;
				}

				int firstLineStartCharIndex = TextOffset + GetLineStartCharIndex(textGenerator, 0);
				int endLineEndCharIndex = TextOffset + GetLineEndCharIndex(textGenerator, 0);

				return !(CaretPosition >= firstLineStartCharIndex && CaretPosition <= endLineEndCharIndex);
			}
			else
			{
				TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
				if(textGenerator.characterCount == 0 || textGenerator.lineCount <= 1)
				{
					return false;
				}

				int firstLineStartCharIndex = TextOffset + GetLineStartCharIndex(textGenerator, 0);
				int endLineEndCharIndex = TextOffset + GetLineEndCharIndex(textGenerator, textGenerator.lineCount - 1);

				return !(CaretPosition >= firstLineStartCharIndex && CaretPosition <= endLineEndCharIndex);
			}
		}
	}
}
