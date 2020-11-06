//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if !UNITY_EDITOR && UNITY_WSA
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using UWPKeyboard;
#endif

namespace AdvancedInputFieldPlugin {
    /// <summary>Configuration preset for the content of this InputField</summary>
    public enum ContentType { Standard, Autocorrected, IntegerNumber, DecimalNumber, Alphanumeric, Name, EmailAddress, Password, Pin, Custom }

    /// <summary>The type of input</summary>
    public enum InputType { Standard, AutoCorrect, Password }

    /// <summary>The keyboard on mobile to use</summary>
    public enum KeyboardType { Default, ASCIICapable, NumbersAndPunctuation, URL, NumberPad, PhonePad, EmailAddress }

    /// <summary>The validation to use for the text</summary>
    public enum CharacterValidation { None, Integer, Decimal, Alphanumeric, Name, EmailAddress }

    /// <summary>The type of line</summary>
    public enum LineType { SingleLine, MultiLineSubmit, MultiLineNewline }

    /// <summary>Determines what to use as start of the drag</summary>
    public enum DragMode { FROM_CURRENT_POSITION, FROM_SELECTION_START, FROM_SELECTION_END }

    /// <summary>The reason for ending edit mode</summary>
    public enum EndEditReason { DESELECT, KEYBOARD_CANCEL, KEYBOARD_DONE, KEYBOARD_NEXT }

    /// <summary>The main class of Advanced Input Field</summary>
    [RequireComponent(typeof(RectTransform))]
    public class AdvancedInputField : Selectable, IPointerClickHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IUpdateSelectedHandler {
        /// <summary>Event used for selection change</summary>
        [Serializable]
        public class SelectionChangedEvent : UnityEvent<bool> { }

        /// <summary>Event used for edit end</summary>
        [Serializable]
        public class EndEditEvent : UnityEvent<string, EndEditReason> { }

        /// <summary>Event used for text value change</summary>
        [Serializable]
        public class ValueChangedEvent : UnityEvent<string> { }

        /// <summary>The time to scroll one line down when using multiline</summary>
        private const float SCROLL_TIME = 0.5f;

        [Tooltip("The main text renderer")]
        [SerializeField]
        private Text textRenderer;

        [Tooltip("The text renderer used for text that has been processed")]
        [SerializeField]
        private Text processedTextRenderer;

        [Tooltip("The caret renderer")]
        [SerializeField]
        private Image caretRenderer;

        [Tooltip("The renderer for text selection")]
        [SerializeField]
        private TextSelectionRenderer selectionRenderer;

        [Tooltip("The main text string")]
        [SerializeField]
        private string text;

        [Tooltip("The placeholder text string")]
        [SerializeField]
        private string placeholderText;

        [Tooltip("The maximum amount of characters allowed, zero means infinite")]
        [SerializeField]
        private int characterLimit;

        [Tooltip("Configuration preset for the content of this InputField")]
        [SerializeField]
        private ContentType contentType;

        [Tooltip("The type of line")]
        [SerializeField]
        private LineType lineType;

        [Tooltip("The type of input")]
        [SerializeField]
        private InputType inputType;

        [Tooltip("The keyboard on mobile to use")]
        [SerializeField]
        private KeyboardType keyboardType;

        [Tooltip("The validation to use for the text")]
        [SerializeField]
        private CharacterValidation characterValidation;

        [Tooltip("The amount of lines")]
        [SerializeField]
        private int lineCount;

        [Tooltip("The filter (if any) to use whenever text or caret position changes")]
        [SerializeField]
        private LiveProcessingFilter liveProcessingFilter;

        [Tooltip("The filter (if any) to use when input field has been deselected")]
        [SerializeField]
        private PostProcessingFilter postProcessingFilter;

        [Tooltip("The blink rate of the caret")]
        [SerializeField]
        [Range(0.1f, 4f)]
        private float caretBlinkRate = 0.85f;

        [Tooltip("The width of the caret")]
        [SerializeField]
        [Range(0.01f, 10)]
        private float caretWidth = 2;

