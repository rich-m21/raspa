//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEngine;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WSA || UNITY_WEBGL
namespace AdvancedInputFieldPlugin
{
	/// <summary>Subclass of TextInputHandler for Standalone platforms</summary>
	public class StandaloneTextInputHandler: TextInputHandler
	{
		/// <summary>The StandaloneTextNavigator</summary>
		private StandaloneTextNavigator standaloneTextNavigator;

		public override TextNavigator TextNavigator
		{
			get { return standaloneTextNavigator; }
			protected set { standaloneTextNavigator = (StandaloneTextNavigator)value; }
		}

		public StandaloneTextInputHandler()
		{
		}

		public override void Process()
		{
			base.Process();

			Event keyboardEvent = new Event();
			while(Event.PopEvent(keyboardEvent))
			{
				if(keyboardEvent.rawType == EventType.KeyDown)
				{
					bool shouldContinue = ProcessKeyboardEvent(keyboardEvent);
					if(!shouldContinue)
					{
						return;
					}
				}

				if((keyboardEvent.type == EventType.ValidateCommand || keyboardEvent.type == EventType.ExecuteCommand)
					&& keyboardEvent.commandName == "SelectAll")
				{
					TextNavigator.SelectAll();
				}
			}
		}

		/// <summary>Processes a keyboard event</summary>
		/// <param name="keyboardEvent">The keyboard event to process</param>
		protected bool ProcessKeyboardEvent(Event keyboardEvent)
		{
			EventModifiers currentEventModifiers = keyboardEvent.modifiers;
			bool ctrl = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
			bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
			bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
			bool ctrlOnly = ctrl && !alt && !shift;

			switch(keyboardEvent.keyCode)
			{
				case KeyCode.Backspace:
					TextManipulator.DeletePreviousChar();
					return true;
				case KeyCode.Delete:
					TextManipulator.DeleteNextChar();
					return true;
				case KeyCode.Home:
					TextNavigator.MoveToStart();
					return true;
				case KeyCode.End:
					TextNavigator.MoveToEnd();
					return true;
				case KeyCode.A: //Select All
					if(ctrlOnly)
					{
						TextNavigator.SelectAll();
						return true;
					}
					break;
				case KeyCode.C: //Copy
					if(ctrlOnly)
					{
						TextManipulator.Copy();
						return true;
					}
					break;
				case KeyCode.V: //Paste
					if(ctrlOnly)
					{
						TextManipulator.Paste();
						return true;
					}
					break;
				case KeyCode.X: //Cut
					if(ctrlOnly)
					{
						TextManipulator.Cut();
						return true;
					}
					break;
				case KeyCode.LeftArrow:
					standaloneTextNavigator.MoveLeft(shift, ctrl);
					return true;
				case KeyCode.RightArrow:
					standaloneTextNavigator.MoveRight(shift, ctrl);
					return true;
				case KeyCode.DownArrow:
					standaloneTextNavigator.MoveDown(shift, ctrl);
					return true;
				case KeyCode.UpArrow:
					standaloneTextNavigator.MoveUp(shift, ctrl);
					return true;
				case KeyCode.Return: //Submit
				case KeyCode.KeypadEnter: //Submit
					if(InputField.ShouldSubmit)
					{
						InputField.Deselect(EndEditReason.KEYBOARD_DONE);
						return false;
					}
					break;
				case KeyCode.Escape:
					InputField.Deselect(EndEditReason.KEYBOARD_CANCEL);
					return false;
				case KeyCode.Tab:
					ClearKeyboardEvents();
					standaloneTextNavigator.GoToNextInputField();
					return false;
			}

			char c = keyboardEvent.character;
			if(!InputField.IsMultiLine && (c == '\t' || c == '\r' || c == 10)) //Don't allow return chars or tabulator key to be entered into single line fields.
			{
				return true;
			}

			if(c == '\r' || (int)c == 3) //Convert carriage return and end-of-text characters to newline.
			{
				c = '\n';
			}

			TextManipulator.TryInsertChar(c);

			return true;
		}

		public void ClearKeyboardEvents()
		{
			Event keyboardEvent = new Event();
			while(Event.PopEvent(keyboardEvent)) { } //Just remove all remaining queued events
		}
	}
}
#endif