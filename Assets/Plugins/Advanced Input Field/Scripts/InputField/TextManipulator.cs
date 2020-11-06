//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Base class for text string manipulation</summary>
	public class TextManipulator
	{
		/// <summary>Special characters for emails. Doesn't include dot and @ on purpose! See usage for details.</summary>
		private const string EMAIL_SPECIAL_CHARACTERS = "!#$%&'*+-/=?^_`{|}~";

		/// <summary>The InputField</summary>
		public AdvancedInputField InputField { get; private set; }

		/// <summary>The TextNavigator/summary>
		public virtual TextNavigator TextNavigator { get; protected set; }

		/// <summary>The main renderer for text</summary>
		public Text TextRenderer { get; private set; }

		/// <summary>The renderer for processed text</summary>
		public Text ProcessedTextRenderer { get; private set; }

		/// <summary>The main text string</summary>
		public virtual string Text
		{
			get { return InputField.Text; }
			set
			{
				InputField.Text = value;
			}
		}

		/// <summary>The text in the clipboard</summary>
		public static string Clipboard
		{
			get
			{
				return GUIUtility.systemCopyBuffer;
			}
			set
			{
				GUIUtility.systemCopyBuffer = value;
			}
		}

		/// <summary>Indicates whether to currrently block text change events from being send to the native bindings</summary>
		public bool BlockNativeTextChange { get; set; }

		/// <summary>Initializes the class</summary>
		public virtual void Initialize(AdvancedInputField inputField, TextNavigator textNavigator, Text textRenderer, Text processedTextRenderer)
		{
			InputField = inputField;
			TextNavigator = textNavigator;
			TextRenderer = textRenderer;
			ProcessedTextRenderer = processedTextRenderer;
		}

		/// <summary>Begins the edit mode</summary>
		public void BeginEditMode()
		{
			TextNavigator.RefreshRenderedText();
			if(InputField.LiveProcessing)
			{
				TextRenderer.enabled = false;
				ProcessedTextRenderer.enabled = true;

				LiveProcessingFilter liveProcessingFilter = InputField.LiveProcessingFilter;
				string text = InputField.Text;
				int caretPosition = TextNavigator.CaretPosition;
				string processedText = liveProcessingFilter.ProcessText(text, caretPosition);
				if(processedText != null)
				{
					InputField.ProcessedText = processedText;

					int processedCaretPosition = liveProcessingFilter.DetermineProcessedCaret(text, caretPosition, processedText);
					TextNavigator.ProcessedCaretPosition = processedCaretPosition;
				}
			}
			else
			{
				TextRenderer.enabled = true;
				ProcessedTextRenderer.enabled = false;
			}
		}

		/// <summary>Ends the edit mode</summary>
		public void EndEditMode()
		{
			if(Text.Length == 0)
			{
				TextRenderer.enabled = false;
				ProcessedTextRenderer.text = InputField.PlaceHolderText;
				ProcessedTextRenderer.enabled = true;
				InputField.ProcessedText = InputField.PlaceHolderText;
			}
			else if(InputField.Filter != null)
			{
				string processedText = null;
				if(InputField.Filter.ProcessText(Text, out processedText))
				{
					TextRenderer.enabled = false;
					ProcessedTextRenderer.text = processedText;
					ProcessedTextRenderer.enabled = true;
				}
			}
		}

		/// <summary>Checks if character is valid</summary>
		/// <param name="c">The character to check</param>
		public bool IsValidChar(char c)
		{
			if((int)c == 127) //Delete key on mac
			{
				return false;
			}

			if(c == '\t' || c == '\n') // Accept newline and tab
			{
				return true;
			}

			return TextRenderer.font.HasCharacter(c);
		}

		/// <summary>Insert a string at caret position</summary>
		/// <param name="input">the string to insert</param>
		public void Insert(string input)
		{
			if(InputField.ReadOnly)
			{
				return;
			}

			if(TextNavigator.HasSelection)
			{
				DeleteSelection();
			}

			int length = input.Length;
			for(int i = 0; i < length; i++)
			{
				TryInsertChar(input[i]);
			}
		}

		/// <summary>Tries to insert a character</summary>
		/// <param name="c">The character to insert</param>
		public void TryInsertChar(char c)
		{
			if(!IsValidChar(c))
			{
				return;
			}

			string currentText = Text;
			char result = Validate(currentText, TextNavigator.CaretPosition, c);
			if(result != 0)
			{
				Insert(result);
			}
		}

		/// <summary>Validates the text</summary>
		/// <param name="text">The text to check</param>
		/// <param name="pos">The current char position</param>
		/// <param name="c">The next character</param>
		protected char Validate(string text, int pos, char ch)
		{
			CharacterValidation characterValidation = InputField.CharacterValidation;
			int caretPosition = TextNavigator.CaretPosition;
			int selectionStartPosition = -1;
			if(TextNavigator.HasSelection)
			{
				selectionStartPosition = TextNavigator.SelectionStartPosition;
			}

			// Validation is disabled
			if(characterValidation == CharacterValidation.None)
			{
				return ch;
			}

			if(characterValidation == CharacterValidation.Integer || characterValidation == CharacterValidation.Decimal)
			{
				// Integer and decimal
				bool cursorBeforeDash = (pos == 0 && text.Length > 0 && text[0] == '-');
				bool dashInSelection = text.Length > 0 && text[0] == '-' && ((caretPosition == 0 && selectionStartPosition > 0) || (selectionStartPosition == 0 && caretPosition > 0));
				bool selectionAtStart = caretPosition == 0 || selectionStartPosition == 0;
				if(!cursorBeforeDash || dashInSelection)
				{
					if(ch >= '0' && ch <= '9') return ch;
					if(ch == '-' && (pos == 0 || selectionAtStart)) return ch;
					if(ch == '.' && characterValidation == CharacterValidation.Decimal && !text.Contains(".")) return ch;
					if(ch == ',' && characterValidation == CharacterValidation.Decimal && !text.Contains(".")) return '.';
				}
			}
			else if(characterValidation == CharacterValidation.Alphanumeric)
			{
				// All alphanumeric characters
				if(ch >= 'A' && ch <= 'Z') return ch;
				if(ch >= 'a' && ch <= 'z') return ch;
				if(ch >= '0' && ch <= '9') return ch;
			}
			else if(characterValidation == CharacterValidation.Name)
			{
				// FIXME: some actions still lead to invalid input:
				//        - Hitting delete in front of an uppercase letter
				//        - Selecting an uppercase letter and deleting it
				//        - Typing some text, hitting Home and typing more text (we then have an uppercase letter in the middle of a word)
				//        - Typing some text, hitting Home and typing a space (we then have a leading space)
				//        - Erasing a space between two words (we then have an uppercase letter in the middle of a word)
				//        - We accept a trailing space
				//        - We accept the insertion of a space between two lowercase letters.
				//        - Typing text in front of an existing uppercase letter
				//        - ... and certainly more
				//
				// The rule we try to implement are too complex for this kind of verification.

				if(char.IsLetter(ch))
				{
					// Character following a space should be in uppercase.
					if(char.IsLower(ch) && ((pos == 0) || (text[pos - 1] == ' ')))
					{
						return char.ToUpper(ch);
					}

					// Character not following a space or an apostrophe should be in lowercase.
					if(char.IsUpper(ch) && (pos > 0) && (text[pos - 1] != ' ') && (text[pos - 1] != '\''))
					{
						return char.ToLower(ch);
					}

					return ch;
				}

				if(ch == '\'')
				{
					// Don't allow more than one apostrophe
					if(!text.Contains("'"))
					{
						// Don't allow consecutive spaces and apostrophes.
						if(!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
							  ((pos < text.Length) && ((text[pos] == ' ') || (text[pos] == '\'')))))
						{
							return ch;
						}
					}
				}

				if(ch == ' ')
				{
					// Don't allow consecutive spaces and apostrophes.
					if(!(((pos > 0) && ((text[pos - 1] == ' ') || (text[pos - 1] == '\''))) ||
						  ((pos < text.Length) && ((text[pos] == ' ') || (text[pos] == '\'')))))
					{
						return ch;
					}
				}
			}
			else if(characterValidation == CharacterValidation.EmailAddress)
			{
				// From StackOverflow about allowed characters in email addresses:
				// Uppercase and lowercase English letters (a-z, A-Z)
				// Digits 0 to 9
				// Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
				// Character . (dot, period, full stop) provided that it is not the first or last character,
				// and provided also that it does not appear two or more times consecutively.

				if(ch >= 'A' && ch <= 'Z') return ch;
				if(ch >= 'a' && ch <= 'z') return ch;
				if(ch >= '0' && ch <= '9') return ch;
				if(ch == '@' && text.IndexOf('@') == -1) return ch;
				if(EMAIL_SPECIAL_CHARACTERS.IndexOf(ch) != -1) return ch;
				if(ch == '.')
				{
					char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
					char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';
					if(lastChar != '.' && nextChar != '.')
					{
						return ch;
					}
				}
			}
			return (char)0;
		}

		/// <summary>Insert a character at caret position</summary>
		/// <param name="c">The character to insert</param>
		public virtual void Insert(char c)
		{
			if(InputField.ReadOnly)
			{
				return;
			}

			if(TextNavigator.HasSelection)
			{
				DeleteSelection();
			}
			else if(InputField.CharacterLimit > 0 && Text.Length >= InputField.CharacterLimit)
			{
				return;
			}

			Text = Text.Insert(TextNavigator.CaretPosition, c.ToString());

			TextNavigator.MoveCaret(1);
		}

		/// <summary>Deletes previous character</summary>
		public void DeletePreviousChar()
		{
			if(InputField.ReadOnly)
			{
				return;
			}

			int originalLineCount = TextNavigator.LineCount;

			if(TextNavigator.HasSelection)
			{
				TextNavigator.MoveLineUpTill(TextNavigator.SelectionStartPosition);
				DeleteSelection();
			}
			else if(TextNavigator.CaretPosition > 0)
			{
				TextNavigator.MoveCaret(-1);
				Text = Text.Remove(TextNavigator.CaretPosition, 1);
			}

			LineCheck(originalLineCount);
		}

		/// <summary>Deletes next character</summary>
		public void DeleteNextChar()
		{
			if(InputField.ReadOnly)
			{
				return;
			}

			int originalLineCount = TextNavigator.LineCount;

			if(TextNavigator.HasSelection)
			{
				TextNavigator.MoveLineUpTill(TextNavigator.SelectionStartPosition);
				DeleteSelection();
			}
			else
			{
				if(TextNavigator.CaretPosition < Text.Length)
				{
					Text = Text.Remove(TextNavigator.CaretPosition, 1);
				}
			}

			LineCheck(originalLineCount);
		}

		/// <summary>line check for text scroll</summary>
		/// <param name="originalLineCount">The line count before the text is refreshed</param>
		private void LineCheck(int originalLineCount)
		{
			if(TextNavigator.LineCount < originalLineCount)
			{
				int lineDifference = originalLineCount - TextNavigator.LineCount;
				for(int i = 0; i < lineDifference; i++)
				{
					TextNavigator.MoveLineUp();
				}
			}
		}

		/// <summary>Deletes current text selection</summary>
		public void DeleteSelection()
		{
			if(InputField.ReadOnly)
			{
				return;
			}

			string text = Text.Remove(TextNavigator.SelectionStartPosition, TextNavigator.SelectionEndPosition - TextNavigator.SelectionStartPosition);

			TextNavigator.CaretToSelectionStart();
			TextNavigator.CancelSelection();
			Text = text;
		}

		/// <summary>Copies current text selection</summary>
		public virtual void Copy()
		{
			if(!InputField.IsPasswordField)
			{
				Clipboard = TextNavigator.SelectedText;
			}
			else
			{
				Clipboard = string.Empty;
			}
		}

		/// <summary>Pastes clipboard text</summary>
		public virtual void Paste()
		{
			if(InputField.ReadOnly)
			{
				return;
			}

			string input = Clipboard;
			string processedInput = string.Empty;

			int length = input.Length;
			for(int i = 0; i < length; i++)
			{
				char c = input[i];

				if(c >= ' ' || c == '\t' || c == '\r' || c == 10 || c == '\n')
				{
					processedInput += c;
				}
			}

			if(!string.IsNullOrEmpty(processedInput))
			{
				Insert(processedInput);
			}
		}

		/// <summary>Cuts current text selection</summary>
		public virtual void Cut()
		{
			if(!InputField.IsPasswordField)
			{
				Clipboard = TextNavigator.SelectedText;
			}
			else
			{
				Clipboard = string.Empty;
			}

			if(InputField.ReadOnly)
			{
				return;
			}

			if(TextNavigator.HasSelection)
			{
				int originalLineCount = TextNavigator.LineCount;

				TextNavigator.MoveLineUpTill(TextNavigator.SelectionStartPosition);
				DeleteSelection();

				LineCheck(originalLineCount);
			}
		}
	}
}
