using System;

namespace Assets.draco18s.util
{
	[Serializable]
	public struct IntRange
	{
		public int min;
		public int max;

		public int GetValue()
		{
			return UnityEngine.Random.Range(min, max + 1);
		}

		public override string ToString()
		{
			return $"({min} — {max})";
		}
	}
}
