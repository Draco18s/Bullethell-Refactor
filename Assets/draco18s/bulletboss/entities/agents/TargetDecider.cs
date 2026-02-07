using Assets.draco18s.util;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Assets.draco18s.bulletboss.entities
{
	public class TargetDecider : Agent
	{
		[SerializeField] private Transform gameContainer;
		[SerializeField] private bool drawDebug = false;
		private Vector3 oobPos = new Vector3(0, 0, -10);

		[SerializeField] private float rewardRatio = 2.25f;
		private Vector3 prevPosition = Vector3.zero;
		private const float perUpdateScore = 0.005f;

		private Dictionary<string, float> awards = new Dictionary<string, float>();
		private List<(Vector3, Vector3, Color)> debugLines = new List<(Vector3, Vector3, Color)>();
		[SerializeField] private float _speed = -1;

		private float goalDist = 0.3f;
		[SerializeField] private long episodes = 0;

		MovementDecider md;
		private float previousBestScore = -5_000;
		private Vector3 previousBestTarget = new Vector3(0, -10, 0);
		private List<Vector3> lastTargets = new List<Vector3>();

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
			if (Mathf.Approximately(amt, 0)) return;
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
			previousBestTarget = new Vector3(0, -10, 0);

			foreach (MountPoint mp in md.transform.GetComponentsInChildren<MountPoint>(true).PadRight(5))
			{
				mp.gameObject.SetActive(Random.value > 0.5f);
			}

			rewardRatio = Mathf.Max(rewardRatio - 0.005f, 0.1f); // ~1,140,000 steps
			lastTargets.Clear();
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

			int l1 = LayerMask.NameToLayer("EnemyBullets");

			List<Collider2D> padObj = objs.Where(c => c.gameObject.layer == l1).ToList();

			IOrderedEnumerable<Collider2D> bul = padObj.PadRight(50).OrderBy(a => a == null ? 100_000 : Vector3.Distance(a.transform.position.ReplaceZ(0), md.transform.position.ReplaceZ(0)));

			//int q = 0;
			foreach (Collider2D b in bul.Take(35))
			{
				AddBulletObservation(sensor, b);
				padObj.Remove(b);
			}

			for (int i = 0; i < 15; i++)
			{
				Collider2D b = padObj.GetRandom();
				AddBulletObservation(sensor, b);
				padObj.Remove(b);
			}

			IOrderedEnumerable<Collider2D> colliders;

			int l2 = LayerMask.NameToLayer("Powerups");
			padObj = objs.Where(c => c.gameObject.layer == l2).ToList();
			colliders = padObj.OrderBy(a => a == null ? 100_000 : a.transform.localPosition.y);
			//foreach (Collider2D b in colliders.PadRight(5))
			colliders = padObj.PadRight(5).OrderBy(a => a == null ? 100_000 : Vector3.Distance(a.transform.position.ReplaceZ(0), md.transform.position.ReplaceZ(0)));
			foreach (Collider2D b in colliders)
			{
				AddPowerupObservation(sensor,b);
				padObj.Remove(b);
			}

			//mounts
			foreach (MountPoint mp in md.transform.GetComponentsInChildren<MountPoint>(true).PadRight(5))
			{
				if (mp == null || !mp.gameObject.activeSelf)
				{
					sensor.AddObservation(Vector3.up);
					continue;
				}

				sensor.AddObservation(mp.transform.right.ReplaceZ(mp.GetShotSize()));
			}

			l1 = LayerMask.NameToLayer("Enemy");

			padObj = objs.Where(c => c.gameObject.layer == l1).ToList();

			colliders = padObj.PadRight(10).OrderBy(a => a == null ? 100000 : Vector3.Distance(a.transform.position.ReplaceZ(0), md.transform.position.ReplaceZ(0)));
			foreach (Collider2D b in colliders.Take(10))
			{
				AddEnemyObservation(sensor, b);
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

			Vector3 v = (coll.transform.localPosition).ReplaceZ(1);
			sensor.AddObservation(v / 10f);
		}

		private void AddBulletObservation(VectorSensor sensor, Collider2D coll)
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
			Vector3 v = (coll.transform.localPosition).ReplaceZ(coll.transform.localScale.x * colliderSize);

			sensor.AddObservation(v / 10f);
			sensor.AddObservation(coll.transform.right * bSpeed / 10);
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
			Vector3 v = (coll.transform.localPosition).ReplaceZ(colliderSize);

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
			lastTargets.Add(target);
			if (lastTargets.Count > 5)
			{
				lastTargets.RemoveAt(0);
			}
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

			float score = CalcScore(obs, target, true);
			previousBestScore = CalcScore(obs, previousBestTarget, false);

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
				//AddReward(-perUpdateScore * Mathf.Sqrt(Mathf.Max(Vector3.Distance(prevPosition, controlSignal), 1)) * 0.333f, "-s(prev)");
			}

			if (previousBestScore < score)
			{
				previousBestScore = score;
				//md.targetLocation = target;
				previousBestTarget = target;
				//if(previousBestScore > -1000)
				//	AddReward(perUpdateScore * 5, "Better");
				AddReward( perUpdateScore * Vector3.Distance(prevPosition, controlSignal) * 5, "+s(prev)");
				foreach (MovementDecider _md in FindObjectsOfType<MovementDecider>())
				{
					if(_md.totalSteps > 250_000)
						_md.targetLocation = target;
				}
			}
			prevPosition = (previousBestTarget) /10f;
		}

		private float CalcScore(float[] obs, Vector3 target, bool record)
		{
			if (obs.Length == 0) return 0;

			float edgeScores = 0;

			Color col2 = Color.yellow;
			float s;
			s = 1 - Mathf.Clamp(target.x - -8.5f, 0, 1);
			col2.a = s * s;

			if (s > 0)
			{
				edgeScores += -perUpdateScore * s * s * 2.5f * Mathf.Clamp(rewardRatio, 0.1f, 1);
				if (record)
					AddReward(-perUpdateScore * s * s * 2.5f * Mathf.Clamp(rewardRatio, 0.1f, 1), "-X edge");
			}

			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.left * 1, col2, 0.02f);

			s = 1 - Mathf.Clamp(8.5f - target.x, 0, 1);
			col2.a = s * s;
			if (s > 0)
			{
				edgeScores += -perUpdateScore * s * s * 2.5f * Mathf.Clamp(rewardRatio, 0.1f, 1);
				if (record)
					AddReward(-perUpdateScore * s * s * 2.5f * Mathf.Clamp(rewardRatio, 0.1f, 1), "+X edge");
			}

			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.right * 1, col2, 0.02f);

			s = 1 - Mathf.Clamp((target.y - -2.5f) * 2, 0, 1);
			col2.a = s * s;
			if (s > 0)
			{
				edgeScores += -perUpdateScore * s * s * 3.5f * Mathf.Clamp(rewardRatio, 0.1f, 1);
				if (record)
					AddReward(-perUpdateScore * s * s * 3.5f * Mathf.Clamp(rewardRatio, 0.1f, 1), "-Y edge");
			}

			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.down * 1, col2, 0.02f);

			s = 1 - Mathf.Clamp((2.5f - target.y) * 2, 0, 1);
			col2.a = s * s;
			if (s > 0)
			{
				edgeScores += -perUpdateScore * s * s * 3.5f * Mathf.Clamp(rewardRatio, 0.1f, 1);
				if (record)
					AddReward(-perUpdateScore * s * s * 3.5f * Mathf.Clamp(rewardRatio, 0.1f, 1), "+Y edge");
			}

			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.up * 1, col2, 0.02f);

			//float m1 = Mathf.Clamp(PoissonReward1(Vector3.Distance(md.targetLocation, target)), -1, 1);
			//float m2 = Mathf.Clamp(PoissonReward2(Vector3.Distance(target, md.transform.localPosition)), -1, 1);
			
			float b1 = 1_000;
			float b2 = 0;
			int bCt = 0;
			//Vector3 nearestBulletCur = oobPos;
			Vector3 nearestBulletPred = oobPos;

			int j = 0;

			for (int i = 0; i < 50 * 6; (i, j) = (i + 6, j + 6)) // 50 bullets
			{
				Vector3 pos = new Vector3(obs[j + 0], obs[j + 1], 0) * 10f;
				float size = obs[j + 2];
				if (size <= 0) continue;

				Vector3 vel = new Vector3(obs[j + 3], obs[j + 4], 0) * 10f;

				float tSec = Vector3.Distance(target, md.transform.localPosition) / (_speed / 2);

				Vector3 pred = pos + vel * tSec * 3f;

				Vector3 intersect = GetClosestPointOnLineSegment(pos, pred, target);

				Vector3 dif1 = intersect - target;
				float df1 = dif1.magnitude / 20;
				Vector3 dif2 = pred - target;
				float df2 = dif2.magnitude / 20;
				Vector3 dif3 = pos - target;
				float df3 = dif3.magnitude / 20;

				if (b1 > df1)
				{
					nearestBulletPred = intersect;
					//nearestBulletCur = pos;
				}

				if(float.IsNormal(df1))
					b1 = Mathf.Min(b1, df1);
				if(df1 < 1 || df2 < 1 || df3 < 1)
				{
					b2 += Mathf.Min(float.IsNormal(df1) ? df1 : 0 * 3, float.IsNormal(df2) ? df2 : 0 * 3, float.IsNormal(df3) ? df3 : 0 * 3);
					bCt++;
				}
			}

			if (b1 > 999)
				b1 = 0;
			b1 = Mathf.Clamp(b1*20, 0, 1f);

			if(bCt > 0)
				b2 /= bCt;
			float m3 = 0;
			float m4 = 0;
			float m5 = 1_000;
			float m6 = 0;

			for (int i = 0; i < 5 * 3; (i, j) = (i + 3, j + 3)) // 5 gems
			{
				Vector3 pos = new Vector3(obs[j + 0], obs[j + 1], 0) * 10f;
				float value = obs[j + 2];
				if (value <= 0) continue;
				m4++;
				float d = Vector3.Distance(pos, target);
				if (d < 0.5f)
				{
					m6 = Mathf.Max(1 / (d + 1f), m6);
				}

				if (d >= 8 && i > 0) continue;

				m3 += Mathf.Clamp(Mathf.Sqrt(Mathf.Max(d - (i / 4f), 0f) / 20), 0, 0.6f);
				m5 = Mathf.Min(m5, d / 20);
				Color col = new Color(0, 1 - Mathf.Clamp01(d * d / 100), 0, 2f / Time.timeScale);
				if (drawDebug && record) Debug.DrawLine(pos + GetLocalPos(), target + GetLocalPos(), col, 0.02f);

				Vector3 intersect = GetClosestPointOnLineSegment(md.LocalPos, target + GetLocalPos(), pos + GetLocalPos());
				float distInt = Vector3.Distance(md.LocalPos, intersect);
				float distPos = Vector3.Distance(pos + GetLocalPos(), intersect);
				if (distPos < 0.3f && distInt < Vector3.Distance(md.LocalPos, target + GetLocalPos()) && distInt < Vector3.Distance(target + GetLocalPos(), intersect))
				{
					m6 = Mathf.Max(1 / (distPos + 1), m6);
				}
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
				//m3 = Mathf.Clamp(Mathf.Sqrt(d)/4.5f, 0, 0.9f);
				//m5 = Mathf.Clamp(d / 5, 0, 1);
				Color col = new Color(Mathf.Clamp01(d * d / 100), 1 - Mathf.Clamp01(d * d / 100), 0, 2f / Time.timeScale);
				if (drawDebug && record) Debug.DrawLine(Vector3.zero + GetLocalPos(), target + GetLocalPos(), col, 0.02f);
			}

			List<Vector3> mounts = new List<Vector3>();
			for (int i = 0; i < 5 * 3; (i, j) = (i + 3, j + 3)) // 5 gun mounts
			{
				Vector3 dir = new Vector3(obs[j + 0], obs[j + 1], obs[j + 2]);
				mounts.Add(dir);
			}

			mounts = mounts.Distinct().ToList();
			float aim1 = 1_000;
			for (int i = 0; i < 10 * 6; (i, j) = (i + 6, j + 6)) // 10 enemies
			{
				Vector3 pos = new Vector3(obs[j + 0], obs[j + 1], 0) * 10f;
				Vector3 vel = new Vector3(obs[j + 3], obs[j + 4], obs[j + 5]) * 10f;
				float size = obs[j + 2]*10;
				float aim2 = 1_000;
				if (obs[j + 2] <= 0 || m4 > 0)
					continue;
				foreach (Vector3 mpDir in mounts)
				{
					//float nDist = Mathf.Min(Mathf.Max((pos - mv).magnitude - size - 0.1f, 0.1f), 0.95f);
					float arc = (size+mpDir.z) / Mathf.PI * 2;
					float d2 = Vector2.Angle((pos - target).normalized, mpDir.ReplaceZ(0));
					d2 /= 180;
					float d3 = Mathf.Clamp(d2 - arc, -1f, 1f);
					aim2 = Mathf.Min(d3, aim2);
					if (drawDebug && record)
					{
						if (d3 < 0)
						{
							d3 = -d3;
							Debug.DrawLine(target + GetLocalPos(), target + mpDir + GetLocalPos(), new Color(d3 * 2, 1 - d3, 0, 3f / Time.timeScale), 0.02f);
						}
						else
						{
							Debug.DrawLine(target + GetLocalPos(), target + mpDir + GetLocalPos(), new Color(d3 + 0.25f, 1 - d3 * 2, 0, 3f / Time.timeScale), 0.02f);
						}
					}
				}
				aim1 = Mathf.Min(aim1, aim2);
			}

			aim1 = Mathf.Clamp(aim1, -1, 1);

			float rm3 = m3;
			float rm5 = m5;

			m3 = Mathf.Pow(m3, 2.5f);
			m5 = Mathf.Pow(Mathf.Clamp(rm5, 0, 2), 2.5f);
			aim1 = Mathf.Sign(aim1) * Mathf.Pow(Mathf.Abs(aim1)*1.5f, 2.5f);

			if (drawDebug && record)
			{
				Color col;
				col = new Color(Mathf.Clamp01(rm5), 1 - Mathf.Clamp01(rm3), b2);
				DrawCircle(target + GetLocalPos(), 18, 0.15f, col);

				Debug.DrawLine(target + GetLocalPos(), nearestBulletPred + GetLocalPos(), new Color(1- (b1 / 2.5f), 0, 0, Mathf.Clamp01(5f / Time.timeScale)), 0.02f);
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
				AddReward(-perUpdateScore * m5 * 0.3f, "min(sGems)");
				AddReward( perUpdateScore * m6 * Mathf.Clamp(rewardRatio, 0.1f, 1) * 5, "nailed");
				AddReward(-perUpdateScore * b1 * b1 * 0.1f, "s(Bullet)");
				AddReward(-perUpdateScore * Mathf.Clamp01(target.y) * 0.15f, "y");
				//AddReward(-perUpdateScore * b2 / 5, "sAvg(Bullet)");
				AddReward( perUpdateScore * Mathf.Abs(aim1) * (aim1 < 0 ? -2 : 0) * Mathf.Clamp(1 - rewardRatio, 0.1f, 1), "t-aim");
			}

			;
			return (-perUpdateScore * m3 * 0.05f) + (-perUpdateScore * m5 * 0.3f) + (perUpdateScore * m6 * Mathf.Clamp(rewardRatio, 0.1f, 1) * 5)
			       + (perUpdateScore * Mathf.Abs(b1) * b1 * 0.1f) + (-perUpdateScore * Mathf.Clamp01(target.y) * 0.05f) 
			       + (perUpdateScore * Mathf.Abs(aim1) * (aim1 < 0 ? -2f : 0) * Mathf.Clamp(1 - rewardRatio, 0.1f, 1)) + edgeScores;
		}

		private static Vector2 GetClosestPointOnLineSegment(Vector2 A, Vector2 B, Vector2 P)
		{
			Vector2 AP = P - A;       //Vector from A to P   
			Vector2 AB = B - A;       //Vector from A to B  

			float magnitudeAB = AB.sqrMagnitude;        //Magnitude of AB vector (it's length squared)     
			float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
			float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

			if (distance < 0)     //Check if P projection is over vectorAB     
			{
				return A;

			}
			else if (distance > 1)
			{
				return B;
			}
			else
			{
				return A + AB * distance;
			}
		}

		private Vector3 GetLocalPos()
		{
			return transform.position;
		}

		private void DrawCircle(Vector3 point, int seg, float rad, Color col)
		{
			float angle = 2 * Mathf.PI / seg;
			Vector3 last = new Vector3(1, 0, 0);
			for (float a = 0; a < 2 * Mathf.PI; a += angle)
			{
				Vector3 p1 = new Vector3(Mathf.Cos(a), Mathf.Sin(a));
				//debugLines.Add((last*rad+point,p*rad+point,col));
				Debug.DrawLine(last * rad + point, p1 * rad + point, col, 0.02f);
				last = p1;
			}
			Debug.DrawLine(last * rad + point, new Vector3(1, 0, 0) * rad + point, col, 0.02f);
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

		public void FighterDamaged(Vector3 p)
		{
			float mult = 1;//10 - (md.totalSteps / 1_500_000f);
			foreach (Vector3 v in lastTargets)
			{
				if (Vector3.Distance(v, md.LocalPos) < 0.5f)
				{
					AddReward(perUpdateScore * mult, "dmg");
				}
			}

			if (md.totalSteps > 1_000_000 && Random.value < 0.1f)
			{
				md.SpawnGem(p + (Vector3)Random.insideUnitCircle * 0.5f);
			}
		}

		public void GemLost()
		{
			if (md.totalSteps < 700_000) return;
			if (Vector3.Distance(md.targetLocation, md.LocalPos) < 0.3f)
			{
				//AddReward(-perUpdateScore * 10f, "gem lost");
			}
		}
	}
}
