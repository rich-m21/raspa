//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEditor;

namespace AdvancedInputFieldPlugin.Editor
{
	[CustomEditor(typeof(AdvancedInputField), true)]
	public class AdvancedInputFieldEditor: UnityEditor.Editor
	{
		private SerializedProperty interactableProperty; //Property from Selectable base class

		private SerializedProperty textRendererProperty;
		private SerializedProperty processedTextRendererProperty;
		private SerializedProperty caretRendererProperty;
		private SerializedProperty selectionRendererProperty;
		private SerializedProperty textProperty;
		private SerializedProperty placeholderTextProperty;
		private SerializedProperty characterLimitProperty;
		private SerializedProperty contentTypeProperty;
		private SerializedProperty lineTypeProperty;
		private SerializedProperty inputTypeProperty;
		private SerializedProperty keyboardTypeProperty;
		private SerializedProperty characterValidationProperty;
		private SerializedProperty liveProcessingFilterProperty;
		private SerializedProperty postProcessingFilterProperty;
		private SerializedProperty caretBlinkRateProperty;
		private SerializedProperty caretWidthProperty;
		private SerializedProperty caretColorProperty;
		private SerializedProperty selectionColorProperty;
		private SerializedProperty readOnlyProperty;
		private SerializedProperty onSelectionChangedProperty;
		private SerializedProperty onEndEditProperty;
		private SerializedProperty onValueChangedProperty;
		private SerializedProperty actionBarProperty;
		private SerializedProperty actionBarCutProperty;
		private SerializedProperty actionBarCopyProperty;
		private SerializedProperty actionBarPasteProperty;
		private SerializedProperty actionBarSelectAllProperty;
		private SerializedProperty selectionCursorsProperty;
		private SerializedProperty selectionCursorsScaleProperty;
		private SerializedProperty nextInputFieldProperty;

		private void OnEnable()
		{
			interactableProperty = serializedObject.FindProperty("m_Interactable");

			textRendererProperty = serializedObject.FindProperty("textRenderer");
			processedTextRendererProperty = serializedObject.FindProperty("processedTextRenderer");
			caretRendererProperty = serializedObject.FindProperty("caretRenderer");
			selectionRendererProperty = serializedObject.FindProperty("selectionRenderer");
			textProperty = serializedObject.FindProperty("text");
			placeholderTextProperty = serializedObject.FindProperty("placeholderText");
			characterLimitProperty = serializedObject.FindProperty("characterLimit");
			contentTypeProperty = serializedObject.FindProperty("contentType");
			lineTypeProperty = serializedObject.FindProperty("lineType");
			inputTypeProperty = serializedObject.FindProperty("inputType");
			keyboardTypeProperty = serializedObject.FindProperty("keyboardType");
			characterValidationProperty = serializedObject.FindProperty("characterValidation");
			liveProcessingFilterProperty = serializedObject.FindProperty("liveProcessingFilter");
			postProcessingFilterProperty = serializedObject.FindProperty("postProcessingFilter");
			caretBlinkRateProperty = serializedObject.FindProperty("caretBlinkRate");
			caretWidthProperty = serializedObject.FindProperty("caretWidth");
			caretColorProperty = serializedObject.FindProperty("caretColor");
			selectionColorProperty = serializedObject.FindProperty("selectionColor");
			readOnlyProperty = serializedObject.FindProperty("readOnly");
			onSelectionChangedProperty = serializedObject.FindProperty("onSelectionChanged");
			onEndEditProperty = serializedObject.FindProperty("onEndEdit");
			onValueChangedProperty = serializedObject.FindProperty("onValueChanged");
			actionBarProperty = serializedObject.FindProperty("actionBar");
			actionBarCutProperty = serializedObject.FindProperty("actionBarCut");
			actionBarCopyProperty = serializedObject.FindProperty("actionBarCopy");
			actionBarPasteProperty = serializedObject.FindProperty("actionBarPaste");
			actionBarSelectAllProperty = serializedObject.FindProperty("actionBarSelectAll");
			selectionCursorsProperty = serializedObject.FindProperty("selectionCursors");
			selectionCursorsScaleProperty = serializedObject.FindProperty("selectionCursorsScale");
			nextInputFieldProperty = serializedObject.FindProperty("nextInputField");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			AdvancedInputField inputField = (AdvancedInputField)target;

			EditorGUILayout.PropertyField(interactableProperty);

			EditorGUILayout.PropertyField(textRendererProperty);
			EditorGUILayout.PropertyField(processedTextRendererProperty);
			EditorGUILayout.PropertyField(caretRendererProperty);
			EditorGUILayout.PropertyField(selectionRendererProperty);

			DrawTextProperties(inputField);
			DrawCharacterLimitProperty(inputField);
			DrawContentTypeProperties(inputField);

			EditorGUILayout.PropertyField(liveProcessingFilterProperty);
			EditorGUILayout.PropertyField(postProcessingFilterProperty);
			DrawCaretProperties();

			EditorGUILayout.PropertyField(selectionColorProperty);
			EditorGUILayout.PropertyField(readOnlyProperty);
			EditorGUILayout.PropertyField(onSelectionChangedProperty);
			EditorGUILayout.PropertyField(onEndEditProperty);
			EditorGUILayout.PropertyField(onValueChangedProperty);
			EditorGUILayout.PropertyField(nextInputFieldProperty);

			DrawMobileOnlyProperties();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawTextProperties(AdvancedInputField inputField)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(textProperty);
			if(EditorGUI.EndChangeCheck())
			{
				inputField.ApplyText(textProperty.stringValue);
			}

			EditorGUILayout.PropertyField(placeholderTextProperty);
		}

