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
			if (pattern != null) return;
			serializedPattern.DeserializeForRuntime();
			serializedPattern.InitOrReset(true);
			pattern = Timeline.CloneFrom(serializedPattern);
			pattern.SetEntityOwner(this);
			pattern.SetMaxChildren(1);
		}

		public override void DestroySelf(bool ignorePenetration=false)
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