        [Tooltip("The color of the caret")]
        [SerializeField]
        private Color caretColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        [Tooltip("The color of the text selection")]
        [SerializeField]
        private Color selectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

        [Tooltip("Indicates if this input field is read only")]
        [SerializeField]
        private bool readOnly = false;

        [Tooltip("Event used for selection change")]
        [SerializeField]
        private SelectionChangedEvent onSelectionChanged = new SelectionChangedEvent();

        [Tooltip("Event used for edit end")]
        [SerializeField]
        private EndEditEvent onEndEdit = new EndEditEvent();

        [Tooltip("Event used for text value change")]
        [SerializeField]
        private ValueChangedEvent onValueChanged = new ValueChangedEvent();

        [Tooltip("Indicates if the ActionBar should be used on mobile")]
        [SerializeField]
        private bool actionBar = false;

        [Tooltip("Indicates if the cut option should be enabled in the ActionBar")]
        [SerializeField]
        private bool actionBarCut = true;

        [Tooltip("Indicates if the copy option should be enabled in the ActionBar")]
        [SerializeField]
        private bool actionBarCopy = true;

        [Tooltip("Indicates if the paste option should be enabled in the ActionBar")]
        [SerializeField]
        private bool actionBarPaste = true;

        [Tooltip("Indicates if the select all option should be enabled in the ActionBar")]
        [SerializeField]
        private bool actionBarSelectAll = true;

        [Tooltip("Indicates if the Selection Cursors (handles for selection start and end) should be used on mobile")]
        [SerializeField]
        private bool selectionCursors = false;

        [Tooltip("The scale of the selection cursors (1 is default)")]
        [SerializeField]
        [Range(0.01f, 10)]
        private float selectionCursorsScale = 1;

        [Tooltip("The next input field (if any) to switch to when pressing the done button on the TouchScreenKeyboard")]
        [SerializeField]
        private AdvancedInputField nextInputField;

        private string processedText;

        /// <summary>The RectTransform</summary>
        private RectTransform rectTransform;

        /// <summary>The TextInputHandler for current platform</summary>
        private TextInputHandler textInputHandler;

        /// <summary>The TextNavigator for current platform</summary>
        private TextNavigator textNavigator;

        /// <summary>The TextManipulator for current platform</summary>
        private TextManipulator textManipulator;

        /// <summary>The start position on the drag (as character index)</summary>
        private int dragStartPosition;

        /// <summary>Indicates if drag position is out of bounds</summary>
        private bool dragOutOfBounds;

        /// <summary>Indicates if the drag state should keep updating (for drag out of bounds)</summary>
        private bool updateDrag;

        /// <summary>The current amount of time dragging out of bounds</summary>
        private float dragOutOfBoundsTime;

        /// <summary>The last drag event data</summary>
        private PointerEventData lastDragEventData;

        /// <summary>The reason for ending edit mode</summary>
        private EndEditReason endEditReason;

        /// <summary>The Canvas</summary>
        private Canvas canvas;

        /// <summary>The Canvas</summary>
        public Canvas Canvas {
            get {
                if (canvas == null) {
                    canvas = GetComponentInParent<Canvas>();
                    if (canvas != null && textInputHandler != null && textNavigator != null) {
                        textInputHandler.OnCanvasScaleChanged(canvas.scaleFactor);
                        textNavigator.OnCanvasScaleChanged(canvas.scaleFactor);

                        lastCanvasScaleFactor = canvas.scaleFactor;
                    }
                }

                return canvas;
            }
        }

        /// <summary>The last known canvas scale factor</summary>
        private float lastCanvasScaleFactor;

#if !UNITY_EDITOR && UNITY_WSA
		private object currentUserInteractionModeLock = new object();
		private UserInteractionMode currentUserInteractionMode;

		public UserInteractionMode CurrentUserInteractionMode
		{
			get
			{
				lock(currentUserInteractionModeLock)
				{
					return currentUserInteractionMode;
				}
			}
			private set
			{
				lock(currentUserInteractionModeLock)
				{
					currentUserInteractionMode = value;
				}
			}
		}
#endif
        /// <summary>Indicates if input field is selected</summary>
        private bool selected;

