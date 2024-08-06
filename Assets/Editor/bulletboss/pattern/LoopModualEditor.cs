using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.draco18s.bulletboss.pattern
{
	[CustomEditor(typeof(LoopModuleType))]
	public class LoopModuleTypeInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty rarity = serializedObject.FindProperty("_rarity");
			SerializedProperty unique = serializedObject.FindProperty("_unique");
			SerializedProperty description = serializedObject.FindProperty("_description");
			SerializedProperty icon = serializedObject.FindProperty("_icon");
			SerializedProperty playableCopies = serializedObject.FindProperty("_playableCopies");
			SerializedProperty preconfigured = serializedObject.FindProperty("_preconfigured");

			SerializedProperty allowedChildren = serializedObject.FindProperty("_allowedChildrenClasses");
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
			EditorGUILayout.PropertyField(allowedChildren);
			EditorGUILayout.PropertyField(unique);
			EditorGUILayout.PropertyField(description);
			EditorGUILayout.PropertyField(icon);
			EditorGUILayout.PropertyField(playableCopies);
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
