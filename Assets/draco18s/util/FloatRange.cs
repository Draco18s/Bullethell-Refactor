﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public override string ToString()
		{
			return $"({min} — {max})";
		}
	}
}
