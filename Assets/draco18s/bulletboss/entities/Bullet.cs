using System;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.util;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.draco18s.bulletboss.entities
{
	public class Bullet : MonoBehaviour, IHasSpeed
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

		[SerializeField, FormerlySerializedAs("mainWeapon")] protected Timeline serializedPattern;
		[SerializeField] protected float baseSpeed;
		[SerializeField] protected BulletShape shape;
		[SerializeField] protected BulletSize size;
		[SerializeField] protected Image sprite;
		protected Timeline pattern;

		public float speed { get; protected set; }
		protected Bullet parentShot;
		public BulletShape bulletShape => shape;
		public BulletSize bulletSize => size;

		public float Speed => speed;

		private void Start()
		{
			speed = baseSpeed;
			serializedPattern.InitOrReset();
			pattern ??= Timeline.CloneFrom(serializedPattern);
			pattern.SetOverrideDuration(20);
		}

		private void FixedUpdate()
		{
			if (GameManager.instance == null || GameManager.instance.gameState != GameManager.GameState.Combat && GameManager.instance.gameState != GameManager.GameState.Editing) return;
			pattern.RuntimeUpdate(this, Time.fixedDeltaTime);
			if (pattern == null) return;
			OnUpdate(Time.fixedDeltaTime);
			ChildUpdate();
		}

		protected virtual void ChildUpdate()
		{

		}

		/*private void Update()
		{
			if (pattern == null) return;
			OnUpdate(Time.deltaTime);
		}*/

		private Vector2Int lastWrite = new Vector2Int(-1000,-1000);

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
				DestroySelf();
				return;
			}


			transform.position = transform.position.ReplaceZ(transform.position.x / -50f + (parentShot != null ? 0.1f : 0));
			
			if (!GameManager.instance.doHeatmap) return;
			Vector2 texPosF = new Vector2(Mathf.Clamp(transform.position.x / 18, -0.5f, 0.5f), Mathf.Clamp(transform.position.y / 10, -0.5f, 0.5f));
			Vector2Int texPos = new Vector2Int(Mathf.RoundToInt((texPosF.x + 0.5f) * GameManager.instance.heatmap.width), Mathf.RoundToInt((texPosF.y + 0.5f) * GameManager.instance.heatmap.height));

			if (texPosF.x > 0.5) return;
			if (texPosF.y > 0.5) return;
			if (texPosF.x <= -0.5) return;
			if (texPosF.y <= -0.5) return;

			if (lastWrite == texPos)
			{
				return;
			}

			lastWrite = texPos;
			if (GameManager.instance.heatmap == null) return;

			Color c = Color.HSVToRGB(Mathf.Clamp01(1) - (1f/255), Mathf.Clamp01(1), Mathf.Clamp01(1));
			//if (h > 0)
			//	c.a = 0.5f + v * s * 0.5f;
			//else
			//	c.a = 0f;
			//texPos.ReplaceX(texPos.x + (int)Mathf.Sign(transform.up.x));
			//texPos.ReplaceY(texPos.y + (int)Mathf.Sign(transform.up.y));
			GameManager.instance.heatmap.SetPixel(texPos.x, texPos.y,c);
			GameManager.instance.heatmap.Apply();
			RenderTexture.active = null;
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
			pattern.ApplyModifiers(this);
		}

		public virtual void DestroySelf()
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
