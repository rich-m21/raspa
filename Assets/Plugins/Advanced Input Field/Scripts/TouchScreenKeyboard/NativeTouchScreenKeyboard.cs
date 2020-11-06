//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEngine;

namespace AdvancedInputFieldPlugin {
    /// <summary>Access point for the NativeTouchScreenKeyboard for current platform</summary>
    public class NativeTouchScreenKeyboard : MonoBehaviour {
        /// <summary>The singleton instance of NativeTouchScreenKeyboard</summary>
        private static NativeTouchScreenKeyboard instance;

        /// <summary>The TouchScreenKeyboard instance of current platform</summary>
        private TouchScreenKeyboard keyboard;

        /// <summary>The singleton instance of NativeTouchScreenKeyboard</summary>
        private static NativeTouchScreenKeyboard Instance {
            get {
                if (instance == null) {
                    instance = GameObject.FindObjectOfType<NativeTouchScreenKeyboard>();
                    if (instance == null) {
                        GameObject gameObject = new GameObject("NativeTouchScreenKeyboard");
                        instance = gameObject.AddComponent<NativeTouchScreenKeyboard>();
                    }
                }

                return instance;
            }
        }

        /// <summary>The TouchScreenKeyboard instance of current platform</summary>
        public static TouchScreenKeyboard Keyboard {
            get { return Instance.keyboard; }
        }

        #region UNITY
        private void Awake() {
#if UNITY_EDITOR
            Debug.LogWarning("Native TouchScreen Keyboard can't be accessed in the Editor");
#elif UNITY_ANDROID
			keyboard = gameObject.AddComponent<AndroidTouchScreenKeyboard>();
			keyboard.Init(name);
#elif UNITY_IOS
			keyboard = gameObject.AddComponent<IOSTouchScreenKeyboard>();
			keyboard.Init(name);
#elif UNITY_WSA
			keyboard = gameObject.AddComponent<UWPTouchScreenKeyboard>();
			keyboard.Init(name);
#else
			Debug.LogWarning("Native TouchScreen Keyboard is only supported on Android, iOS and UWP");
#endif
        }

        private void OnDestroy() {
            instance = null;
        }
        #endregion

        public static void TryDestroy() {
            if (instance != null && instance.gameObject != null) {
                Destroy(instance.gameObject);
            }
        }

        /// <summary>Shows the TouchScreenKeyboard for current platform</summary>
        /// <param name="text">The current text of the InputField</param>
        /// <param name="keyboardType">The keyboard type to use</param>
        /// <param name="characterValidation">The characterValidation to use</param>
        /// <param name="lineType">The lineType to use</param>
        /// <param name="autocorrection">Indicates whether autocorrection is enabled</param>
        /// <param name="characterLimit">The character limit for the text</param>
        /// <param name="secure">Indicates whether input should be secure</param>
        public static void ShowKeyboard(string text, KeyboardType keyboardType = KeyboardType.Default, CharacterValidation characterValidation = CharacterValidation.None, LineType lineType = LineType.SingleLine, bool autocorrection = true, bool secure = false, int characterLimit = 0) {
            Instance.keyboard.ShowKeyboard(text, keyboardType, characterValidation, lineType, autocorrection, secure, characterLimit);

        }

        /// <summary>Hides the TouchScreenKeyboard for current platform</summary>
        public static void HideKeyboard() {
            Instance.keyboard.HideKeyboard();

        }

        /// <summary>Adds a KeyboardHeightChangeListener</summary>
        /// <param name="listener">The KeyboardHeightChangeListener to add</param>
        public static void AddKeyboardHeightChangedListener(OnKeyboardHeightChangedHandler listener) {
            Instance.keyboard.AddKeyboardHeightChangedListener(listener);
        }

        /// <summary>Removes a KeyboardHeightChangeListener</summary>
        /// <param name="listener">The KeyboardHeightChangeListener to remove</param>
        public static void RemoveKeyboardHeightChangedListener(OnKeyboardHeightChangedHandler listener) {
            Instance.keyboard.RemoveKeyboardHeightChangedListener(listener);
        }
    }
}