        /// <summary>The current drag mode</summary>
        public DragMode CurrentDragMode { get; set; }

        /// <summary>The offset to use when determining drag position</summary>
        public Vector2 DragOffset { get; set; }

        /// <summary>The filter (if any) to use when input field has been deselected</summary>
        public PostProcessingFilter Filter { get { return postProcessingFilter; } }

        /// <summary>Configuration preset for the content of this InputField</summary>
        public ContentType ContentType { get { return contentType; } }

        /// <summary>The type of line</summary>
        public LineType LineType { get { return lineType; } }

        /// <summary>The type of input</summary>
        public InputType InputType { get { return inputType; } }

        /// <summary>The keyboard on mobile to use</summary>
        public KeyboardType KeyboardType { get { return keyboardType; } }

        /// <summary>The validation to use for the text</summary>
        public CharacterValidation CharacterValidation { get { return characterValidation; } }

        /// <summary>Indicates if autocorrection should be used</summary>
        public bool AutoCorrection { get { return inputType == InputType.AutoCorrect; } }

        /// <summary>Indicates is input should be secure</summary>
        public bool Secure { get { return inputType == InputType.Password; } }

        /// <summary>Indicates if line type is multiline</summary>
        public bool Multiline { get { return lineType != LineType.SingleLine; } }

        /// <summary>Indicates if this InputField is initialized</summary>
        public bool Initialized { get; private set; }

        /// <summary>Indicates if this input field is read only</summary>
        public bool ReadOnly {
            get { return readOnly; }
            set {
                readOnly = value;

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
				if(readOnly && Selected && NativeTouchScreenKeyboard.Keyboard != null)
				{
					NativeTouchScreenKeyboard.HideKeyboard();
                }
#endif
            }
        }

        /// <summary>The amount of lines</summary>
        public int LineCount { get { return lineCount; } }

        /// <summary>The blink rate of the caret</summary>
        public float CaretBlinkRate { get { return caretBlinkRate; } }

        /// <summary>The width of the caret</summary>
        public float CaretWidth { get { return caretWidth; } }

        /// <summary>The color of the caret</summary>
        public Color CaretColor { get { return caretColor; } }

        /// <summary>The color of the text selection</summary>
        public Color SelectionColor { get { return selectionColor; } }

        /// <summary>Event used for selection change</summary>
        public SelectionChangedEvent OnSelectionChanged { get { return onSelectionChanged; } }

        /// <summary>Event used for end edit</summary>
        public EndEditEvent OnEndEdit { get { return onEndEdit; } }

        /// <summary>Event used for value change</summary>
        public ValueChangedEvent OnValueChanged { get { return onValueChanged; } }

        /// <summary>Indicates if the ActionBar should be used on mobile</summary>
        public bool ActionBarEnabled { get { return actionBar; } }

        /// <summary>Indicates if the cut option should be enabled in the ActionBar</summary>
        public bool ActionBarCut { get { return actionBarCut; } }

        /// <summary>Indicates if the copy option should be enabled in the ActionBar</summary>
        public bool ActionBarCopy { get { return actionBarCopy; } }

        /// <summary>Indicates if the paste option should be enabled in the ActionBar</summary>
        public bool ActionBarPaste { get { return actionBarPaste; } }

        /// <summary>Indicates if the select all option should be enabled in the ActionBar</summary>
        public bool ActionBarSelectAll { get { return actionBarSelectAll; } }

        /// <summary>Indicates if the Selection Cursors (handles for selection start and end) should be used on mobile</summary>
        public bool SelectionCursorsEnabled { get { return selectionCursors; } }

        /// <summary>The scale of the selection cursors (1 is default)</summary>
        public float SelectionCursorsScale { get { return selectionCursorsScale; } }

        /// <summary>The next input field (if any) to switch to when pressing the done button on the TouchScreenKeyboard</summary>
        public AdvancedInputField NextInputField { get { return nextInputField; } }

