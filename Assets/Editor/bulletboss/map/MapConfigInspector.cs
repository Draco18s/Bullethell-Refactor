using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Assets.draco18s.bulletboss.map
{
	[CustomEditor(typeof(MapConfig))]
	public class MapConfigInspector : Editor
	{
		private ReorderableList listView;
		private SerializedProperty layersProp;
		private void OnEnable()
		{
			layersProp = serializedObject.FindProperty(nameof(MapConfig.layers));
			listView = new ReorderableList(serializedObject, layersProp)
			{
				drawElementCallback = (rect, index, _, _) =>
				{
					rect.y -= 1;
					rect.x += 8;
					rect.width -= 8;
					SerializedProperty elem = layersProp.GetArrayElementAtIndex(index);
					SerializedProperty nodeTypeProp = elem.FindPropertyRelative(nameof(MapLayer.nodeType));
					int eInd = nodeTypeProp.enumValueIndex;
					rect.height = EditorGUIUtility.singleLineHeight;
					elem.isExpanded = EditorGUI.Foldout(rect, elem.isExpanded, ((MapNodeType)eInd).ToString());
					if (!elem.isExpanded) return;
					rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					EditorGUI.PropertyField(rect, nodeTypeProp);
					rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					EditorGUI.PropertyField(rect, elem.FindPropertyRelative(nameof(MapLayer.distanceFromPreviousLayer)));
					rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					EditorGUI.PropertyField(rect, elem.FindPropertyRelative(nameof(MapLayer.nodesApartDistance)));
					rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					EditorGUI.PropertyField(rect, elem.FindPropertyRelative(nameof(MapLayer.randomizePosition)));
					rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					EditorGUI.PropertyField(rect, elem.FindPropertyRelative(nameof(MapLayer.randomizeNodes)));
				},
				elementHeightCallback = index => EditorGUI.GetPropertyHeight(layersProp.GetArrayElementAtIndex(index))
			};
		}

		public override void OnInspectorGUI()
		{
			SerializedProperty blueprintsProp = serializedObject.FindProperty(nameof(MapConfig.nodeBlueprints));
			EditorGUILayout.PropertyField(blueprintsProp);
			if (GUILayout.Button("Find all Blueprints"))
			{
				string[] modules = AssetDatabase.FindAssets("t:LocationType");
				string[] query = modules.Select(AssetDatabase.GUIDToAssetPath).ToArray();
				blueprintsProp.arraySize = query.Length;
				for (int i = 0; i < query.Length; i++)
				{
					Object asset = AssetDatabase.LoadAssetAtPath(query[i], typeof(ScriptableObject));
					blueprintsProp.GetArrayElementAtIndex(i).objectReferenceValue = asset;
				}
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(MapConfig.numOfPreBossNodes)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(MapConfig.numOfStartingNodes)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(MapConfig.extraPaths)));
			listView.DoLayoutList();
		}
	}
}
