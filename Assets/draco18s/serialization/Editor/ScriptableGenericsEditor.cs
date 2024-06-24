using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Linq;
using System.Reflection;
//using Assets.draco18s.util.Editor;

namespace Assets.draco18s.serialization
{
	[CustomEditor(typeof(ScriptableGenerics))]
	public class ScriptableGenericsEditor : Editor {
		IList theList;
		ReorderableList reordList;
		IEnumerable self;
		bool expanded = false;

		public void OnEnable() {
			self = (IEnumerable)((ScriptableGenerics)target).obj;
			if(self == null) {
				return;
			}
			if(self.GetType().GetGenericTypeDefinition() == typeof(ScriptableObjectMap<>)) {
				//Debug.Log(self.GetType());
				//Debug.Log(self.GetType().GetField("objects", BindingFlags.Instance | BindingFlags.NonPublic));

				theList = (IList)self.GetType().GetField("objects", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self);
				reordList = new ReorderableList(theList, self.GetType().GenericTypeArguments[0]) {
					drawHeaderCallback = rect => {
						EditorGUI.LabelField(rect, "Objects");
					},
					drawElementCallback = (rect, index, active, focused) => {
						ScriptableObject elem = (ScriptableObject)theList[index];
						EditorGUI.BeginChangeCheck();
						EditorGUI.indentLevel++;
						EditorGUI.ObjectField(rect, elem, typeof(ScriptableObject), false);
						if(EditorGUI.EndChangeCheck()) {
							serializedObject.ApplyModifiedProperties();
						}
						EditorGUI.indentLevel--;
					}
				};
			}
		}

		public override void OnInspectorGUI() {
			self = (IEnumerable)((ScriptableGenerics)target).obj;
			if(self == null) {
				return;
			}
			serializedObject.FindProperty("_bundle").stringValue = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(target)).assetBundleName;
			expanded = EditorGUILayout.Foldout(expanded, new GUIContent("Objects"));
			if(expanded) {
				reordList.DoLayoutList();
			}
			SerializedProperty folder = serializedObject.FindProperty("_directory");
			folder.stringValue = EditorGUILayout.DelayedTextField(folder.stringValue);
			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(folder.stringValue));
			/*if (GUILayout.Button("Auto Import")) {
				var f = ($"Scriptables/Crafting/{folder.stringValue}");
				ScriptableObject[] objects = EditorTypeExtensions.GetAtPath<ScriptableObject>(f);
				foreach(ScriptableObject r in objects) {
					theList.Add(r);
				}
				EditorUtility.SetDirty(target);
			}*/
			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndDisabledGroup();
			if (GUILayout.Button("Build Bundles")) {
				BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
			}
		}
	}
}
