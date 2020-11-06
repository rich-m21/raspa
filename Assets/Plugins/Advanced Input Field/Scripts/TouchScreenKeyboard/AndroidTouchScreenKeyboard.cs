
#if !UNITY_EDITOR && UNITY_ANDROID
using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class that acts as a bridge for the Native Android Keyboard</summary>
	public class AndroidTouchScreenKeyboard: TouchScreenKeyboard
	{
		/// <summary>The main Android class</summary>
		private AndroidJavaClass mainClass;

		/// <summary>Gets the singleton instance of the Native Android Keyboard</summary>
		public AndroidJavaObject instance { get { return mainClass.GetStatic<AndroidJavaObject>("instance"); } }

		protected override void Setup()
		{
			mainClass = new AndroidJavaClass("com.jeroenvanpienbroek.touchscreenkeyboard.TouchScreenKeyboardFragment");
			mainClass.CallStatic("initialize", gameObjectName);
		}

		public override void ShowKeyboard(string text, KeyboardType keyboardType, CharacterValidation characterValidation, LineType lineType, bool autocorrection, bool secure, int characterLimit)
		{
			mainClass.CallStatic("showKeyboard", text, (int)keyboardType, (int)characterValidation, (int)lineType, autocorrection, secure, characterLimit);
		}

		public override void HideKeyboard()
		{
			mainClass.CallStatic("hideKeyboard");
		}

		public override void ChangeKeyboardText(string text, bool ignoreCaretPositionChange)
		{
			mainClass.CallStatic("changeKeyboardText", text, ignoreCaretPositionChange);
		}

		public override void SetSelection(int selectionStartPosition, int selectionEndPosition)
		{
			mainClass.CallStatic("setSelection", selectionStartPosition, selectionEndPosition);
		}

		public override void BlockKeyboardCancel()
		{
			mainClass.CallStatic("blockKeyboardCancel");
		}
	}
}
#endif