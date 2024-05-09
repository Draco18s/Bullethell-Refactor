using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.bulletboss.ui;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.draco18s.bulletboss.entities
{
	public class MountPoint : Bullet
	{
		void Start()
		{
			serializedPattern.DeserializeForRuntime();
			serializedPattern.InitOrReset(true);
			pattern = Timeline.CloneFrom(serializedPattern);
		}

		void Update()
		{
			pattern.RuntimeUpdate(this, Time.deltaTime);
		}

		[UsedImplicitly]
		private void OnMouseUpAsButton()
		{
			TimelineUI.instance.Select(pattern);
			pattern.InitOrReset();
		}
	}
}
