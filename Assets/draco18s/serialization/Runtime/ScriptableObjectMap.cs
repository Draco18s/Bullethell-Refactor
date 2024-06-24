using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Linq;

namespace Assets.draco18s.serialization
{
	public class ScriptableObjectMap<T> : IEnumerable<T> where T:ScriptableObject
	{
		[SerializeField] protected List<T> objects = new List<T>();

		protected Dictionary<string,T> map;

		public void OnEnable() {
			map = objects.GroupBy(x => x.name).ToDictionary(x => x.Key, x => x.First());
		}

		public T this[string name] => Get(name);

		public T Get(string name) {
			if(map == null) {
				OnEnable();
			}
			if(map.ContainsKey(name))
				return map[name];
			return null;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return objects.GetEnumerator();
		}

		// Must also implement IEnumerable.GetEnumerator, but implement as a private method.
		private IEnumerator GetEnumerator1()
		{
			return this.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator1();
		}
	}
}