        /// <summary>The main text string</summary>
        public string Text {
            get { return text; }
            set {
                if (text != value) {
                    text = value;
                    if (!Initialized) {
                        return;
                    }
                    textNavigator.RefreshRenderedText();

                    if (string.IsNullOrEmpty(text) || LiveProcessing) {
                        textRenderer.enabled = false;
                        processedTextRenderer.enabled = true;
                    } else {
                        textRenderer.enabled = true;
                        processedTextRenderer.enabled = false;
                    }

                    if (onValueChanged != null) {
                        onValueChanged.Invoke(text);
                    }

                    if (string.IsNullOrEmpty(text)) {
                        ProcessedTextRenderer.text = placeholderText;
                    }

                    if (LiveProcessing) {
                        string processedText = liveProcessingFilter.ProcessText(text, textNavigator.CaretPosition);
                        if (processedText != null) {
                            ProcessedText = processedText;

                            if (Selected) {
                                int caretPosition = textNavigator.CaretPosition;
                                int processedCaretPosition = liveProcessingFilter.DetermineProcessedCaret(text, caretPosition, processedText);
                                textNavigator.ProcessedCaretPosition = processedCaretPosition;
                            }
                        }
                    } else {
                        if (Selected) {
                            if (textNavigator.CaretPosition > text.Length) {
                                textNavigator.CaretPosition = text.Length;
                            }
                        }
                    }
                }
            }
        }

        public string ProcessedText {
            get { return processedText; }
            set {
                processedText = value;
                textNavigator.RefreshRenderedText();

                if (string.IsNullOrEmpty(text) || LiveProcessing) {
                    textRenderer.enabled = false;
                    processedTextRenderer.enabled = true;
                } else {
                    textRenderer.enabled = true;
                    processedTextRenderer.enabled = false;
                }
            }
        }

        /// <summary>The placeholder text string</summary>
        public string PlaceHolderText {
            get { return placeholderText; }
            set {
                placeholderText = value;
                if (Initialized && string.IsNullOrEmpty(Text)) {
                    ProcessedTextRenderer.text = placeholderText;
                }
            }
        }

        /// <summary>The selected text string</summary>
        public string SelectedText {
            get {
                if (Initialized) {
                    return textNavigator.SelectedText;
                }

                return string.Empty;
            }
        }

        /// <summary>The maximum amount of characters allowed, zero means infinite</summary>
        public int CharacterLimit {
            get { return characterLimit; }
            set { characterLimit = value; }
        }

        public Text TextRenderer {
            get { return textRenderer; }
        }

        public Text ProcessedTextRenderer {
            get { return processedTextRenderer; }
        }

        /// <summary>The text that is actually rendered</summary>
        public string RenderedText {
            get {
                if (LiveProcessing) {
                    return processedTextRenderer.text;
                } else {
                    return textRenderer.text;
                }
            }
            set {
                if (LiveProcessing) {
                    if (processedTextRenderer.text != value) {
                        processedTextRenderer.text = value;
                        processedTextRenderer.UpdateImmediately();
                        textNavigator.UpdateSelection();
                    }
                } else {
                    if (textRenderer.text != value) {
                        textRenderer.text = value;
                        textRenderer.UpdateImmediately();
                        textNavigator.UpdateSelection();
                    }
                }
            }
        }

        /// <summary>Indicates if multiline</summary>
        public bool IsMultiLine {
            get { return lineCount < 1 || lineCount > 1; }
        }

        public bool LiveProcessing {
            get { return liveProcessingFilter != null; }
        }

        /// <summary>Indicates if Enter key should submit</summary>
        public bool ShouldSubmit {
            get { return (lineType != LineType.MultiLineNewline); }
        }

        /// <summary>Indicates if input field is a password field</summary>
        public bool IsPasswordField {
            get { return inputType == InputType.Password; }
        }

        public LiveProcessingFilter LiveProcessingFilter {
            get { return liveProcessingFilter; }
        }

