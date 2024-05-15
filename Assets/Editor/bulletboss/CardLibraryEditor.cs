using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.draco18s.bulletboss;
using Assets.draco18s.bulletboss.pattern;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(CardLibrary))]
public class CardLibraryEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (GUILayout.Button("Find all Cards"))
		{
			SerializedProperty modulesProp = serializedObject.FindProperty("modules");
			SerializedProperty modifiersProp = serializedObject.FindProperty("modifiers");
			modulesProp.arraySize = 0;
			modifiersProp.arraySize = 0;
			string[] locations = new string[]
			{
				"Assets/ScriptableObjects/0 Starting",
				"Assets/ScriptableObjects/1 Common",
				"Assets/ScriptableObjects/2 Uncommon",
				"Assets/ScriptableObjects/3 Rare",
				"Assets/ScriptableObjects/4 Epic",
				"Assets/ScriptableObjects/5 Ultra Rare",
				"Assets/ScriptableObjects/6 Legendary",
				"Assets/ScriptableObjects/7 Artifact",
			};
			string[] modules = AssetDatabase.FindAssets("t:PatternModuleType", locations);

			string[] query = modules.Select(AssetDatabase.GUIDToAssetPath).ToArray();

			int i = 0;
			modulesProp.arraySize = query.Length;
			foreach (string path in query)
			{
				Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
				modulesProp.GetArrayElementAtIndex(i++).objectReferenceValue = asset;
			}

			modules = AssetDatabase.FindAssets("t:TimelineModuleType", locations);
			query = modules.Select(AssetDatabase.GUIDToAssetPath).ToArray();
			i = 0;
			modifiersProp.arraySize = query.Length;
			foreach (string path in query)
			{
				Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
				modifiersProp.GetArrayElementAtIndex(i++).objectReferenceValue = asset;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
