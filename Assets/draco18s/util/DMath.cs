using UnityEngine;

namespace Assets.draco18s.util
{
	public static class DMath
	{
		public static float Step(float val, float stepSize)
		{
			return Mathf.RoundToInt(val / stepSize) * stepSize;
		}
	}
}
