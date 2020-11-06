//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEditor;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	public class EditorExtensions
	{
		private const string INPUT_FIELD_PREFAB_PATH = "Assets/Plugins/Advanced Input Field/Prefabs/InputField.prefab";

		[MenuItem("Advanced Input Field/Create InputField")]
		public static void CreateInputField()
		{
			GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(INPUT_FIELD_PREFAB_PATH, typeof(GameObject));
			CreateInstance(prefab);
		}

		private static void CreateInstance(GameObject prefab)
		{
			GameObject instance = GameObject.Instantiate(prefab);

			Transform parentTransform = Selection.activeTransform;
			instance.transform.SetParent(parentTransform);
			instance.transform.localPosition = Vector3.zero;
			instance.transform.localScale = Vector3.one;

			string name = instance.name;
			name = name.Substring(0, name.Length - "(Clone)".Length);
			instance.name = name;
		}
	}
}