using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	//[CustomPropertyDrawer(typeof(Timeline))]
	public class TimelineDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position = EditorGUI.IndentedRect(position);
			property.FindPropertyRelative("runtimeEditable").boolValue = false;
			SerializedProperty durationProp = property.FindPropertyRelative("duration");
			SerializedProperty upgradesProp = property.FindPropertyRelative("upgrades");
			SerializedProperty patternObjectsProp = property.FindPropertyRelative("patternObjects");

			position.height = EditorGUIUtility.singleLineHeight;
			property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
			if (property.isExpanded)
			{
				EditorGUI.indentLevel++;
				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.PropertyField(position, durationProp);
				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.PropertyField(position, upgradesProp);
				position.y += EditorGUI.GetPropertyHeight(upgradesProp) + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.PropertyField(position, patternObjectsProp);
				EditorGUI.indentLevel--;
			}

			property.serializedObject.ApplyModifiedProperties();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

			SerializedProperty durationProp = property.FindPropertyRelative("duration");
			SerializedProperty upgradesProp = property.FindPropertyRelative("upgrades");
			SerializedProperty patternObjectsProp = property.FindPropertyRelative("patternObjects");

			return EditorGUI.GetPropertyHeight(durationProp) + EditorGUI.GetPropertyHeight(upgradesProp) + EditorGUI.GetPropertyHeight(patternObjectsProp);
		}
	}
}
