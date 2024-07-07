using UnityEditor;
using UnityEngine;

namespace Assets.draco18s.util
{
	public class ForceSaveAll : Editor
	{
		[MenuItem("File/Force Save Assets", priority = 192)]
		private static void ReSave()
		{
			string[] assets = AssetDatabase.FindAssets("", new[] { "Assets" });

			Debug.Log($"Re-saving {assets.Length} assets");
			foreach (string guid in assets)
			{
				if (string.IsNullOrEmpty(guid))
					continue;

				Object asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
				if (asset == null)
					continue;

				// Skips attempting to save assets such as implementations of `UnityEditor.ScriptableSingleton<T>`.
				if ((asset.hideFlags & (HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor)) != 0)
					continue;

				EditorUtility.SetDirty(asset);
			}

			AssetDatabase.SaveAssets();
		}
	}
}