        /// <summary>Indicates if input field is selected</summary>
        public bool Selected {
            get {
                return selected;
            }
            private set {
                if (selected != value) {
                    selected = value;
                    if (onSelectionChanged != null) {
                        onSelectionChanged.Invoke(selected);
                    }

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
					if(selected && ActionBarEnabled && textInputHandler is MobileTextInputHandler)
					{
						((MobileTextInputHandler)textInputHandler).InitActionBar(this, textRenderer);
					}
#endif
                }
            }
        }

        #region UNITY_METHODS
        protected override void Awake() {
            base.Awake();

            if (UnityEngine.Application.isPlaying) {
                rectTransform = GetComponent<RectTransform>();

#if !UNITY_EDITOR && UNITY_WSA
				if(UWPThreadHelper.Instance == null)
				{
					UWPThreadHelper.CreateInstance(); //Has to be created on Unity thread
				}

				UWPThreadHelper.RunOnUWPThread(
				() =>
				{
					currentUserInteractionMode = UIViewSettings.GetForCurrentView().UserInteractionMode;
					Window.Current.SizeChanged += SizeChanged;
					UWPThreadHelper.ScheduleMessageOnUnityThread(this, "InitializeInputField", null);
				});
#else
                InitializeInputField();
#endif
            }

            CurrentDragMode = DragMode.FROM_CURRENT_POSITION;
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            if (textInputHandler != null) {
                textInputHandler.OnDestroy();
            }
            if (textNavigator != null) {
                textNavigator.OnDestroy();
            }
        }

        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();

            if (Selected && Canvas != null && lastCanvasScaleFactor != Canvas.scaleFactor) {
                textInputHandler.OnCanvasScaleChanged(Canvas.scaleFactor);
                textNavigator.OnCanvasScaleChanged(Canvas.scaleFactor);

                lastCanvasScaleFactor = Canvas.scaleFactor;
            }
        }
        #endregion

        /// <summary>Initializes the InputField</summary>
        public void InitializeInputField() {
#if !UNITY_EDITOR && UNITY_WSA
			if(currentUserInteractionMode == UserInteractionMode.Mouse)
			{
				NativeTouchScreenKeyboard.TryDestroy(); //We don't need the TouchScreenKeyboard if we're in PC Mode
				textInputHandler = new StandaloneTextInputHandler();
				textNavigator = new StandaloneTextNavigator();
				textManipulator = new TextManipulator();
			}
			else if(currentUserInteractionMode == UserInteractionMode.Touch)
			{
				textInputHandler = new MobileTextInputHandler();
				if(ActionBarEnabled)
				{
					((MobileTextInputHandler)textInputHandler).InitActionBar(this, textRenderer);
				}
				textNavigator = new MobileTextNavigator();
				textManipulator = new MobileTextManipulator();
			}
#elif !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
			textInputHandler = new MobileTextInputHandler();
			if(ActionBarEnabled)
			{
				((MobileTextInputHandler)textInputHandler).InitActionBar(this, textRenderer);
			}
			textNavigator = new MobileTextNavigator();
			textManipulator = new MobileTextManipulator();
#else
            textInputHandler = new StandaloneTextInputHandler();
            textNavigator = new StandaloneTextNavigator();
            textManipulator = new TextManipulator();
#endif

            textInputHandler.Initialize(this, textNavigator, textManipulator);
            textNavigator.Initialize(this, caretRenderer, selectionRenderer);
            textManipulator.Initialize(this, textNavigator, textRenderer, processedTextRenderer);

            Initialized = true;
            if (LiveProcessing) {
                string processedText = liveProcessingFilter.ProcessText(text, textNavigator.CaretPosition);
                if (processedText != null) {
                    ProcessedText = processedText;
                }
            }
            textNavigator.EndEditMode();
            textManipulator.EndEditMode();
            Selected = false;
            textNavigator.RefreshRenderedText();
            textNavigator.UpdateCaret();

            if (string.IsNullOrEmpty(Text) || LiveProcessing) {
                textRenderer.enabled = false;
                processedTextRenderer.enabled = true;
            } else {
                textRenderer.enabled = true;
                processedTextRenderer.enabled = false;
            }
        }

#if !UNITY_EDITOR && UNITY_WSA
		public void SizeChanged(object sender, WindowSizeChangedEventArgs args)
		{
			UserInteractionMode userInteractionMode = UIViewSettings.GetForCurrentView().UserInteractionMode;
			if(CurrentUserInteractionMode != userInteractionMode)
			{
				CurrentUserInteractionMode = userInteractionMode;
				UWPThreadHelper.ScheduleMessageOnUnityThread(this, "OnUserInteractionModeChanged", null);
			}
		}

