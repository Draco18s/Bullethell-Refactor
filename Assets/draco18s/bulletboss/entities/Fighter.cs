using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.bulletboss.ui;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.draco18s.bulletboss.entities
{
	public class Fighter : Bullet
	{
		public int maximumHP { get; protected set; } = 1;
		public int currentHP { get; protected set; } = 1;

		[UsedImplicitly]
		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("PlayerBullets"))
			{
				Bullet b = other.GetComponent<Bullet>();
				currentHP -= b.Damage;
				b.DoOnDamageEffects();
				b.DestroySelf();
			}
		}

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

		[UsedImplicitly]
		private void OnMouseUpAsButton()
		{
			TimelineUI.instance.Select(pattern);
			pattern.InitOrReset();
		}

		public bool AddModifier(TimelineModifierType modifier)
		{
			return pattern.AddAIPlayerModifier(new Card(modifier));
		}
	}
}