		private void DrawCharacterLimitProperty(AdvancedInputField inputField)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(characterLimitProperty);
			if(EditorGUI.EndChangeCheck())
			{
				inputField.ApplyCharacterLimit(characterLimitProperty.intValue);
				textProperty.stringValue = inputField.Text;
			}
		}

		private void DrawContentTypeProperties(AdvancedInputField inputField)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(contentTypeProperty);
			if(EditorGUI.EndChangeCheck())
			{
				ContentType contentType = ((ContentType)contentTypeProperty.enumValueIndex);
				inputField.UpdateContentType(contentType);
				lineTypeProperty.enumValueIndex = (int)inputField.LineType;
				inputTypeProperty.enumValueIndex = (int)inputField.InputType;
				keyboardTypeProperty.enumValueIndex = (int)inputField.KeyboardType;
				characterValidationProperty.enumValueIndex = (int)inputField.CharacterValidation;
			}

			EditorGUI.indentLevel = 1;
			EditorGUILayout.PropertyField(lineTypeProperty);
			if(((ContentType)contentTypeProperty.enumValueIndex) == ContentType.Custom)
			{
				EditorGUILayout.PropertyField(inputTypeProperty);
				EditorGUILayout.PropertyField(keyboardTypeProperty);
				EditorGUILayout.PropertyField(characterValidationProperty);
			}
			EditorGUI.indentLevel = 0;
		}

		private void DrawCaretProperties()
		{
			EditorGUILayout.PropertyField(caretBlinkRateProperty);
			EditorGUILayout.PropertyField(caretWidthProperty);
			EditorGUILayout.PropertyField(caretColorProperty);
		}

		private void DrawMobileOnlyProperties()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Mobile only:", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(actionBarProperty);
			if(actionBarProperty.boolValue)
			{
				EditorGUILayout.PropertyField(actionBarCutProperty);
				EditorGUILayout.PropertyField(actionBarCopyProperty);
				EditorGUILayout.PropertyField(actionBarPasteProperty);
				EditorGUILayout.PropertyField(actionBarSelectAllProperty);
			}

			EditorGUILayout.PropertyField(selectionCursorsProperty);
			if(selectionCursorsProperty.boolValue)
			{
				EditorGUILayout.PropertyField(selectionCursorsScaleProperty);
			}
		}
	}
}