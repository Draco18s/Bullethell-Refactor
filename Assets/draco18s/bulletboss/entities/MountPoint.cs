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

		public override void DestroySelf()
		{
		}

		[UsedImplicitly]
		private void OnMouseUpAsButton()
		{
			TimelineUI.instance.Select(pattern);
			pattern.InitOrReset();
		}
	}
}
