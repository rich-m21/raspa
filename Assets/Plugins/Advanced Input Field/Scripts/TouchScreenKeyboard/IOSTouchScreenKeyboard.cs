//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

#if !UNITY_EDITOR && UNITY_IOS
using System.Runtime.InteropServices;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class that acts as a bridge for the Native iOS Keyboard</summary>
	public class IOSTouchScreenKeyboard: TouchScreenKeyboard
	{
		[DllImport("__Internal")]
		private static extern void _initialize(string gameObjectName);
		[DllImport("__Internal")]
		private static extern void _showKeyboard(string text, int keyboardType, int characterValidation, int lineType, bool autocorrection, bool secure, int characterLimit);
		[DllImport("__Internal")]
		private static extern void _hideKeyboard();
		[DllImport("__Internal")]
		private static extern void _changeKeyboardText(string text, bool ignoreSelectionChange);
		[DllImport("__Internal")]
		private static extern void _setSelection(int selectionStartPosition, int selectionEndPosition);

		protected override void Setup()
		{
			_initialize(gameObjectName);
		}

		public override void ShowKeyboard(string text, KeyboardType keyboardType, CharacterValidation characterValidation, LineType lineType, bool autocorrection, bool secure, int characterLimit)
		{
			_showKeyboard(text, (int)keyboardType, (int)characterValidation, (int)lineType, autocorrection, secure, characterLimit);
		}

		public override void HideKeyboard()
		{
			_hideKeyboard();
		}

		public override void ChangeKeyboardText(string text, bool ignoreSelectionChange)
		{
			_changeKeyboardText(text, ignoreSelectionChange);
		}

		public override void SetSelection(int selectionStartPosition, int selectionEndPosition)
		{
			_setSelection(selectionStartPosition, selectionEndPosition);
		}
	}
}
#endif