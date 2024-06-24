using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Assets.draco18s.serialization {
	public static class ScriptableObjectRegistry {
		private static Dictionary<string,object> REGISTRIES = new Dictionary<string,object>();
		private static bool locked;

		public static void AddRegistry(string type, ScriptableObject obj) {
			if(locked) return;
			REGISTRIES.Add(type,obj);
		}

		public static void AddRegistry(ScriptableObject obj) {
			string regName = obj.GetType().Name;
			if(obj is ScriptableGenerics genObj) {
				regName = genObj.Get().GetType().GenericTypeArguments[0].Name;
			}
			if(locked) return;
			REGISTRIES.Add(regName,obj);
		}

		public static void AddRegistry<U>(ScriptableObjectMap<U> obj) where U:ScriptableObject {
			if(locked) return;
			REGISTRIES.Add(obj.GetType().Name,obj);
		}

		public static T GetRegistry<T>() where T:ScriptableObject {
			if(REGISTRIES.TryGetValue(typeof(T).Name, out object r)) return (T)r;
			return null;
		}

		public static ScriptableObjectMap<U> GetRegistry2<U>() where U:ScriptableObject {
			if(REGISTRIES.TryGetValue(typeof(U).Name, out object r)) {
				ScriptableGenerics gen = (ScriptableGenerics)r;
				return (ScriptableObjectMap<U>)gen.Get();
			}
			return null;
		}

		public static void Lock() {
			locked = true;
			/*foreach(var o in REGISTRIES.Values) {
				if(o is ScriptableObject so)
					so.Invoke("OnEnable");
				if(o is ScriptableGenerics sg)
					sg.Get().
			}*/
		}
	}
}