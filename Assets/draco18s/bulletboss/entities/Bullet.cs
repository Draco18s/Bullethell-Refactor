using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.util;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss.entities
{
	public class Bullet : MonoBehaviour
	{
		public enum BulletShape
		{
			Circle,
			Wide,
			Long,
		}
		public enum BulletSize
		{
			Small,
			Medium,
			Large,
		}

		[SerializeField] protected Timeline serializedPattern;
		[SerializeField] protected float baseSpeed;
		[SerializeField] protected BulletShape shape;
		[SerializeField] protected BulletSize size;
		[SerializeField] protected Image sprite;
		protected Timeline pattern;

		public float speed { get; protected set; }
		protected Bullet parentShot;
		public BulletShape bulletShape => shape;
		public BulletSize bulletSize => size;

		private void Start()
		{
			speed = baseSpeed;
			serializedPattern.InitOrReset();
			pattern ??= Timeline.CloneFrom(serializedPattern);
			pattern.SetOverrideDuration(20);
		}
		
		private void Update()
		{
			if (pattern == null) return;
			OnUpdate(Time.deltaTime);
		}

		private void OnUpdate(float dt)
		{
			bool kill = pattern.RuntimeUpdate(this, dt);
			transform.Translate(Vector3.right * speed * dt, Space.Self);
			if (parentShot != null)
			{
				transform.Translate(parentShot.transform.right * parentShot.speed * dt, Space.World);
			}

			if (Mathf.Abs(transform.position.x) > 10 || Mathf.Abs(transform.position.y) > 6.5f)
			{
				kill = true;
			}
			
			if (kill)
			{
				Destroy(gameObject);
			}
			transform.position = transform.position.ReplaceZ(transform.position.x / -50f + (parentShot != null?0.1f:0));
		}

		public void ChangeSpeed(float val)
		{
			speed = val;
		}

		public void ChangeScale(float val)
		{
			transform.localScale = new Vector3(val, val, val);
		}

		public void ChangeRotation(float val)
		{
			transform.localEulerAngles = transform.localEulerAngles.ReplaceZ(val);
		}

		public void SetParent(Bullet bullet)
		{
			parentShot = bullet;
		}

		public void SetPattern(Timeline p)
		{
			pattern = Timeline.CloneFrom(p);
			pattern.InitOrReset(false);
			pattern.ResetForNewLoopIteration();
		}

		public void DestroySelf()
		{
			Destroy(gameObject);
		}

		public int GetTargetLayerMask()
		{
			return LayerMask.GetMask(gameObject.layer == LayerMask.NameToLayer("PlayerBullets") ? "Enemy" : "AIPlayer");
		}

		public void SetSprite(Sprite newIcon)
		{
			sprite.sprite = newIcon;
		}
	}
}
