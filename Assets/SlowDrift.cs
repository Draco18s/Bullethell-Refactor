using System.Collections;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.util;
using UnityEngine;

public class SlowDrift : MonoBehaviour, IHasSpeed
{
	bool hasInit = false;
	public float Speed => speed;
	private float speed = 0;
	private void Init()
	{
		hasInit = true;
		if (Random.value < 0.1f)
		{
			return;
		}

		speed = Random.value/4 + 0.25f;
		transform.RotateAround(transform.position, Vector3.forward, Mathf.Floor(Random.value*24) * 15);
	}
    void FixedUpdate()
    {
	    if (!hasInit)
	    {
		    Init();
	    }
		if(speed < 0.001) return;
        transform.Translate(Vector3.right * Time.deltaTime * speed, Space.Self);
        if (Mathf.Abs(transform.localPosition.x) > 9f)
	        transform.localPosition = new Vector3(-transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
		if (transform.localPosition.y < -5 || transform.localPosition.y > 5)
			transform.localPosition = new Vector3(transform.localPosition.x, -transform.localPosition.y, transform.localPosition.z);

		transform.localPosition = transform.localPosition.ReplaceX(Mathf.Clamp(transform.localPosition.x, -9, 9)).ReplaceY(Mathf.Clamp(transform.localPosition.y, -5, 5));
    }

	public void SpecialSetup(float rot, float speed)
	{
		this.speed = speed;
		transform.RotateAround(transform.position, Vector3.forward, Mathf.Floor(rot / 15) * 15);
		hasInit = true;
	}
}
