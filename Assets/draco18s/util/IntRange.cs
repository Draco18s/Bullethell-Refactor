using System;

namespace Assets.draco18s.util
{
	[Serializable]
	public struct IntRange
	{
		public static readonly IntRange Zero = new IntRange(0,0);

		public int min;
		public int max;
		public float Range => max - min;

		public IntRange(int min, int max)
		{
			this.min = min;
			this.max = max;
		}

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