		public void OnUserInteractionModeChanged()
		{
			Deselect(EndEditReason.DESELECT);
			InitializeInputField(); //Reinitialize InputField
		}
#endif

#if UNITY_EDITOR
        #region EDITOR_METHODS
        /// <summary>Applies the text change</summary>
        /// <param name="text">The new text value</param>
        public void ApplyText(string text) {
            textRenderer.text = text;
        }

        /// <summary>Applies the character limit change</summary>
        /// <param name="characterLimit">The new character limit value</param>
        public void ApplyCharacterLimit(int characterLimit) {
            if (characterLimit > 0 && text.Length > characterLimit) {
                text = text.Substring(0, characterLimit);
                textRenderer.text = text;
            }
        }
        #endregion
#endif

        /// <summary>Applies the content type change</summary>
        /// <param name="contentType">The new content type value</param>
        public void UpdateContentType(ContentType contentType) {
            switch (contentType) {
                case ContentType.Standard: {
                        // Don't enforce line type for this content type.
                        inputType = InputType.Standard;
                        keyboardType = KeyboardType.Default;
                        characterValidation = CharacterValidation.None;
                        break;
                    }
                case ContentType.Autocorrected: {
                        // Don't enforce line type for this content type.
                        inputType = InputType.AutoCorrect;
                        keyboardType = KeyboardType.Default;
                        characterValidation = CharacterValidation.None;
                        break;
                    }
                case ContentType.IntegerNumber: {
                        lineType = LineType.SingleLine;
                        inputType = InputType.Standard;
                        keyboardType = KeyboardType.NumberPad;
                        characterValidation = CharacterValidation.Integer;
                        break;
                    }
                case ContentType.DecimalNumber: {
                        lineType = LineType.SingleLine;
                        inputType = InputType.Standard;
                        keyboardType = KeyboardType.NumbersAndPunctuation;
                        characterValidation = CharacterValidation.Decimal;
                        break;
                    }
                case ContentType.Alphanumeric: {
                        lineType = LineType.SingleLine;
                        inputType = InputType.Standard;
                        keyboardType = KeyboardType.ASCIICapable;
                        characterValidation = CharacterValidation.Alphanumeric;
                        break;
                    }
                case ContentType.Name: {
                        lineType = LineType.SingleLine;
                        inputType = InputType.Standard;
                        keyboardType = KeyboardType.Default;
                        characterValidation = CharacterValidation.Name;
                        break;
                    }
                case ContentType.EmailAddress: {
                        lineType = LineType.SingleLine;
                        inputType = InputType.Standard;
                        keyboardType = KeyboardType.EmailAddress;
                        characterValidation = CharacterValidation.EmailAddress;
                        break;
                    }
                case ContentType.Password: {
                        lineType = LineType.SingleLine;
                        inputType = InputType.Password;
                        keyboardType = KeyboardType.Default;
                        characterValidation = CharacterValidation.None;
                        break;
                    }
                case ContentType.Pin: {
                        lineType = LineType.SingleLine;
                        inputType = InputType.Password;
                        keyboardType = KeyboardType.NumberPad;
                        characterValidation = CharacterValidation.Integer;
                        break;
                    }
                default: {
                        // Includes Custom type. Nothing should be enforced.
                        break;
                    }
            }

            EnforceTextHorizontalOverflow();
        }

