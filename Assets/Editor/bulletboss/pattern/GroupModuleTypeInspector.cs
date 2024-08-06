using UnityEditor;

namespace Assets.draco18s.bulletboss.pattern
{
	[CustomEditor(typeof(PatternGroupModuleType), true)]
	public class GroupModuleTypeInspector : Editor
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
				EditorGUILayout.PropertyField(preconfiguredPattern);
				if (preconfiguredPattern.boolValue)
				{
					EditorGUILayout.PropertyField(pattern);
				}
				else
				{
					EditorGUILayout.PropertyField(maxObjects);
				}
			}
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}
