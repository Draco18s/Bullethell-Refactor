using UnityEditor;
using UnityEngine;

namespace Assets.draco18s.util
{
	public class ResaveAll : Editor
	{
		[MenuItem("File/Force Save All", priority = 192)]
		private static void Resave()
		{
			string[] assets = AssetDatabase.FindAssets("", new string[] { "Assets" });

			Debug.Log($"Re-saving {assets.Length} assets");
			foreach (string guid in assets)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
				EditorUtility.SetDirty(asset);
			}

			AssetDatabase.SaveAssets();
		}
	}
}
