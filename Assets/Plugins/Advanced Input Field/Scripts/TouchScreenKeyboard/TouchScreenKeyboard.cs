//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>The delegate for Keyboard Height Changed event</summary>
	public delegate void OnKeyboardHeightChangedHandler(int keyboardHeight);

	/// <summary>Base class that acts as a bridge for the Native Keyboard for a specific platform</summary>
	public abstract class TouchScreenKeyboard: MonoBehaviour
	{
		/// <summary>Event type of the keyboard callbacks</summary>
		public enum EventType
		{
			SHOW,
			HIDE,
			TEXT_CHANGE,
			SELECTION_CHANGE,
			DONE,
			NEXT,
			CANCEL,
			DELETE
		}

		/// <summary>Event for keyboard callbacks</summary>
		public class Event
		{
			/// <summary>Event type</summary>
			public EventType Type { get; private set; }

			/// <summary>The value</summary>
			public string Value { get; private set; }

			public Event(EventType type, string value = null)
			{
				Type = type;
				Value = value;
			}
		}

		/// <summary>Queue with Keyboard events</summary>
		protected Queue<Event> eventQueue;

		/// <summary>The name of the GameObject used for callbacks</summary>
		protected string gameObjectName;

		/// <summary>The event for Keyboard Height Changed</summary>
		private event OnKeyboardHeightChangedHandler onKeyboardHeightChanged;

		/// <summary>Indicates whether the keyboard is being used</summary>
		public bool Active { get; set; }

		/// <summary>Initializes this class</summary>
		/// <param name="gameObjectName">The name of the GameObject to use for callbacks</param>
		public void Init(string gameObjectName)
		{
			this.gameObjectName = gameObjectName;
			eventQueue = new Queue<Event>(30);
			Setup();
		}

		/// <summary>Gets and removes next keyboard event</summary>
		/// <param name="keyboardEvent">The output keyboard event</param>
		public bool PopEvent(out Event keyboardEvent)
		{
			if(eventQueue.Count == 0)
			{
				keyboardEvent = null;
				return false;
			}

			keyboardEvent = eventQueue.Dequeue();
			return true;
		}

		/// <summary>Clears the keyboard event queue</summary>
		public void ClearEventQueue()
		{
			eventQueue.Clear();
		}

		/// <summary>Adds a KeyboardHeightChangeListener</summary>
		/// <param name="listener">The KeyboardHeightChangeListener to add</param>
		public void AddKeyboardHeightChangedListener(OnKeyboardHeightChangedHandler listener)
		{
			onKeyboardHeightChanged += listener;
		}

		/// <summary>Removes a KeyboardHeightChangeListener</summary>
		/// <param name="listener">The KeyboardHeightChangeListener to remove</param>
		public void RemoveKeyboardHeightChangedListener(OnKeyboardHeightChangedHandler listener)
		{
			onKeyboardHeightChanged -= listener;
		}

		/// <summary>Setups the bridge to the Native TouchScreenKeyboard</summary>
		protected virtual void Setup() { }

		/// <summary>Shows the TouchScreenKeyboard for current platform</summary>
		/// <param name="text">The current text of the InputField</param>
		/// <param param name="keyboardType">The keyboard type to use</param>
		/// <param param name="characterValidation">The character validation to use</param>
		/// <param name="lineType">The lineType to use</param>
		/// <param name="autocorrection">Indicates whether autocorrection is enabled</param>
		/// <param name="characterLimit">The character limit for the text</param>
		/// <param name="secure">Indicates whether input should be secure</param>
		public virtual void ShowKeyboard(string text, KeyboardType keyboardType, CharacterValidation characterValidation, LineType lineType, bool autocorrection, bool secure, int characterLimit) { }

		/// <summary>Hides the TouchScreenKeyboard for current platform</summary>
		public virtual void HideKeyboard() { }

		/// <summary>Changes the text natively used by the keyboard</summary>
		/// <param name="ignoreSelectionChange">Indicates whether to ignore next selection change</param>
		public virtual void ChangeKeyboardText(string text, bool ignoreSelectionChange) { }

		/// <summary>Sets the selection for the native keyboard</summary>
		/// <param name="selectionStartPosition">The start position of the selection</param>
		/// <param name="selectionEndPosition">The end position of the selection</param>
		public virtual void SetSelection(int selectionStartPosition, int selectionEndPosition) { }

		/// <summary>Blocks next keyboard cancel event (Android only)</summary>
		public virtual void BlockKeyboardCancel() { }

		/// <summary>Event callback when the keyboard gets shown</summary>
		public void OnKeyboardShow()
		{
			eventQueue.Enqueue(new Event(EventType.SHOW, null));
			Active = true;
		}

		/// <summary>Event callback when the keyboard gets hidden</summary>
		public void OnKeyboardHide()
		{
			eventQueue.Enqueue(new Event(EventType.HIDE, null));
			Active = false;
		}

		/// <summary>Event callback when the text used by the native keyboard got changed</summary>
		/// <param name="text">The changed text</param>
		public void OnKeyboardTextChanged(string text)
		{
			eventQueue.Enqueue(new Event(EventType.TEXT_CHANGE, text));
		}

		/// <summary>Event callback when the keyboard height changes</summary>
		/// <param name="keyboardHeightString">The string value for keyboard height</param>
		public void OnKeyboardHeightChanged(string keyboardHeightString)
		{
			int keyboardHeight = 0;
			if(int.TryParse(keyboardHeightString, out keyboardHeight))
			{
				if(onKeyboardHeightChanged != null)
				{
					onKeyboardHeightChanged.Invoke(keyboardHeight);
				}

				if(keyboardHeight == 0) //Safety check if something external caused the keyboard to hide
				{
					Active = false;
				}
			}
		}

		/// <summary>Event callback when the cancel key of the keyboard (back key on Android) gets pressed</summary>
		public void OnKeyboardCancel()
		{
			eventQueue.Enqueue(new Event(EventType.CANCEL));
		}

		/// <summary>Event callback when the delete/backspace key of the keyboard gets pressed</summary>
		public void OnKeyboardDelete()
		{
			eventQueue.Enqueue(new Event(EventType.DELETE));
		}

		/// <summary>Event callback when the done key of the keyboard gets pressed</summary>
		public void OnKeyboardDone()
		{
			eventQueue.Enqueue(new Event(EventType.DONE));
		}

		/// <summary>Event callback when the next key of the keyboard gets pressed</summary>
		public void OnKeyboardNext()
		{
			eventQueue.Enqueue(new Event(EventType.NEXT));
		}

		/// <summary>Event callback when the selection got changed natively</summary>
		public void OnSelectionChanged(string valueString)
		{
			eventQueue.Enqueue(new Event(EventType.SELECTION_CHANGE, valueString));
		}
	}
}
