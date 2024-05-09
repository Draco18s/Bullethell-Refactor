using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Assets.draco18s.util
{
	[CustomPropertyDrawer(typeof(FloatRange))]
	public class FloatRangeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.LabelField(position, label);
			position.height = EditorGUIUtility.singleLineHeight;
			position.width = position.width / 2 - EditorGUIUtility.standardVerticalSpacing - EditorGUIUtility.labelWidth;
			position.x += EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing;
			SerializedProperty prop = property.FindPropertyRelative("min");
			prop.floatValue = EditorGUI.DelayedFloatField(position, GUIContent.none, prop.floatValue);
			position.x += position.width + EditorGUIUtility.standardVerticalSpacing;
			prop = property.FindPropertyRelative("max");
			prop.floatValue = EditorGUI.DelayedFloatField(position, GUIContent.none, prop.floatValue);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}
	}
}