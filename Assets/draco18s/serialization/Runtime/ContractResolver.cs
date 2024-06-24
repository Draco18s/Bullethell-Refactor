using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Assets.draco18s.serialization {
	public class ContractResolver : DefaultContractResolver {
		public static JsonSerializerSettings jsonSettings;

		public override JsonContract ResolveContract(System.Type type) {
			JsonContract contract = base.ResolveContract(type);
			if(typeof(Vector2Int).IsAssignableFrom(type)) {
				contract.Converter = new Vector2IntConverter();
				return contract;
			}
			if(typeof(Vector3Int).IsAssignableFrom(type)) {
				contract.Converter = new Vector3IntConverter();
				return contract;
			}
			if(typeof(Vector2).IsAssignableFrom(type)) {
				contract.Converter = new Vector3Converter();
				return contract;
			}
			if(typeof(Vector3).IsAssignableFrom(type)) {
				contract.Converter = new Vector3Converter();
				return contract;
			}
			foreach (System.Attribute attr in System.Attribute.GetCustomAttributes(type))
			{
				if (!(attr is JsonResolver)) continue;
				JsonResolver a = (JsonResolver)attr;
				contract.Converter = (JsonConverter)Activator.CreateInstance(a.converter);
				return contract;
			}

			if (type.Namespace?.Contains("draco18s") ?? false)
			{
				Debug.LogWarning($"No converter found for {type.Name}");
			}
			return contract;
		}

		protected override IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization) {
			IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

			foreach(JsonProperty prop in properties) {
				prop.DefaultValueHandling = DefaultValueHandling.Ignore;
			}

			return properties;
		}
	}
}