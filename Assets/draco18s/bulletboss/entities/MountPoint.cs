using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.bulletboss.ui;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.draco18s.bulletboss.entities
{
	public class MountPoint : Bullet
	{
		[SerializeField] private SpriteRenderer selectionFrame;
		[UsedImplicitly]
		void Start()
		{
			serializedPattern.DeserializeForRuntime();
			serializedPattern.InitOrReset(true);
			pattern = Timeline.CloneFrom(serializedPattern);
		}

		public override void DestroySelf()
		{
		}

		public void SetSelected(bool s)
		{
			selectionFrame.enabled = s;
		}

		[UsedImplicitly]
		private void OnMouseUpAsButton()
		{
			SetSelected(true);
			TimelineUI.instance.Select(pattern, this);
			pattern.InitOrReset();
		}

		public bool AddModifier(TimelineModifierType modifier)
		{
			return pattern.AddAIPlayerModifier(new Card(modifier));
		}
	}
}
