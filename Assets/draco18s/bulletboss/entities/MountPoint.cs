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

		private void Update()
		{
			if (GameManager.instance.gameState != GameManager.GameState.Combat && GameManager.instance.gameState != GameManager.GameState.Editing) return;
			pattern.RuntimeUpdate(this, Time.deltaTime);
			ChildUpdate();
		}

		protected virtual void ChildUpdate()
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
