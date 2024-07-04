using System;

namespace Assets.draco18s.serialization {
	/// <summary>
	/// Attribute to mark a class to use a given JsonConverter implementation
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
	public class JsonResolver : Attribute {
		public Type converter;

		/// <summary>
		/// The JsonConverter to use
		/// </summary>
		/// <param name="converter"></param>
		public JsonResolver(Type converter) {
			this.converter = converter;
		}
	}
}