        /// <summary>Enforces horizontal overflow for the text</summary>
        private void EnforceTextHorizontalOverflow() {
            if (textRenderer != null) {
                if (IsMultiLine) {
                    textRenderer.horizontalOverflow = HorizontalWrapMode.Wrap;
                } else {
                    textRenderer.horizontalOverflow = HorizontalWrapMode.Overflow;
                }
            }
        }

        /// <summary>Clears the InputField</summary>
        public void Clear() {
            Text = string.Empty;
            if (Initialized && !Selected) {
                textManipulator.EndEditMode();
            }
        }

        #region INTERFACE_METHODS
        public void OnPointerClick(PointerEventData eventData) {
            if (!Selected) {
                base.OnSelect(eventData);

                EnableSelection();

                Vector2 localMousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(textRenderer.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);

                textInputHandler.OnPress(localMousePosition);
                textInputHandler.OnRelease(localMousePosition);
            }
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (!Selected) {
                IBeginDragHandler beginDragHandler = transform.parent.GetComponentInParent<IBeginDragHandler>();
                if (beginDragHandler != null) {
                    beginDragHandler.OnBeginDrag(eventData);
                }

                updateDrag = false;

                return;
            }

            updateDrag = true;

            if (CurrentDragMode == DragMode.FROM_CURRENT_POSITION) {
                Vector2 localMousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(textRenderer.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);

                textNavigator.ResetCaret(localMousePosition);
                dragStartPosition = textNavigator.VisibleCaretPosition;
            } else if (CurrentDragMode == DragMode.FROM_SELECTION_START) {
                dragStartPosition = textNavigator.VisibleSelectionStartPosition;
            } else if (CurrentDragMode == DragMode.FROM_SELECTION_END) {
                dragStartPosition = textNavigator.VisibleSelectionEndPosition;
            }

            dragStartPosition += textNavigator.TextOffset;

            eventData.Use();
        }

