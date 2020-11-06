//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	/// <summary>The TextNavigator for mobile platforms</summary>
	public class MobileTextNavigator: TextNavigator
	{
		/// <summary>The thumb size multiplier used for selection cursors size calculations</summary>
		private const float THUMB_SIZE_SCALE = 0.55f;

		/// <summary>The TouchScreenKeyboard</summary>
		private TouchScreenKeyboard keyboard;

		/// <summary>The Transform of the selection cursors</summary>
		private Transform selectionCursorsTranform;

		/// <summary>The start cursor renderer</summary>
		private Image startCursor;

		/// <summary>The end cursor renderer</summary>
		private Image endCursor;

		/// <summary>The current cursor renderer</summary>
		private Image currentCursor;

		/// <summary>The size of the cursors</summary>
		private float cursorSize;

		/// <summary>Indicates whether at least one character has been inserted (or deleted) afer last click</summary>
		public bool hasInsertedCharAfterClick;

		/// <summary>Indicates whether at least one character has been inserted (or deleted) afer last click</summary>
		public bool HasInsertedCharAfterClick
		{
			get { return hasInsertedCharAfterClick; }
			set
			{
				hasInsertedCharAfterClick = value;
				currentCursor.enabled = !hasInsertedCharAfterClick;
			}
		}

		/// <summary>The TouchScreenKeyboard</summary>
		private TouchScreenKeyboard Keyboard
		{
			get
			{
				if(keyboard == null)
				{
					keyboard = NativeTouchScreenKeyboard.Keyboard;
				}

				return keyboard;
			}
		}

		public override int CaretPosition
		{
			get { return base.CaretPosition; }
			set
			{
				base.CaretPosition = value;

				if(Keyboard.Active && !InputField.ReadOnly && !HasSelection && !BlockNativeSelectionChange)
				{
					Keyboard.SetSelection(caretPosition, caretPosition);
				}

				if(Keyboard.Active && !InputField.ReadOnly && !BlockNativeSelectionChange)
				{
					UpdateNativeSelection();
				}
			}
		}

		public override int ProcessedCaretPosition
		{
			get { return base.ProcessedCaretPosition; }
			set
			{
				base.ProcessedCaretPosition = value;

				if(Keyboard.Active && !InputField.ReadOnly && !HasSelection && !BlockNativeSelectionChange)
				{
					Keyboard.SetSelection(caretPosition, caretPosition);
				}

				if(Keyboard.Active && !InputField.ReadOnly && !BlockNativeSelectionChange)
				{
					UpdateNativeSelection();
				}
			}
		}

		public override int SelectionStartPosition
		{
			get { return base.SelectionStartPosition; }
			protected set
			{
				base.SelectionStartPosition = value;

				if(InputField.SelectionCursorsEnabled)
				{
					if(Canvas != null)
					{
						UpdateSelectionCursors(Canvas.scaleFactor);
					}
				}

				if(InputField.ActionBarEnabled)
				{
					UpdateSelectionCursorsActionBar();
				}

				if(Keyboard.Active && !InputField.ReadOnly && !BlockNativeSelectionChange)
				{
					UpdateNativeSelection();
				}
			}
		}

		public override int ProcessedSelectionStartPosition
		{
			get { return base.ProcessedSelectionStartPosition; }
			protected set
			{
				base.ProcessedSelectionStartPosition = value;

				if(InputField.SelectionCursorsEnabled)
				{
					if(Canvas != null)
					{
						UpdateSelectionCursors(Canvas.scaleFactor);
					}
				}

				if(InputField.ActionBarEnabled)
				{
					UpdateSelectionCursorsActionBar();
				}

				if(Keyboard.Active && !InputField.ReadOnly && !BlockNativeSelectionChange)
				{
					UpdateNativeSelection();
				}
			}
		}

		public override int SelectionEndPosition
		{
			get { return base.SelectionEndPosition; }
			protected set
			{
				base.SelectionEndPosition = value;

				if(InputField.SelectionCursorsEnabled)
				{
					if(Canvas != null)
					{
						UpdateSelectionCursors(Canvas.scaleFactor);
					}
				}

				if(InputField.ActionBarEnabled)
				{
					UpdateSelectionCursorsActionBar();
				}

				if(Keyboard.Active && !InputField.ReadOnly && !BlockNativeSelectionChange)
				{
					UpdateNativeSelection();
				}
			}
		}

		public override int ProcessedSelectionEndPosition
		{
			get { return base.ProcessedSelectionEndPosition; }
			protected set
			{
				base.ProcessedSelectionEndPosition = value;

				if(InputField.SelectionCursorsEnabled)
				{
					if(Canvas != null)
					{
						UpdateSelectionCursors(Canvas.scaleFactor);
					}
				}

				if(InputField.ActionBarEnabled)
				{
					UpdateSelectionCursorsActionBar();
				}

				if(Keyboard.Active && !InputField.ReadOnly && !BlockNativeSelectionChange)
				{
					UpdateNativeSelection();
				}
			}
		}

		/// <summary>The ActionBar</summary>
		public ActionBar actionBar { get; set; }

		public override void Initialize(AdvancedInputField inputField, Image caretRenderer, TextSelectionRenderer selectionRenderer)
		{
			base.Initialize(inputField, caretRenderer, selectionRenderer);

			selectionCursorsTranform = GameObject.Instantiate(Resources.Load<Transform>("SelectionCursors"));
			selectionCursorsTranform.SetParent(TextRenderer.transform);
			selectionCursorsTranform.localScale = Vector3.one;
			selectionCursorsTranform.localPosition = Vector3.zero;

			startCursor = selectionCursorsTranform.Find("StartCursor").GetComponent<Image>();
			endCursor = selectionCursorsTranform.Find("EndCursor").GetComponent<Image>();
			currentCursor = selectionCursorsTranform.Find("CurrentCursor").GetComponent<Image>();

			if(Canvas != null)
			{
				UpdateCursorSize(Canvas.scaleFactor);
			}

			startCursor.enabled = false;
			endCursor.enabled = false;
			currentCursor.enabled = false;

			startCursor.GetComponent<PointerListener>().RegisterCallbacks(OnStartCursorDown, OnStartCursorUp);
			endCursor.GetComponent<PointerListener>().RegisterCallbacks(OnEndCursorDown, OnEndCursorUp);
			currentCursor.GetComponent<PointerListener>().RegisterCallbacks(OnCurrentCursorDown, OnCurrentCursorUp);
		}

		public override void OnCanvasScaleChanged(float canvasScaleFactor)
		{
			base.OnCanvasScaleChanged(canvasScaleFactor);

			UpdateCursorSize(canvasScaleFactor);
		}

		private void UpdateCursorSize(float canvasScaleFactor)
		{
			int thumbSize = Util.DetermineThumbSize();
			if(thumbSize <= 0)
			{
				Debug.LogWarning("Unknown DPI");
				if(InputField.TextRenderer.resizeTextForBestFit)
				{
					cursorSize = InputField.TextRenderer.cachedTextGenerator.fontSizeUsedForBestFit / canvasScaleFactor;
				}
				else
				{
					cursorSize = InputField.TextRenderer.fontSize / canvasScaleFactor;
				}
			}
			else
			{
				cursorSize = (thumbSize * THUMB_SIZE_SCALE) / canvasScaleFactor;
			}

			cursorSize *= InputField.SelectionCursorsScale;

			startCursor.rectTransform.sizeDelta = new Vector2(cursorSize, cursorSize);
			endCursor.rectTransform.sizeDelta = new Vector2(cursorSize, cursorSize);
			currentCursor.rectTransform.sizeDelta = new Vector2(cursorSize, cursorSize);
		}

		public override void BeginEditMode()
		{
			base.BeginEditMode();

			if(Canvas != null)
			{
				UpdateCursorSize(Canvas.scaleFactor);
			}
		}

		public override void EndEditMode()
		{
			EditMode = false;
			caretBlinkTime = InputField.CaretBlinkRate;
			CaretRenderer.enabled = false;
			UpdateSelection(0, 0);

			TextOffset = 0;

			startCursor.enabled = false;
			endCursor.enabled = false;
			currentCursor.enabled = false;

			if(actionBar != null)
			{
				actionBar.Hide();
			}
		}

		public void HideCurrentMobileCursor()
		{
			currentCursor.enabled = false;
		}

		private void UpdateNativeSelection()
		{
			if(HasSelection)
			{
				Keyboard.SetSelection(selectionStartPosition, selectionEndPosition);
			}
			else
			{
				Keyboard.SetSelection(caretPosition, caretPosition);
			}
		}

		/// <summary>Event callback when start cursor is pressed</summary>
		private void OnStartCursorDown()
		{
			InputField.CurrentDragMode = DragMode.FROM_SELECTION_END; //We're going to move the start cursor, so start from end cursor
			InputField.DragOffset = new Vector2(startCursor.rectTransform.rect.width * 0.5f, startCursor.rectTransform.rect.height);
		}

		/// <summary>Event callback when start cursor is released</summary>
		private void OnStartCursorUp()
		{
		}

		/// <summary>Event callback when end cursor is pressed</summary>
		private void OnEndCursorDown()
		{
			InputField.CurrentDragMode = DragMode.FROM_SELECTION_START; //We're going to move the end cursor, so start from start cursor
			InputField.DragOffset = new Vector2(-startCursor.rectTransform.rect.width * 0.5f, startCursor.rectTransform.rect.height);
		}

		/// <summary>Event callback when end cursor is released</summary>
		private void OnEndCursorUp()
		{
		}

		/// <summary>Event callback when current cursor is pressed</summary>
		private void OnCurrentCursorDown()
		{
			if(!InputField.ActionBarEnabled)
			{
				return;
			}

			if(actionBar.Visible)
			{
				actionBar.Hide();
			}
			else
			{
				bool paste = !InputField.ReadOnly && InputField.ActionBarPaste;
				bool selectAll = InputField.ActionBarSelectAll;
				actionBar.Show(false, false, paste, selectAll);
			}
		}

		/// <summary>Event callback when current cursor is released</summary>
		private void OnCurrentCursorUp()
		{
		}

		/// <summary>Updates the selection cursors</summary>
		private void UpdateSelectionCursors(float canvasScaleFactor)
		{
			if(SelectionEndPosition > SelectionStartPosition)
			{
				TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
				int lineHeight = textGenerator.lines[0].height;

				int startIndex = 0;
				int endIndex = 0;

				if(VisibleSelectionStartPosition >= 0)
				{
					startIndex = Mathf.Clamp(VisibleSelectionStartPosition, 0, textGenerator.characterCount - 1);
					Vector2 startCursorPosition = textGenerator.characters[startIndex].cursorPos;
					startCursorPosition.y -= lineHeight;
					startCursor.rectTransform.anchoredPosition = startCursorPosition / canvasScaleFactor;
					startCursor.enabled = true;
				}
				else
				{
					startCursor.enabled = false;
				}

				if(VisibleSelectionEndPosition >= 0)
				{
					endIndex = Mathf.Clamp(VisibleSelectionEndPosition, 0, textGenerator.characterCount - 1);
					Vector2 endCursorPosition = textGenerator.characters[endIndex].cursorPos;
					endCursorPosition.y -= lineHeight;
					endCursor.rectTransform.anchoredPosition = endCursorPosition / canvasScaleFactor;
					endCursor.enabled = true;
				}
			}
			else
			{
				startCursor.enabled = false;
				endCursor.enabled = false;
			}
		}

		private void UpdateSelectionCursorsActionBar()
		{
			if(SelectionEndPosition > SelectionStartPosition)
			{
				TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
				int startIndex = 0;

				if(VisibleSelectionStartPosition >= 0)
				{
					startIndex = Mathf.Clamp(VisibleSelectionStartPosition, 0, textGenerator.characterCount - 1);
				}

				actionBar.transform.SetParent(currentCursor.transform.parent);
				actionBar.transform.localScale = Vector3.one;
				bool cut = !InputField.Secure && !InputField.ReadOnly && InputField.ActionBarCut;
				bool copy = !InputField.Secure && InputField.ActionBarCopy;
				bool paste = !InputField.ReadOnly && InputField.ActionBarPaste;
				bool selectAll = InputField.ActionBarSelectAll;
				actionBar.Show(cut, copy, paste, selectAll);
				Vector2 actionBarPosition = textGenerator.characters[startIndex].cursorPos;
				actionBar.UpdatePosition(actionBarPosition);
			}
			else
			{
				actionBar.Hide();
			}
		}

		/// <summary>Updates the current cursor</summary>
		public void UpdateCurrentCursor(float canvasScaleFactor)
		{
			TextGenerator textGenerator = TextRenderer.cachedTextGenerator;
			if(textGenerator.lineCount > 0)
			{
				int lineHeight = textGenerator.lines[0].height;

				int index = Mathf.Clamp(VisibleCaretPosition, 0, textGenerator.characterCount - 1);
				Vector2 currentCursorPosition = textGenerator.characters[index].cursorPos;
				currentCursorPosition.y -= lineHeight;
				currentCursor.rectTransform.anchoredPosition = currentCursorPosition / canvasScaleFactor;
				if(!HasInsertedCharAfterClick)
				{
					currentCursor.enabled = true;
				}

				if(InputField.ActionBarEnabled)
				{
					actionBar.transform.SetParent(currentCursor.transform.parent);
					actionBar.transform.localScale = Vector3.one;
					Vector2 actionBarPosition = textGenerator.characters[index].cursorPos;
					actionBar.UpdatePosition(actionBarPosition);
				}
			}
			else
			{
				currentCursor.enabled = false;
			}
		}

		public override void UpdateCaret()
		{
			base.UpdateCaret();

			if(EditMode && (InputField.ActionBarEnabled || InputField.SelectionCursorsEnabled))
			{
				if(Canvas != null)
				{
					UpdateCurrentCursor(Canvas.scaleFactor);
				}
			}
		}

		public override void UpdateSelected()
		{
			base.UpdateSelected();

			if(EditMode && (InputField.ActionBarEnabled || InputField.SelectionCursorsEnabled))
			{
				currentCursor.enabled = !HasSelection && !HasInsertedCharAfterClick;
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			if(selectionCursorsTranform != null)
			{
				GameObject.Destroy(selectionCursorsTranform.gameObject);
				selectionCursorsTranform = null;
			}
		}

		/// <summary>Updates the selection without updating selection in native text editor</summary>
		/// <param name="position">The new caret position</param>
		public void UpdateSelection(int startPosition, int endPosition)
		{
			if(startPosition + 1 <= selectionStartPosition)
			{
				base.SelectionStartPosition = startPosition;
				base.SelectionEndPosition = endPosition;
				base.CaretPosition = startPosition;
			}
			else
			{
				base.SelectionStartPosition = startPosition;
				base.SelectionEndPosition = endPosition;
				base.CaretPosition = endPosition;
			}

			if(InputField.SelectionCursorsEnabled)
			{
				if(Canvas != null)
				{
					UpdateSelectionCursors(Canvas.scaleFactor);
				}
			}

			if(InputField.ActionBarEnabled)
			{
				UpdateSelectionCursorsActionBar();
			}
		}
	}
}
#endif