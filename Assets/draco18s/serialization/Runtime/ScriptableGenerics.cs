using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.draco18s.serialization {
	public class ScriptableGenerics : ScriptableObject, ISerializationCallbackReceiver {
		[SerializeField] protected string _directory;
		[SerializeField] protected string _class;
		[SerializeField] protected string _data;
		[SerializeField] protected string _bundle;
		public System.Object obj;

		void Awake() {
			if(string.IsNullOrEmpty(_data)) return;
			string[] names = _data.Split(',');
			Type soType = obj.GetType();
			Type typeT = soType.GenericTypeArguments[0];
			IList theList = (IList)soType.GetField("objects", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
			foreach(string n in names) {
#if UNITY_EDITOR
				ScriptableObject so;
				if(Application.isPlaying) {
					AssetBundle bundle = AssetBundle.GetAllLoadedAssetBundles().First(x => x.name == _bundle);
					so = bundle.LoadAsset<ScriptableObject>(n);
				}
				else {
					so = AssetDatabase.LoadAssetAtPath(n,typeof(ScriptableObject)) as ScriptableObject;
				}
#else		
				AssetBundle bundle = AssetBundle.GetAllLoadedAssetBundles().First(x => x.name == _bundle);
				ScriptableObject so = bundle.LoadAsset<ScriptableObject>(n);
#endif
				if(so == null) continue;
				if(typeT != so.GetType()) continue;
				theList.Add(so);
			}
			soType.GetMethod("OnEnable").Invoke(obj, null);
		}
	
		public void Set<T>(T obj) where T : class {
			this.obj = obj;
		}
	
		public T Get<T>() where T : class {
			return (T)obj;
		}
	
		public void Set(System.Object obj) {
			this.obj = obj;
		}
	
		public System.Object Get(){
			return obj;
		}

		public void OnBeforeSerialize() {
#if UNITY_EDITOR
			if(obj != null) {
				_class = obj.GetType().AssemblyQualifiedName;
				List<string> dat = new List<string>();
				IEnumerable<ScriptableObject> enm = (IEnumerable<ScriptableObject>)obj;
				foreach(ScriptableObject so in enm) {
					dat.Add(AssetDatabase.GetAssetPath(so));
				}
				_data = string.Join(",",dat);
			}
#endif
		}

		public void OnAfterDeserialize() {
			if(string.IsNullOrEmpty(_class)) return;
			Type soType = Type.GetType(_class);
			obj = Activator.CreateInstance(soType);
		}
	}
}