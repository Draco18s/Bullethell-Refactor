using System.Linq;
using Assets.draco18s.bulletboss;
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
			string basePath = "Assets/ScriptableObjects/";
			string patternPath = "Patterns/";
			string modififerPath = "Pattern Modifiers/";

			string[] rarityFolders = {
				"0 Starting",
				"1 Common",
				"2 Uncommon",
				"3 Rare",
				"4 Epic",
				"5 Ultra Rare",
				"6 Legendary",
				"7 Artifact",
			};
			string[] modules = AssetDatabase.FindAssets("t:PatternModuleType", rarityFolders.Select(f => $"{basePath}{patternPath}{f}").ToArray());

			string[] query = modules.Select(AssetDatabase.GUIDToAssetPath).ToArray();

			int i = 0;
			modulesProp.arraySize = query.Length;
			foreach (string path in query)
			{
				Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
				modulesProp.GetArrayElementAtIndex(i++).objectReferenceValue = asset;
			}

			modules = AssetDatabase.FindAssets("t:TimelineModifierType", rarityFolders.Select(f => $"{basePath}{modififerPath}{f}").ToArray());
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
