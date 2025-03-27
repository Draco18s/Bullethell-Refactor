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
		[SerializeField] private Transform gameContainer;
		[SerializeField] private bool drawDebug = false;
		private Vector3 oobPos = new Vector3(0, 0, -10);

		private Vector3 prevPosition = Vector3.zero;
		private const float perUpdateScore = 0.005f;

		private Dictionary<string, float> awards = new Dictionary<string, float>();
		private List<(Vector3, Vector3, Color)> debugLines = new List<(Vector3, Vector3, Color)>();
		[SerializeField] private float _speed = -1;

		private float goalDist = 0.3f;
		[SerializeField] private long episodes = 0;

		MovementDecider md;
		private float previousBestScore = -5_000;
		private Vector3 previousBestTarget = new Vector3(0, 0, -10);

		public void AddReward(float amt, string reason)
		{
			if (float.IsNaN(amt))
			{
				Debug.LogError($"NaN passed to add reward '{reason}'");
				return;
			}
			if(float.IsInfinity(amt))
			{
				Debug.LogError($"Infinity passed to add reward '{reason}'");
				return;
			}
			awards.TryAdd(reason, 0);
			if (Mathf.Abs(awards[reason]) > 50 /*|| Mathf.Abs(amt)*3000 > 500*/)
			{
				Debug.Log($"Holy shit, {reason} {amt} ({awards[reason]})");
				return;
			}
			awards[reason] += amt;
			AddReward(amt);
		}

		[UsedImplicitly]
		public override void OnEpisodeBegin()
		{
			if (md == null)
			{
				md = transform.GetComponentInChildren<MovementDecider>();
			}
			base.OnEpisodeBegin();
			_speed = transform.GetComponentInChildren<IHasSpeed>().Speed;
			if (drawDebug) Debug.Log($"Total: {awards.Select(kvp => kvp.Value).Sum():F3} | " + string.Join(", ", awards.OrderByDescending(kvp => Mathf.Abs(kvp.Value)).Select(kvp => $"{kvp.Key}: {kvp.Value:F3}")));
			debugLines.Clear();

			awards.Clear();
			if((episodes+1) % 10 == 0)
				goalDist = Mathf.Max(goalDist - .005f, 0.1f);
			prevPosition = oobPos/10;
			MaxStep = 3_000;
			previousBestScore = -5_000;
			previousBestTarget = oobPos;
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
			sensor.AddObservation(prevPosition.x);
			sensor.AddObservation(prevPosition.y);

			/*IEnumerable<MountPoint> mounts = gameObject.GetComponentsInChildren<MountPoint>().PadRight(10);
			foreach (MountPoint mount in mounts.Take(10))
			{
				if (mount == null)
				{
					sensor.AddObservation(Vector3.zero);
					continue;
				}
				sensor.AddObservation(mount.transform.right);
			}*/

			int l1 = LayerMask.NameToLayer("EnemyBullets");

			List<Collider2D> padObj = objs.Where(c => c.gameObject.layer == l1).ToList();

			IOrderedEnumerable<Collider2D> bul = padObj.PadRight(50).OrderBy(a => a == null ? 100_000 : Vector3.Distance(a.transform.localPosition.ReplaceZ(0), transform.localPosition.ReplaceZ(0)));

			//int q = 0;
			foreach (Collider2D b in bul.Take(35))
			{
				AddEnemyObservation(sensor, b);
				padObj.Remove(b);
			}

			for (int i = 0; i < 15; i++)
			{
				Collider2D b = padObj.GetRandom();
				AddEnemyObservation(sensor, b);
				padObj.Remove(b);
			}

			IOrderedEnumerable<Collider2D> colliders;
			/*l1 = LayerMask.NameToLayer("Enemy");

			padObj = objs.Where(c => c.gameObject.layer == l1).ToList();

			colliders = padObj.PadRight(25).OrderBy(a => a == null ? 100000 : Vector3.Distance(a.transform.localPosition.ReplaceZ(0), transform.localPosition.ReplaceZ(0)));

			//int q = 0;
			foreach (Collider2D b in colliders.Take(15))
			{
				AddEnemyObservation(sensor, b);
				padObj.Remove(b);
			}
			for (int i = 0; i < 10; i++)
			{
				Collider2D b = padObj.GetRandom();
				AddEnemyObservation(sensor, b);
				padObj.Remove(b);
			}*/

			int l2 = LayerMask.NameToLayer("Powerups");
			padObj = objs.Where(c => c.gameObject.layer == l2).ToList();
			colliders = padObj.OrderBy(a => a == null ? 100_000 : a.transform.localPosition.y);
			//foreach (Collider2D b in colliders.PadRight(5))
			colliders = padObj.PadRight(5).OrderBy(a => a == null ? 100_000 : Vector3.Distance(md.transform.position.ReplaceZ(0), transform.position.ReplaceZ(0)));
			foreach (Collider2D b in colliders)
			{
				if (b == null)
				{
					sensor.AddObservation(oobPos / 10f);
					continue;
				}

				Vector3 v = (b.transform.localPosition).ReplaceZ(1);
				sensor.AddObservation(v / 10f);
				padObj.Remove(b);
			}
		}

		private void AddPowerupObservation(VectorSensor sensor, Collider2D coll)
		{
			if (coll == null)
			{
				sensor.AddObservation(oobPos / 10f);
				return;
			}

			float colliderSize = 0.1f;
			if (coll is CircleCollider2D cir)
			{
				colliderSize = cir.radius;
			}

			Vector3 v = (coll.transform.localPosition - GetLocalPos()).ReplaceZ(coll.transform.localScale.x * colliderSize);

			sensor.AddObservation(v / 10f);
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

			float bSpeed = coll.GetComponent<IHasSpeed>().Speed;
			Vector3 v = (coll.transform.localPosition - GetLocalPos()).ReplaceZ(coll.transform.localScale.x * colliderSize);

			sensor.AddObservation(v / 10f);
			sensor.AddObservation(coll.transform.right * bSpeed / 10);
		}

		public override void OnActionReceived(ActionBuffers actionBuffers)
		{
			Vector3 controlSignal = Vector3.zero;
			controlSignal.x = actionBuffers.ContinuousActions[0];
			controlSignal.y = actionBuffers.ContinuousActions[1];
			Vector3 target = new Vector3(controlSignal.x * 10, controlSignal.y * 10, 0);

			var obs = GetObservations().Skip(2).ToArray();

			if (target.x < -8f || target.x > 8f || target.y < -2f || target.y > 2f)
			{
				target = new Vector3(Mathf.Clamp(target.x, -8.5f, 8.5f), Mathf.Clamp(target.y, -2.5f, 2.5f), target.z);
				//AddReward(-perUpdateScore * Vector3.Distance(target, ntarget), "Bounds");
				//target = ntarget;
				//controlSignal = (target - GetLocalPos())/10f;
			}

			Color col2 = Color.yellow;
			float s;
			s = 1 - Mathf.Clamp(Mathf.Abs(target.x - -8.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f, "-X edge");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.left * 1, (s < 0.001 ? Color.clear : (controlSignal.x < 0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(target.x - 8.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f, "+X edge");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.right * 1, (s < 0.001 ? Color.clear : (controlSignal.x > -0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(target.y - -2.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f, "-Y edge");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.down * 1, (s < 0.001 ? Color.clear : (controlSignal.y < 0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(target.y - 2.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f, "+Y edge");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.up * 1, (s < 0.001 ? Color.clear : (controlSignal.y > -0.05 ? Color.red : col2)), 0.02f);

			//target += GetLocalPos();

			/*List<Vector3> mounts = new List<Vector3>();
			for (int i = 0; i < 10 * 3; (i, j) = (i + 3, j + 3)) // 10 mount points
			{
				Vector3 vec = new Vector3(obs[i + 0], obs[i + 1], 0);
				if(vec != Vector3.zero)
					mounts.Add(vec);
			}*/
			//Debug.Log($"Mounts: {mounts.Count}");

			if (_speed == 0)
			{
				_speed = 3;
			}

			float score = CalcScore(obs, target + GetLocalPos(), true);
			previousBestScore = CalcScore(obs, previousBestTarget + GetLocalPos(), false);

			/*if (prevPosition.y > -1 && md.trainingTargetLocation.y > -10)
			{
				if (Vector3.Distance(md.transform.localPosition, md.trainingTargetLocation) < goalDist)
				{
					md.AddReward(1,"Arrive");
					md.EndEpisode();
					prevPosition = oobPos/10;
					//episodes += 1;
					previousBestScore = -5000;
					previousBestTarget = oobPos;
					return;
				}
				//return;
			}*/

			if (previousBestScore > score)
			{
				AddReward(-perUpdateScore * Vector3.Distance(prevPosition, controlSignal) * 5, "-s(prev)");
			}

			if (previousBestScore < score)
			{
				previousBestScore = score;
				md.targetLocation = target;
				previousBestTarget = target;
				if(previousBestScore > -1000)
					md.AddReward(perUpdateScore * 5, "Better");
				AddReward( perUpdateScore * Vector3.Distance(prevPosition, controlSignal), "+s(prev)");
			}
			prevPosition = (previousBestTarget)/10f;
		}

		private float CalcScore(float[] obs, Vector3 target, bool record)
		{
			if (obs.Length == 0) return 0;

			float m1 = Mathf.Clamp(PoissonReward1(Vector3.Distance(md.targetLocation, target)), -1, 1);
			float m2 = Mathf.Clamp(PoissonReward2(Vector3.Distance(target, md.transform.localPosition)), -1, 1);
			float a1 = -1;
			float a2 = 1;
			float b1 = 1_000;
			float b2 = 1_000;
			Vector3 nearestBulletCur = oobPos;
			Vector3 nearestBulletPred = oobPos;

			int j = 0;

			for (int i = 0; i < 50 * 6; (i, j) = (i + 6, j + 6)) // 50 bullets
			{
				Vector3 pos = new Vector3(obs[j + 0], obs[j + 1], obs[j + 2]) * 10f;
				if (Vector3.Distance(pos, oobPos) < 0.1f) continue;
				pos = pos.ReplaceZ(0);
				pos += GetLocalPos();
				Vector3 vel = new Vector3(obs[j + 3], obs[j + 4], 0) * 10f;

				Vector3 pred = pos + vel * Vector3.Distance(target, md.transform.localPosition) / (_speed / 2);

				Vector3 dif = pred - target;
				float df = dif.magnitude / 20;

				if (b1 > df)
				{
					nearestBulletPred = pred;
					nearestBulletCur = pos;
				}

				b1 = Mathf.Min(b1, df);
				b2 = Mathf.Min(b2, df);
			}

			b1 = Mathf.Min(b1, b2);

			if (b1 > 999)
				b1 = 0;
			else if(b1 < 1)
				b1 = -b1*5;
			if (b2 < 1)
				b2 = -b2 * 5;

			float m3 = 0;
			float m4 = 0;
			float m5 = 1_000;
			for (int i = 0; i < 5 * 3; (i, j) = (i + 3, j + 3)) // 5 gems
			{
				Vector3 pos = new Vector3(obs[j + 0], obs[j + 1], obs[j + 2]) * 10f;
				if (Vector3.Distance(pos, oobPos) < 0.1f) continue;
				pos = pos.ReplaceZ(0);
				m4++;
				pos += GetLocalPos();
				float d = Vector3.Distance(pos, target);
				m3 += Mathf.Clamp(Mathf.Sqrt(d / 20), 0, 0.6f);
				m5 = Mathf.Min(m5, d / 20);

				Color col = new Color(Mathf.Clamp01(d * d / 100), 1 - Mathf.Clamp01(d * d / 100), 0);
				if (drawDebug && record)  Debug.DrawLine(pos, target, col, 0.02f);
			}

			if (m4 > 0)
			{
				m3 /= m4;
			}
			else
			{
				float d = Vector3.Distance(target, Vector3.zero + GetLocalPos());
				m3 = Mathf.Clamp(Mathf.Sqrt(d / 20), 0, 0.6f);
				m5 = Mathf.Clamp(d / 20, 0, 1);
			}

			float rm3 = m3;
			float rm5 = m5;

			m3 = Mathf.Pow(m3, 2.5f);
			m5 = Mathf.Pow(m5, 2.5f);

			if (drawDebug && record)
			{
				Color col;// = new Color(Mathf.Clamp01(m1 / 2f + 0.5f), 1 - Mathf.Clamp01(m1 / 2f + 0.5f), 0);
						  //Debug.DrawLine(target, md.targetLocation, col, 0.02f);

				//col = new Color(b1/20, 0, 0);
				//Debug.DrawLine(target + Vector3.left * .25f, target + Vector3.right * .25f, col, 0.02f);
				//Debug.DrawLine(target + Vector3.up * .25f, target + Vector3.down * .25f, col, 0.02f);

				//col = new Color(0, 0, Mathf.Clamp01(m2 * 10 + 0.5f));
				//Debug.DrawLine(target, md.transform.position, col, 0.02f);
				col = new Color(Mathf.Clamp01(rm5), 1 - Mathf.Clamp01(rm3), Mathf.Clamp01(b1));
				DrawCircle(target, 18, 0.15f, col);
			}

			if (drawDebug && !record)
			{
				//DrawCircle(md.targetLocation, 18, goalDist, Color.white);
				//DrawCircle(md.TrainingTargetLocation + GetLocalPos(), 18, goalDist, Color.magenta);
			}

			if (record)
			{
				//AddReward( perUpdateScore * m1 * 1, "delta-p Targ");
				//AddReward( perUpdateScore * m2 * 1, "s(Targ)");
				AddReward(-perUpdateScore * m3 * 0.05f, "s(Gems)");
				AddReward(-perUpdateScore * m5 * 0.1f, "min(sGems)");
				//AddReward( perUpdateScore * a1 * 1, "angle Targ");
				//AddReward( perUpdateScore * a2 * 2, "cumm angle");
				AddReward( perUpdateScore * Mathf.Abs(b1) * b1 * 10f, "s(Bullet)");
			}

			return (-perUpdateScore * m3 * 0.1f) + (-perUpdateScore * m5 * 0.5f) + (-perUpdateScore * b1 * 0.01f);
		}

		private Vector3 GetLocalPos()
		{
			return transform.position;
		}

		private void DrawCircle(Vector3 point, int seg, float rad, Color col)
		{
			float angle = 2 * Mathf.PI / seg;
			Vector3 last = new Vector3(1, 0, 0);
			for (float a = 0; a < 360; a += angle)
			{
				Vector3 p1 = new Vector3(Mathf.Cos(a), Mathf.Sin(a));
				//debugLines.Add((last*rad+point,p*rad+point,col));
				Debug.DrawLine(last * rad + point, p1 * rad + point, col, 0.02f);
				last = p1;
			}
		}


		//0 to 1.5    - positive
		//1.5 to 4.25 - negative
		//4.25 to ~   - positive
		private static float PoissonReward1(float x)
		{
			//(1.24468f * (x / 3f) / Factorial(x / 3f))) * (1.9f / 3)
			return -(Mathf.Pow(0.7f, x) - (0.622338f*x/ Factorial(x / 3f)) - Mathf.Min(x/40, x+15f/80) + 1f / 3f);
		}

		//0 to 1 - negative
		//1 to 4 - positive
		//4 to ~ - negative
		private static float PoissonReward2(float x)
		{
			return ((1.86702f * (x - 0.65f) / Factorial(x - 0.65f)) - Mathf.Pow(0.5f, x + 1.35f)) * 2 / 2.9f - (1f / 3f);
		}

		private static float Factorial(float x)
		{
			if (Mathf.Approximately(x, 0))
				return 1;
			float r = x;
			r *= Gamma(x);
			return r;
		}

		private static readonly int g = 7;
		private static readonly double[] p = {0.99999999999980993, 676.5203681218851, -1_259.1392167224028,
			771.32342877765313, -176.61502916214059, 12.507343278686905,
			-0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7};
		private static float Gamma(float z)
		{
			if (z < 0.5)
				return Mathf.PI / (Mathf.Sin(Mathf.PI * z) * Gamma(1 - z));
			z -= 1;
			double x = p[0];
			for (var i = 1; i < g + 2; i++)
				x += p[i] / (z + i);
			float t = z + g + 0.5f;
			return (float)(Mathf.Sqrt(2 * Mathf.PI) * (Mathf.Pow(t, z + 0.5f)) * Mathf.Exp(-t) * x);
		}

		public void SetContainer(Transform bulletContainer)
		{
			gameContainer = bulletContainer;
		}
	}
}
