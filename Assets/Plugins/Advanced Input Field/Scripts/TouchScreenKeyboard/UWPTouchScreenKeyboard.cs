#if !UNITY_EDITOR && UNITY_WSA
namespace AdvancedInputFieldPlugin
{
	/// <summary>Class that acts as a bridge for the Native UWP Keyboard</summary>
	public class UWPTouchScreenKeyboard: TouchScreenKeyboard
	{
		private UWPKeyboard.TouchScreenKeyboard keyboard;

		protected override void Setup()
		{
			keyboard = new UWPKeyboard.TouchScreenKeyboard();
			keyboard.Initialize(
			(methodName, message) =>
			{
				UWPThreadHelper.ScheduleMessageOnUnityThread(this, methodName, message);
			});
		}

		public override void ShowKeyboard(string text, KeyboardType keyboardType, CharacterValidation characterValidation, LineType lineType, bool autocorrection, bool secure, int characterLimit)
		{
			keyboard.ShowKeyboard(text, (int)keyboardType, (int)characterValidation, (int)lineType, autocorrection, secure, characterLimit);
		}

		public override void HideKeyboard()
		{
			keyboard.HideKeyboard();
		}

		public override void ChangeKeyboardText(string text, bool ignoreCaretPositionChange)
		{
			keyboard.ChangeKeyboardText(text, ignoreCaretPositionChange);
		}

		public override void SetSelection(int selectionStartPosition, int selectionEndPosition)
		{
			keyboard.SetSelection(selectionStartPosition, selectionEndPosition);
		}

		private void OnDestroy()
		{
			keyboard.Destroy();
			keyboard = null;
		}
	}
}
#endif
