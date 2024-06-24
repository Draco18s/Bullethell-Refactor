using UnityEditor;
using UnityEngine;

namespace Assets.draco18s.serialization
{
	[CustomPropertyDrawer(typeof(ScriptableObjectMap<>))]
	public class ScriptableObjectMapDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.LabelField(position, new GUIContent("Here"));
		}
	}
}
