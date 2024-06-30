using System;

namespace Assets.draco18s.util
{
	[Serializable]
	public struct FloatRange
	{
		public float min;
		public float max;
		public float Range => max - min;

		public FloatRange(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public float GetValue()
		{
			return UnityEngine.Random.Range(min, max + 1);
		}

		public override string ToString()
		{
			return $"({min} — {max})";
		}
	}
}