        public void OnDrag(PointerEventData eventData) {
            if (!Selected) {
                IDragHandler dragHandler = transform.parent.GetComponentInParent<IDragHandler>();
                if (dragHandler != null) {
                    dragHandler.OnDrag(eventData);
                }
                return;
            }

            if (PositionOutOfBounds(eventData)) {
                lastDragEventData = eventData;

                if (!dragOutOfBounds) {
                    dragOutOfBounds = true;
                    dragOutOfBoundsTime = 0;
                }
            } else {
                dragOutOfBounds = false;
            }

            Vector2 localMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(textRenderer.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);

            localMousePosition += DragOffset;

            int position = 0;
            if (LiveProcessing) {
                position = textNavigator.GetCharacterIndexFromPosition(processedTextRenderer, localMousePosition);
                textNavigator.UpdateSelectionArea(position, dragStartPosition, false);
            } else {
                position = textNavigator.GetCharacterIndexFromPosition(textRenderer, localMousePosition);
                textNavigator.UpdateSelectionArea(position, dragStartPosition, false);
            }

            textInputHandler.OnDrag(localMousePosition);

            eventData.Use();
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (!Selected) {
                IEndDragHandler endDragHandler = transform.parent.GetComponentInParent<IEndDragHandler>();
                if (endDragHandler != null) {
                    endDragHandler.OnEndDrag(eventData);
                }
                return;
            }

            dragStartPosition = -1;
            CurrentDragMode = DragMode.FROM_CURRENT_POSITION;

            updateDrag = false;

            eventData.Use();
        }

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);

            if (!Selected) {
                return;
            }

            Vector2 localMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(textRenderer.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);

            textInputHandler.OnPress(localMousePosition);
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);

            if (!Selected) {
                return;
            }

            Vector2 localMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(textRenderer.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);

            textInputHandler.OnRelease(localMousePosition);
        }

        public override void OnSelect(BaseEventData eventData) {
            eventData.Use(); //Ignore the base behaviour, so we can scroll without enabling edit mode (and showing TouchScreenKeyboard)
        }

        public override void OnDeselect(BaseEventData eventData) {
            StartCoroutine(DelayedDeselect());
        }

        public void OnUpdateSelected(BaseEventData eventData) {
            if (textNavigator.EditMode) {
                textInputHandler.Process();
                textNavigator.UpdateSelected();

                if (updateDrag) {
                    UpdateDrag();
                }
            }

            eventData.Use();
        }

        /// <summary>Don't call this method to select this inputfield programmatically, use ManualSelect() instead</summary>
        public override void Select() {
            //Ignore the base behaviour, so we can scroll without enabling edit mode (and showing TouchScreenKeyboard)
        }

        public void Deselect(EndEditReason reason) {
            endEditReason = reason;
            EventSystem.current.SetSelectedGameObject(null);
        }
        #endregion

        /// <summary>Updates the drag for out of bounds text scroll</summary>
        public void UpdateDrag() {
            if (dragOutOfBounds) {
                dragOutOfBoundsTime += Time.deltaTime;

                float scrollTime = SCROLL_TIME;
                if (lineType == LineType.SingleLine) {
                    scrollTime = SCROLL_TIME * 0.25f;
                }

                if (dragOutOfBoundsTime >= scrollTime) {
                    dragOutOfBoundsTime = 0;

                    Vector2 localMousePos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(textRenderer.rectTransform, lastDragEventData.position, lastDragEventData.pressEventCamera, out localMousePos);

                    Rect rect = textRenderer.rectTransform.rect;

                    if (lineType == LineType.SingleLine) {
                        if (localMousePos.x > rect.xMax) {
                            textNavigator.MoveLineRight(1);
                        } else if (localMousePos.x < rect.xMin) {
                            textNavigator.MoveLineLeft(1);
                        }
                    } else {
                        if (localMousePos.y > rect.yMax) {
                            textNavigator.MoveLineUp();
                        } else if (localMousePos.y < rect.yMin) {
                            textNavigator.MoveLineDown();
                        }
                    }
                }
            }
        }

        /// <summary>Checks if position is out of bounds</summary>
        /// <param name="eventData">The event data to check</param>
        /// <returns>true is position is out of bounds</returns>
        public bool PositionOutOfBounds(PointerEventData eventData) {
            return !RectTransformUtility.RectangleContainsScreenPoint(textRenderer.rectTransform, eventData.position, eventData.pressEventCamera);
        }

        /// <summary>Marks as selected and enables edit mode</summary>
        public void EnableSelection() {
            Selected = true;
            textInputHandler.BeginEditMode();
            textNavigator.BeginEditMode();
            textManipulator.BeginEditMode();
        }

        /// <summary>Checks if ActionBar is selected</summary>
        /// <returns>true is ActionBar is selected</returns>
        private bool IsActionBarSelected() {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_WSA)
			GameObject currentSelection = EventSystem.current.currentSelectedGameObject;
			if(currentSelection != null)
			{
				return currentSelection.GetComponentInParent<ActionBarRenderer>();
			}
#endif

            return false;
        }

        /// <summary>Delayed deselect method to check for valid deselects</summary>
        private IEnumerator DelayedDeselect() {
            yield return null;

            if (!Initialized) {
                yield break;
            }

            if (IsActionBarSelected()) //Invalid deselect
            {
                Reselect();
            } else //Valid deselect
              {
                textNavigator.EndEditMode();
                textManipulator.EndEditMode();
                textInputHandler.CancelInput();
                Selected = false;

                if (onEndEdit != null) {
                    onEndEdit.Invoke(Text, endEditReason);
                }
            }

            endEditReason = EndEditReason.DESELECT; //Reset to default reason (deselection)
        }

        /// <summary>(Re)selects the InputField</summary>
        public void Reselect() {
            EventSystem.current.SetSelectedGameObject(gameObject);
            textInputHandler.OnSelect();
        }

        /// <summary>Manually selects this inputfield (call this to select this inputfield programmatically)</summary>
        public void ManualSelect() {
            if (Initialized) {
                Reselect();
                EnableSelection();
                textNavigator.MoveToEnd();
                textInputHandler.Process();
            } else {
                Debug.LogErrorFormat("InputField not yet initialized, please wait until it's ready");
            }
        }
    }
}
