using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.draco18s.bulletboss.pattern
{
	[CustomEditor(typeof(LoopModuleType))]
	public class LoopModuleTypeEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty rarity = serializedObject.FindProperty("rarity");
			SerializedProperty unique = serializedObject.FindProperty("unique");
			SerializedProperty description = serializedObject.FindProperty("description");
			SerializedProperty icon = serializedObject.FindProperty("icon");
			SerializedProperty preconfigured = serializedObject.FindProperty("preconfigured");
			SerializedProperty preconfiguredPattern = serializedObject.FindProperty("preconfiguredPattern");
			SerializedProperty maxObjects = serializedObject.FindProperty("maxObjects");
			SerializedProperty allowedMaxObjRange = serializedObject.FindProperty("allowedMaxObjValueRange");

			SerializedProperty iterations = serializedObject.FindProperty("iterations");
			SerializedProperty allowedRange = serializedObject.FindProperty("allowedValueRange");
			SerializedProperty pattern = serializedObject.FindProperty("pattern");

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.TextField("Name", serializedObject.targetObject.name);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.PropertyField(rarity);
			EditorGUILayout.PropertyField(unique);
			EditorGUILayout.PropertyField(description);
			EditorGUILayout.PropertyField(icon);
			EditorGUILayout.PropertyField(preconfigured);

			if (preconfigured.boolValue)
			{
				EditorGUILayout.PropertyField(iterations);

				EditorGUILayout.PropertyField(preconfiguredPattern);
				EditorGUILayout.PropertyField(maxObjects);
				if (preconfiguredPattern.boolValue)
				{
					EditorGUILayout.PropertyField(pattern);
				}
			}
			else
			{
				EditorGUILayout.PropertyField(allowedRange);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
