//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Subclass of TextInputHandler for mobile platforms</summary>
	public class MobileTextInputHandler: TextInputHandler
	{
		/// <summary>The TouchScreenKeyboard</summary>
		private TouchScreenKeyboard keyboard;

		/// <summary>The MobileTextNavigator</summary>
		private MobileTextNavigator mobileTextNavigator;

		/// <summary>The MobileTextManipulator</summary>
		private MobileTextManipulator mobileTextManipulator;

		/// <summary>The ActionBar</summary>
		protected ActionBar actionBar;

		/// <summary>The caret position when calling the keyboard</summary>
		private int startCaretPosition;

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

		public override TextNavigator TextNavigator
		{
			get { return mobileTextNavigator; }
			protected set { mobileTextNavigator = (MobileTextNavigator)value; }
		}

		public override TextManipulator TextManipulator
		{
			get { return mobileTextManipulator; }
			protected set { mobileTextManipulator = (MobileTextManipulator)value; }
		}

		public MobileTextInputHandler()
		{
		}

		public override void Initialize(AdvancedInputField inputField, TextNavigator textNavigator, TextManipulator textManipulator)
		{
			base.Initialize(inputField, textNavigator, textManipulator);

			mobileTextNavigator.actionBar = actionBar;
		}

		/// <summary>Initializes the ActionBar</summary>
		/// <param name="textRenderer">The text renderer to attach the ActionBar to</param>
		public void InitActionBar(AdvancedInputField inputField, Text textRenderer)
		{
			actionBar = inputField.transform.root.GetComponentInChildren<ActionBar>(true);
			if(actionBar == null)
			{
				actionBar = GameObject.Instantiate(Resources.Load<ActionBar>("ActionBar"));
			}

			actionBar.transform.SetParent(textRenderer.transform);
			actionBar.transform.localScale = Vector3.one;
			actionBar.transform.localPosition = Vector3.zero;

			actionBar.Initialize(inputField, this);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			if(actionBar != null)
			{
				GameObject.Destroy(actionBar.gameObject);
				actionBar = null;
			}
		}

		public override void OnCanvasScaleChanged(float canvasScaleFactor)
		{
			base.OnCanvasScaleChanged(canvasScaleFactor);

			if(actionBar != null)
			{
				actionBar.UpdateSize(canvasScaleFactor);
				actionBar.UpdateButtons();
			}
		}

		public override void Process()
		{
			base.Process();

			if(InputField.ReadOnly)
			{
				return;
			}

			if(!Keyboard.Active)
			{
				startCaretPosition = TextNavigator.CaretPosition; //Workaround for setting correct caret position when starting to show keyboard

				string text = InputField.Text;
				KeyboardType mobileKeyboardType = InputField.KeyboardType;
				CharacterValidation characterValidation = InputField.CharacterValidation;
				LineType lineType = InputField.LineType;
				bool autocorrection = InputField.AutoCorrection;
				bool secure = InputField.Secure;
				int characterLimit = InputField.CharacterLimit;
				NativeTouchScreenKeyboard.ShowKeyboard(text, mobileKeyboardType, characterValidation, lineType, autocorrection, secure, characterLimit);
			}

			TouchScreenKeyboard.Event keyboardEvent = null;
			while(Keyboard.PopEvent(out keyboardEvent))
			{
				switch(keyboardEvent.Type)
				{
					case TouchScreenKeyboard.EventType.SHOW: ProcessShow(keyboardEvent); break;
					case TouchScreenKeyboard.EventType.HIDE: ProcessHide(keyboardEvent); break;
					case TouchScreenKeyboard.EventType.TEXT_CHANGE: ProcessTextChange(keyboardEvent); break;
					case TouchScreenKeyboard.EventType.SELECTION_CHANGE: ProcessSelectionChange(keyboardEvent); break;
					case TouchScreenKeyboard.EventType.DONE: ProcessDone(keyboardEvent); break;
					case TouchScreenKeyboard.EventType.NEXT: ProcessDone(keyboardEvent); break;
					case TouchScreenKeyboard.EventType.CANCEL: ProcessCancel(keyboardEvent); break;
				}
			}
		}

		public override void BeginEditMode()
		{
			base.BeginEditMode();

			Keyboard.Active = false; //Flag keyboard as inactive, so this inputfield will load it's settings

			if(actionBar != null)
			{
				if(Canvas != null)
				{
					actionBar.UpdateSize(Canvas.scaleFactor);
					actionBar.UpdateButtons();
				}
			}
		}

		/// <summary>Processes keyboard show event</summary>
		private void ProcessShow(TouchScreenKeyboard.Event keyboardEvent)
		{
			TextNavigator.CaretPosition = startCaretPosition; //Workaround for setting correct caret position when starting to show keyboard
		}

		/// <summary>Processes keyboard hide event</summary>
		private void ProcessHide(TouchScreenKeyboard.Event keyboardEvent)
		{
		}

		/// <summary>Processes keyboard text change event</summary>
		private void ProcessTextChange(TouchScreenKeyboard.Event keyboardEvent)
		{
			int textLengthBefore = InputField.Text.Length;

			TextManipulator.BlockNativeTextChange = true;
			TextNavigator.BlockNativeSelectionChange = true;
			InputField.Text = keyboardEvent.Value;
			TextManipulator.BlockNativeTextChange = false;
			TextNavigator.BlockNativeSelectionChange = false;

			int textLengthAfter = InputField.Text.Length;
			if(textLengthBefore != textLengthAfter)
			{
				if(!mobileTextNavigator.HasInsertedCharAfterClick)
				{
					mobileTextNavigator.HasInsertedCharAfterClick = true;

					if(InputField.ActionBarEnabled)
					{
						mobileTextNavigator.actionBar.Hide();
					}

					if(InputField.SelectionCursorsEnabled)
					{
						mobileTextNavigator.HideCurrentMobileCursor();
					}
				}
			}
		}

		/// <summary>Processes keyboard caret position change event</summary>
		private void ProcessSelectionChange(TouchScreenKeyboard.Event keyboardEvent)
		{
			string[] values = keyboardEvent.Value.Split(',');
			int selectionStartPosition = int.Parse(values[0].Trim());
			int selectionEndPosition = int.Parse(values[1].Trim());

			TextManipulator.BlockNativeTextChange = true;
			TextNavigator.BlockNativeSelectionChange = true;
			mobileTextNavigator.UpdateSelection(selectionStartPosition, selectionEndPosition);
			TextManipulator.BlockNativeTextChange = false;
			TextNavigator.BlockNativeSelectionChange = false;
		}

		/// <summary>Processes keyboard done event</summary>
		private void ProcessDone(TouchScreenKeyboard.Event keyboardEvent)
		{
			Keyboard.ClearEventQueue(); //Should be last event to process, so clear queue

			if(InputField.NextInputField != null)
			{
				Keyboard.Active = false; //Flag keyboard as inactive, so next inputfield will load it's settings
				InputField.Deselect(EndEditReason.KEYBOARD_NEXT);
				InputField.NextInputField.ManualSelect();
			}
			else
			{
				Keyboard.HideKeyboard();
				InputField.Deselect(EndEditReason.KEYBOARD_DONE);
			}
		}

		/// <summary>Processes keyboard cancel event</summary>
		private void ProcessCancel(TouchScreenKeyboard.Event keyboardEvent)
		{
			Keyboard.ClearEventQueue(); //Should be last event to process, so clear queue

			Keyboard.HideKeyboard();
			InputField.Deselect(EndEditReason.KEYBOARD_CANCEL);
		}

		public override void OnSelect()
		{
			base.OnSelect();

			if(InputField.ReadOnly)
			{
				return;
			}

			if(!Keyboard.Active)
			{
				string text = InputField.Text;
				KeyboardType mobileKeyboardType = InputField.KeyboardType;
				CharacterValidation characterValidation = InputField.CharacterValidation;
				LineType lineType = InputField.LineType;
				bool autocorrection = InputField.AutoCorrection;
				bool secure = InputField.Secure;
				int characterLimit = InputField.CharacterLimit;
				NativeTouchScreenKeyboard.ShowKeyboard(text, mobileKeyboardType, characterValidation, lineType, autocorrection, secure, characterLimit);
			}

			Keyboard.SetSelection(TextNavigator.CaretPosition, TextNavigator.CaretPosition);
		}

		public override void OnHold(Vector2 position)
		{
			base.OnHold(position);
			TextNavigator.SelectCurrentWord();
		}

		public override void OnSingleTap(Vector2 position)
		{
			base.OnSingleTap(position);

			if(InputField.ActionBarEnabled)
			{
				mobileTextNavigator.HasInsertedCharAfterClick = false;
			}
		}

		/// <summary>Event callback when cut button has been clicked</summary>
		public void OnCut()
		{
			TextManipulator.Cut();
		}

		/// <summary>Event callback when copy button has been clicked</summary>
		public void OnCopy()
		{
			TextManipulator.Copy();
		}

		/// <summary>Event callback when paste button has been clicked</summary>
		public void OnPaste()
		{
			TextManipulator.Paste();
		}

		/// <summary>Event callback when select all button has been clicked</summary>
		public void OnSelectAll()
		{
			TextNavigator.SelectAll();
		}

		public override void CancelInput()
		{
			base.CancelInput();

			if(Keyboard != null)
			{
				GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
				if(currentSelectedGameObject != null && InputField.NextInputField != null
					&& currentSelectedGameObject == InputField.NextInputField.gameObject && !InputField.NextInputField.ReadOnly)
				{
					Keyboard.BlockKeyboardCancel();
					return; //Don't hide keyboard, next inputfield is selected
				}

				Keyboard.HideKeyboard();
				Keyboard.ClearEventQueue(); //Should be last event to process, so clear queue
			}
		}
	}
}
#endif