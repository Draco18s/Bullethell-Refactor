using System.Collections;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.util;
using UnityEngine;

public class Magnet : MonoBehaviour, IHasSpeed
{
	[SerializeField] private float _speed = 0.2f;
	[SerializeField] private bool _magnet = false;
	public float Speed => _speed;
	private float radius = 1.5f;

	void FixedUpdate()
	{
		if (Mathf.Abs(transform.localPosition.x) > 8.25f)
		{
			Vector3 dir1 = (transform.localPosition.ReplaceY(0).ReplaceZ(0)).normalized;
			transform.Translate(-dir1 * Mathf.Max(_speed, 0.1f) * Time.fixedDeltaTime, Space.World);
		}
		if (_magnet)
		{
			Collider2D c = Physics2D.OverlapCircle(transform.position, radius, LayerMask.GetMask("AIPlayer"));
			if (c)
			{

				Vector3 off = c.transform.position - transform.position;
				Vector3 dir = (off).normalized;
				float mag = off.magnitude;
				float speed = 1.5f / (mag + 0.5f);
				transform.Translate(dir * Mathf.Min(Time.fixedDeltaTime * speed * speed, mag), Space.World);
				return;
			}
		}
		transform.Translate(Vector3.down * Time.fixedDeltaTime * Speed, Space.World);
		if (transform.localPosition.y < -3)
		{
			transform.localPosition = transform.localPosition.ReplaceY(Random.value * 2f + 2).ReplaceX((Random.value * 18) - 8);
			//transform.parent.parent.GetComponentInChildren<PlayerAgent>().AddReward(-0.2f, "gem lost");
		}
	}
}