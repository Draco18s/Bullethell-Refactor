using UnityEditor;

namespace Assets.draco18s.bulletboss.pattern
{
	[CustomEditor(typeof(ChangeModuleType))]
	public class ChangeModuleInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty rarity = serializedObject.FindProperty("_rarity");
			SerializedProperty unique = serializedObject.FindProperty("_unique");
			SerializedProperty killOnComplete = serializedObject.FindProperty("_killOnComplete");
			SerializedProperty description = serializedObject.FindProperty("_description");
			SerializedProperty icon = serializedObject.FindProperty("_icon");
			SerializedProperty preconfigured = serializedObject.FindProperty("_preconfigured");

			SerializedProperty changeType = serializedObject.FindProperty("changeType");
			SerializedProperty allowedRange = serializedObject.FindProperty("allowedValueRange");
			SerializedProperty newValue = serializedObject.FindProperty("newValue");
			SerializedProperty randomRange = serializedObject.FindProperty("randomRange");
			SerializedProperty changeDuration = serializedObject.FindProperty("changeDuration");
			SerializedProperty allowedDurationRange = serializedObject.FindProperty("allowedDurationRange");

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.TextField("Name", serializedObject.targetObject.name);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.PropertyField(rarity);
			EditorGUILayout.PropertyField(unique);
			EditorGUILayout.PropertyField(description);
			EditorGUILayout.PropertyField(icon);
			EditorGUILayout.PropertyField(changeType);
			EditorGUILayout.PropertyField(preconfigured);
			EditorGUILayout.PropertyField(killOnComplete);

			if (preconfigured.boolValue)
			{
				EditorGUILayout.PropertyField(newValue);
				EditorGUILayout.PropertyField(randomRange);
				EditorGUILayout.PropertyField(changeDuration);
			}
			else
			{
				EditorGUILayout.PropertyField(allowedRange);
				EditorGUILayout.PropertyField(allowedDurationRange);
			}
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}
