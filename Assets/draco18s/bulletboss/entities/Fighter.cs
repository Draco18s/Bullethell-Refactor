using System.Linq;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.pattern;
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
		public int reward { get; protected set; } = 0;

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
			pattern.SetEntityOwner(this);
		}

		public override void DestroySelf(bool ignorePenetrations=false)
		{
			if (!ignorePenetrations && currentHP > 0) return;
			Destroy(gameObject);
			this.enabled = false;
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

		public override bool CanAddModule(PatternModule refPattern)
		{
			if (refPattern.patternTypeData is SpawnModuleType)
			{
				return !pattern.GetModules().Any(p => p.Value.pattern.patternTypeData is SpawnModuleType);
			}
			return true;
		}

		public void SetData(FighterConfigType config)
		{
			GetComponentInChildren<SpriteRenderer>().sprite = config.sprite;
			currentHP = maximumHP = config.health;
			speed = config.speed * 0.2f;
			reward = config.gems;
			foreach (Timeline pat in config.weaponPatterns)
			{
				pat.DeserializeForRuntime();
				AddGun(pat);
			}
		}

		public void AddGun(Timeline data)
		{
			GameObject mount = Instantiate(GameAssets.mountPointPrefab, transform);
			mount.layer = gameObject.layer;
			mount.transform.localPosition = Vector3.zero;
			MountPoint b = mount.GetComponent<MountPoint>();
			data.DeserializeForRuntime();
			data.InitOrReset(true);
			b.SetPattern(data, true);
		}
	}
}
