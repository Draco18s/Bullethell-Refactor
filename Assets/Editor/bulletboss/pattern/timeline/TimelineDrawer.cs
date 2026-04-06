using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	[CustomPropertyDrawer(typeof(Timeline))]
	public class TimelineDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position = EditorGUI.IndentedRect(position);
			SerializedProperty editable = property.FindPropertyRelative("runtimeEditable");
			SerializedProperty duration = property.FindPropertyRelative("overrideDuration");
			SerializedProperty childLimit = property.FindPropertyRelative("overrideChildLimit");
			SerializedProperty upgradesProp = property.FindPropertyRelative("modifiers");
			SerializedProperty patternObjectsProp = property.FindPropertyRelative("patternObjects");

			position.height = EditorGUIUtility.singleLineHeight;
			property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
			if (property.isExpanded)
			{
				EditorGUI.indentLevel++;
				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.PropertyField(position, editable);
				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.PropertyField(position, duration);
				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.PropertyField(position, childLimit);
				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.PropertyField(position, upgradesProp);
				position.y += EditorGUI.GetPropertyHeight(upgradesProp) + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.PropertyField(position, patternObjectsProp);
				position.y += EditorGUI.GetPropertyHeight(patternObjectsProp) + EditorGUIUtility.standardVerticalSpacing;

				SerializedProperty timelineKeys = patternObjectsProp.FindPropertyRelative("keys");
				SerializedProperty timelineVals = patternObjectsProp.FindPropertyRelative("values");

				//position.y += EditorGUIUtility.singleLineHeight;

				Rect indentPos = EditorGUI.IndentedRect(position);

				if (EditorGUI.DropdownButton(indentPos, new GUIContent("Add Module"), FocusType.Keyboard))
				{
					GenericMenu menu = new GenericMenu();
					CardLibrary cl = GameObject.FindFirstObjectByType<CardLibrary>();
					foreach(Type v in cl.GetModules().Select(x => x.GetType()).Distinct())
					{
						menu.AddItem(new GUIContent(v.Name), false, (o) =>
						{
							Type t = (Type)o;
							ScriptableObject asset = ScriptableObject.CreateInstance(t);
							asset.name = t.Name;
							AssetDatabase.AddObjectToAsset(asset, property.serializedObject.targetObject);

							timelineKeys.arraySize++;
							timelineKeys.GetArrayElementAtIndex(timelineKeys.arraySize - 1).intValue = timelineKeys.GetArrayElementAtIndex(timelineKeys.arraySize - 2).intValue + 1;
							timelineVals.arraySize++;
							timelineVals.GetArrayElementAtIndex(timelineVals.arraySize - 1).objectReferenceValue = asset;
							
							timelineKeys.serializedObject.ApplyModifiedProperties();
							timelineVals.serializedObject.ApplyModifiedProperties();

							EditorUtility.SetDirty(asset);
							EditorUtility.SetDirty(property.serializedObject.targetObject);
							AssetDatabase.SaveAssets();
							AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
						}, v);
					}
					menu.ShowAsContext();
				}

				EditorGUI.indentLevel--;
			}

			property.serializedObject.ApplyModifiedProperties();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

			SerializedProperty editable = property.FindPropertyRelative("runtimeEditable");
			SerializedProperty duration = property.FindPropertyRelative("overrideDuration");
			SerializedProperty childLimit = property.FindPropertyRelative("overrideChildLimit");
			SerializedProperty upgradesProp = property.FindPropertyRelative("modifiers");
			SerializedProperty patternObjectsProp = property.FindPropertyRelative("patternObjects");

			return EditorGUI.GetPropertyHeight(editable)
			       + EditorGUI.GetPropertyHeight(duration)
			       + EditorGUI.GetPropertyHeight(childLimit)
			       + EditorGUI.GetPropertyHeight(upgradesProp)
			       + EditorGUI.GetPropertyHeight(patternObjectsProp)
				   + EditorGUIUtility.singleLineHeight*3.5f;
		}
	}
}
