using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
