using Assets.draco18s.util;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Assets.draco18s.bulletboss.entities
{
	public class TargetDecider : Agent
	{
		[SerializeField] private Player playerShip;
		[SerializeField] private GameObject gemPrefab;
		[SerializeField] private GameObject bulletPrefab;
		[SerializeField] private Transform gameContainer;
		[SerializeField] private bool drawDebug = false;
		[SerializeField] private bool deterministicDebug = false;
		[SerializeField] private bool doMovement = true;
		[SerializeField] private long totalSteps = 0;

		private Vector3 oobPos = new Vector3(0, 0, -10);

		private Vector2 prevMove = Vector2.zero;
		private const float perUpdateScore = 0.005f;

		private Dictionary<string, float> awards = new Dictionary<string, float>();
		private List<(Vector3, Vector3, Color)> debugLines = new List<(Vector3, Vector3, Color)>();

		/*public void AddReward(float amt, string reason)
		{
			AddReward(amt);
			awards.TryAdd(reason, 0);
			awards[reason] += amt;
		}*/

		[UsedImplicitly]
		public override void OnEpisodeBegin()
		{
			base.OnEpisodeBegin();
			if (drawDebug) Debug.Log($"Total: {awards.Select(kvp => kvp.Value).Sum():F3} | " + string.Join(", ", awards.OrderByDescending(kvp => Mathf.Abs(kvp.Value)).Select(kvp => $"{kvp.Key}: {kvp.Value:F3}")));
			debugLines.Clear();

			awards.Clear();
			
			prevMove = Vector2.zero;
		}

		public override void CollectObservations(VectorSensor sensor)
		{
			List<Collider2D> objs = new List<Collider2D>();
			foreach (Transform t in gameContainer)
			{
				Collider2D c = t.GetComponent<Collider2D>();
				if (c == null) continue;
				objs.Add(c);
			}
			sensor.AddObservation(prevMove.x);
			sensor.AddObservation(prevMove.y);

			int l1 = LayerMask.NameToLayer("Enemy");

			List<Collider2D> padObj = objs.Where(c => c.gameObject.layer == l1).ToList();

			IOrderedEnumerable<Collider2D> colliders = padObj.PadRight(Mathf.Max(padObj.Count, 5)).OrderBy(a => a == null ? 100000 : Vector3.Distance(a.transform.localPosition.ReplaceZ(0), transform.localPosition.ReplaceZ(0)));

			//int q = 0;
			foreach (Collider2D b in colliders.Take(3))
			{
				AddEnemyObservation(sensor, b);
				padObj.Remove(b);
			}
			for (int i = 0; i < 2; i++)
			{
				Collider2D b = padObj.GetRandom();
				AddEnemyObservation(sensor, b);
				padObj.Remove(b);
			}

			int l2 = LayerMask.NameToLayer("Powerups");
			padObj = objs.Where(c => c.gameObject.layer == l2).ToList();
			colliders = padObj.PadRight(Mathf.Max(padObj.Count, 3)).OrderBy(a => a == null ? 100000 : Vector3.Distance(a.transform.position.ReplaceZ(0), transform.position.ReplaceZ(0)));
			foreach (Collider2D b in colliders.Take(3))
			{
				if (b == null)
				{
					sensor.AddObservation(oobPos / 10f);
					continue;
				}

				Vector3 v = (b.transform.localPosition - transform.localPosition).ReplaceZ(1);
				sensor.AddObservation(v / 10f);
				padObj.Remove(b);
			}
		}

		private void AddEnemyObservation(VectorSensor sensor, Collider2D coll)
		{
			if (coll == null)
			{
				sensor.AddObservation(oobPos / 10f);
				sensor.AddObservation(Vector3.zero);
				return;
			}

			float colliderSize = 0.1f;
			if (coll is CircleCollider2D cir)
			{
				colliderSize = cir.radius;
			}

			Vector3 v = (coll.transform.localPosition - transform.localPosition).ReplaceZ(coll.transform.localScale.x * colliderSize);

			float bSpeed = coll.GetComponent<IHasSpeed>().Speed;
			sensor.AddObservation(v / 10f);
			sensor.AddObservation(coll.transform.right * bSpeed * Time.fixedDeltaTime);
		}

		public override void OnActionReceived(ActionBuffers actionBuffers)
		{
			if (deterministicDebug)
				foreach ((Vector3 a, Vector3 b, Color c) in debugLines)
				{
					Debug.DrawLine(a, b, c, 0.02f);
				}
			
		}
	}
}
