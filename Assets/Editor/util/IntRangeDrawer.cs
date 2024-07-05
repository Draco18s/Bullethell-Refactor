using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Assets.draco18s.util
{
	[CustomPropertyDrawer(typeof(IntRange))]
	public class IntRangeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.LabelField(position, label);
			position.height = EditorGUIUtility.singleLineHeight;
			position.width = position.width / 2 - EditorGUIUtility.standardVerticalSpacing - EditorGUIUtility.labelWidth;
			position.x += EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing;
			SerializedProperty prop = property.FindPropertyRelative("min");
			prop.intValue = EditorGUI.DelayedIntField(position, GUIContent.none, prop.intValue);
			position.x += position.width + EditorGUIUtility.standardVerticalSpacing;
			prop = property.FindPropertyRelative("max");
			prop.intValue = EditorGUI.DelayedIntField(position, GUIContent.none, prop.intValue);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}
	}
}