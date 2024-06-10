using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.draco18s.bulletboss.pattern.timeline;
using UnityEngine;

namespace Assets.draco18s.bulletboss.entities
{
	public class Player : MountPoint
	{
		public int currentHP { get; protected set; }
		public int maximumHP { get; protected set; }


		protected override void ChildUpdate()
		{

		}
	}
}
