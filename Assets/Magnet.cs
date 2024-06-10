using System.Collections;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.util;
using UnityEngine;

public class Magnet : MonoBehaviour, IHasSpeed
{
	public float Speed => 0;
	private float radius = 1.5f;
	void FixedUpdate()
	{
		Collider2D c = Physics2D.OverlapCircle(transform.position, radius, LayerMask.GetMask("AIPlayer"));
		if (!c)
		{
			transform.Translate(Vector3.down * Time.deltaTime * 0.2f, Space.World);
			if (transform.localPosition.y < -3)
			{
				transform.localPosition = transform.localPosition.ReplaceY(4.5f);
			}
			return;
		}
		Vector3 off = c.transform.position - transform.position;
		Vector3 dir = (off).normalized;
		float mag = off.magnitude;
		float speed = 1.5f/(mag+0.5f); 
		transform.Translate(dir * Mathf.Min(Time.deltaTime * speed * speed, mag), Space.World);
	}
}
