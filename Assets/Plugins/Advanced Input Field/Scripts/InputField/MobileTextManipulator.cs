//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
namespace AdvancedInputFieldPlugin
{
	/// <summary>Subclass of TextManipulator for mobile platforms</summary>
	public class MobileTextManipulator: TextManipulator
	{
		/// <summary>The TouchScreenKeyboard</summary>
		private TouchScreenKeyboard keyboard;

		/// <summary>The MobileTextNavigator</summary>
		private MobileTextNavigator mobileTextNavigator;

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

		public override string Text
		{
			get { return base.Text; }
			set
			{
				base.Text = value;
				if(!InputField.ReadOnly && !BlockNativeTextChange)
				{
					Keyboard.ChangeKeyboardText(value, true);
				}
			}
		}

		public override void Insert(char c)
		{
			base.Insert(c);

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

		public override void Copy()
		{
			base.Copy();

			if(InputField.ActionBarEnabled)
			{
				mobileTextNavigator.actionBar.Hide();
				mobileTextNavigator.CaretToSelectionEnd();
				mobileTextNavigator.CancelSelection();
			}
		}

		public override void Paste()
		{
			base.Paste();

			if(InputField.ActionBarEnabled)
			{
				mobileTextNavigator.actionBar.Hide();
			}
		}

		public override void Cut()
		{
			base.Cut();

			if(InputField.ActionBarEnabled)
			{
				mobileTextNavigator.actionBar.Hide();
			}
		}
	}
}
#endif