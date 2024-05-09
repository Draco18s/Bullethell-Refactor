using System;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.util;
using UnityEngine;

namespace Assets.draco18s.bulletboss.entities
{
	public class Bullet : MonoBehaviour
	{
		[SerializeField] protected Timeline serializedPattern;
		[SerializeField] protected float baseSpeed;
		protected Timeline pattern;

		public float speed { get; protected set; }
		protected Bullet parentShot;

		private void Start()
		{
			speed = baseSpeed;
			serializedPattern.InitOrReset();
			pattern ??= Timeline.CloneFrom(serializedPattern);
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
				transform.Translate(parentShot.transform.forward * parentShot.speed * dt, Space.Self); 
			}
			
			if (kill)
			{
				Destroy(gameObject);
			}
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
			Debug.Log($"SetPattern: {p?.GetModules()?.Count ?? -1}");
			pattern = Timeline.CloneFrom(p);
			pattern.InitOrReset(false);
			pattern.ResetForNewLoopIteration();
		}

		public void DestroySelf()
		{
			Destroy(gameObject);
		}
	}
}
