using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using Assets.draco18s.util;
using JetBrains.Annotations;
using Unity.Burst.Intrinsics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

namespace Assets.draco18s.bulletboss.entities
{
	public class MovementDecider : Agent
	{
		[SerializeField] private GameObject subTrainerPrefab;
		[SerializeField] private GameObject gemPrefab;
		[SerializeField] private GameObject bulletPrefab;
		[SerializeField] private GameObject fighterPrefab;
		[SerializeField] internal Transform gameContainer;
		[SerializeField] private bool drawDebug = false;
		[SerializeField] private bool textDebug = false;
		[SerializeField] private bool deterministicDebug = false;
		[SerializeField] private bool doMovement = true;
		[SerializeField] private bool runInfiniteEpisode = false;

		//public MovementDecider primary;

		public Vector3 TargetLocation => targetLocation;
		public Vector3 PrevMove => prevMove;
		public Vector3 LocalPos => transform.localPosition;
		public Vector3 WorldPos => transform.position;
		public Vector3 TrainingTargetLocation => trainingTargetLocation;


		public Vector3 targetLocation = Vector3.zero;
		//public static Vector3 s_trainingTargetLocation = Vector3.zero;
		public Vector3 trainingTargetLocation = Vector3.zero;
		private TargetDecider td;

		[SerializeField] private float rewardRatio = 2.25f;
		private float xpos = -4;
		private Vector3 oobPos = new Vector3(0, -100, 0);
		private int regenerate = 1;

		[SerializeField] private long totalSteps = 0;
		private long lastSteps = 0;

		private List<(Vector3, Vector3, Color)> debugLines = new List<(Vector3, Vector3, Color)>();
		private List<(Transform, Vector3)> startPositions = new List<(Transform, Vector3)>();

		protected override void Awake()
		{
			base.Awake();
			regenerate = 1;
			xpos = -4;
			targetLocation = oobPos;
			trainingTargetLocation = oobPos;
			//s_trainingTargetLocation = new Vector3(Random.value * 2 + 6, Random.value * 4 - 2, 0);

			/*for (int i = 0; i < 5; i++)
			{
				GameObject go = Instantiate(subTrainerPrefab, transform.parent);
				//MovementDeciderSub sub = go.GetComponent<MovementDeciderSub>();
				//BehaviorParameters par = go.GetComponent<BehaviorParameters>();
				//par.TeamId = i + 1;
			}*/
		}

		private Vector3 lastStartPos = Vector3.zero;

		[UsedImplicitly]
		public override void OnEpisodeBegin()
		{
			if (gameContainer == null)
			{
				StartCoroutine(TryAgain());
				return;
			}
			base.OnEpisodeBegin();

			if (td == null)
			{
				td = GetComponentInParent<TargetDecider>();
			}
			if (regenerate < 0)
			{
				//regenerate++;
				transform.localPosition = lastStartPos;
				foreach ((Transform t, Vector3 p) in startPositions)
				{
					t.localPosition = p;
				}
				return;
			}
			if (drawDebug) Debug.Log($"Total: {awards.Select(kvp => kvp.Value).Sum():F3} | " + string.Join(", ", awards.OrderByDescending(kvp => Mathf.Abs(kvp.Value)).Select(kvp => $"{kvp.Key}: {kvp.Value:F3}")) + $"\nReward Ratio: {rewardRatio}");
			awards.Clear();
			regenerate = 1;
			exactlyOnce = true;

			startPositions.Clear();
			debugLines.Clear();

			int leftRight = Random.value > 0.5f ? 1 : -1;
			bool spawnGems = true;// Random.value < 0.75f;
			bool spawnShots = (Random.value < 0.85f && (rewardRatio < 1.5f || totalSteps > 500_000)) || rewardRatio < 0.5f;

			Vector3 newPos = new Vector3(Random.value * 2 * -leftRight + 6 * -leftRight, Random.value * 4 - 2, 0);
			List<Collider2D> objs = new List<Collider2D>();
			foreach (Transform t in gameContainer)
			{
				Collider2D c = t.GetComponent<Collider2D>();
				if (c == null) continue;
				objs.Add(c);
			}

			targetLocation = new Vector3(Random.value * 16 - 8, Random.value * 4 - 2, 0);

			while (Vector3.Distance(TargetLocation, newPos) < 3)
			{
				newPos = new Vector3(Random.value * 2 * -leftRight + 6 * -leftRight, Random.value * 4 - 2, 0);
				leftRight = -leftRight;
			}
			transform.localPosition = newPos;

			if (transform.GetSiblingIndex() > 0) return;
			if (totalSteps - lastSteps < 15_000) return;

			gameContainer.Clear();
			lastSteps = totalSteps;

			int num = Mathf.FloorToInt(Random.value * 4 + Mathf.FloorToInt(totalSteps / 3_000_000f)) + 1;

			//numGemsSpawned = spawnGems ? num : 0;
			//gemsCollected = 0;

			for (int i = 0; i < num && spawnGems; i++)
			{
				GameObject go = Instantiate(gemPrefab, gameContainer);
				Vector3 p;
				do
				{
					p = new Vector3((Random.value * (3.5f + Mathf.Clamp(xpos / 10f, 0, 3)) + Mathf.Clamp(xpos / 2 - 2, -5, 3)) * leftRight, Random.value * 4, 0);
				} while (Vector3.Distance(p, transform.localPosition) < 2.0f);
				go.transform.localPosition = p;
				startPositions.Add((go.transform, p));
			}
			/*for (int i = 0; i < num && spawnGems; i++)
			{
				GameObject go = Instantiate(fighterPrefab, gameContainer);
				Vector3 p;
				p = new Vector3((Random.value * 20 -10), Random.value * 4 + 3, 0);
				go.transform.localPosition = p;
				go.transform.localRotation = Quaternion.Euler(0, 0, Mathf.RoundToInt(Random.value * 18) * 10 - 180);
			}*/
			float range = Mathf.Max((totalSteps) / 2_000_000f, 1);
			num = Mathf.FloorToInt(Random.value * (range+1)) + 1 + Mathf.Max(Mathf.FloorToInt((totalSteps - 1_000_000) / 3_000_000f), 0);
			num = Mathf.Min(num, 40);
			for (int i = 0; i < num && spawnShots; i++)
			{
				GameObject go = Instantiate(bulletPrefab, gameContainer);
				Vector3 p;
				do
				{
					p = new Vector3(Random.value * 18 - 9f, Random.value * range - range/2, 0);
				} while (Vector3.Distance(p, transform.localPosition) < 4.5f);
				go.transform.localPosition = p;
				go.transform.localScale = Vector3.one * ((Random.value * 2.5f) + 1f);
				startPositions.Add((go.transform, p));
			}
			/*for (int i = 0; i < 5; i++)
			{
				float x = (Random.value * 0.25f) + ((i+2) * 3.25f);
				float y = Random.value * 4.5f - 2.25f;
				GenerateVerticalBar(x, y, bulletPrefab);
			}*/

			prevMove = Vector2.zero;
			MaxStep = 3_000;
			//targetLocation = oobPos;

			lastStartPos = transform.localPosition;
		}

		private IEnumerator TryAgain()
		{
			yield return null;
			OnEpisodeBegin();
		}

		private void GenerateVerticalBar(float _x, float _y, GameObject prefab)
		{
			for (int i = 0; i < 15; i++)
			{
				float y = i * 0.5f - 3;

				if (Mathf.Abs(_y - y) <= 1f) continue;

				GameObject go = Instantiate(prefab, gameContainer);
				go.transform.localPosition = new Vector3(_x, y);
				go.transform.localScale = Vector3.one * ((Random.value / 2) + 0.5f);
			}
		}

		public override void CollectObservations(VectorSensor sensor)
		{
			Color w = Color.white;
			w.a = 0.2f;
			List<Collider2D> objs = new List<Collider2D>();
			foreach (Transform t in gameContainer)
			{
				Collider2D c = t.GetComponent<Collider2D>();
				if (transform.GetSiblingIndex() == 0)
					t.GetComponentInChildren<SpriteRenderer>().color = w;
				if (c == null) continue;
				objs.Add(c);
			}

			if (float.IsNaN(PrevMove.x) || float.IsNaN(PrevMove.x))
			{
				prevMove = Vector3.zero;
			}

			sensor.AddObservation(PrevMove.x);
			sensor.AddObservation(PrevMove.y);
			Vector3 targ = (TargetLocation - LocalPos).normalized;
			sensor.AddObservation(targ.x);
			sensor.AddObservation(targ.y);

			float d = Mathf.Abs((LocalPos.x - -8.5f) / 10f);
			sensor.AddObservation(d);
			d = Mathf.Abs((LocalPos.x - 8.5f) / 10f);
			sensor.AddObservation(d);

			d = Mathf.Abs((LocalPos.y - -2.5f) / 10f);
			sensor.AddObservation(d);
			d = Mathf.Abs((LocalPos.y - 2.5f) / 10f);
			sensor.AddObservation(d);

			int l1 = LayerMask.NameToLayer("EnemyBullets");

			List<Collider2D> padObj = objs.Where(c => c.gameObject.layer == l1).ToList();

			var bul = padObj.PadRight(Mathf.Max(padObj.Count, 15)).OrderBy(a => a == null ? 100_000 : Vector3.Distance(a.transform.localPosition.ReplaceZ(0), LocalPos.ReplaceZ(0))).ToList();

			float selfSize = GetComponent<CircleCollider2D>().radius;

			if (drawDebug) DrawCircle(transform.position, 16, 0.5f + selfSize, new Color(1, 0.333f, 0, 0.05f / Time.timeScale));

			//int q = 0;
			for (int i = 0; i < 3; i++)
			{
				Collider2D b = bul.First();
				if (b == null)
				{
					sensor.AddObservation(oobPos / 10f);
					sensor.AddObservation(Vector3.zero);
					continue;
				}

				float colliderSize = 0.1f;
				if (b is CircleCollider2D cir)
				{
					colliderSize = cir.radius;
				}

				if (Vector3.Distance(b.transform.localPosition.ReplaceZ(0), LocalPos.ReplaceZ(0)) >= 0.5f + colliderSize + selfSize)
				{
					sensor.AddObservation(oobPos / 10f);
					sensor.AddObservation(Vector3.zero);
					continue;
				}

				Vector3 v = (b.transform.localPosition - LocalPos).ReplaceZ(b.transform.localScale.x * colliderSize + selfSize);
				float bSpeed = b.GetComponent<IHasSpeed>().Speed;
				sensor.AddObservation(v / 1f);
				sensor.AddObservation(b.transform.right * bSpeed * 2);
				if (transform.GetSiblingIndex() == 0)
					b.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
				bul.Remove(b);
			}
			w.a = 0.6f;
			if (drawDebug) DrawCircle(transform.position, 16, 0.9f + selfSize, new Color(1, 0.666f, 0, 0.05f / Time.timeScale));
			for (int i = 0; i < 3; i++)
			{
				Collider2D b = bul.First();

				float colliderSize = selfSize;
				
				if (b == null)
				{
					sensor.AddObservation(oobPos / 10f);
					sensor.AddObservation(Vector3.zero);
					continue;
				}

				if (b is CircleCollider2D cir)
				{
					colliderSize = cir.radius;
				}

				if (Vector3.Distance(b.transform.localPosition.ReplaceZ(0), LocalPos.ReplaceZ(0)) >= 0.9f + colliderSize + selfSize)
				{
					sensor.AddObservation(oobPos / 10f);
					sensor.AddObservation(Vector3.zero);
					continue;
				}

				Vector3 v = (b.transform.localPosition - LocalPos).ReplaceZ(b.transform.localScale.x * colliderSize + selfSize);
				float bSpeed = b.GetComponent<IHasSpeed>().Speed;
				sensor.AddObservation(v / 2f);
				sensor.AddObservation(b.transform.right * bSpeed);
				bul.Remove(b);
				b.GetComponentInChildren<SpriteRenderer>().color = w;
			}
			w.a = 0.4f;
			if (drawDebug) DrawCircle(transform.position, 16, 3f + selfSize, new Color(1, 1f, 0, 0.05f / Time.timeScale));
			for (int i = 0; i < 3; i++)
			{
				Collider2D b = bul.First();
				if (b == null)
				{
					sensor.AddObservation(oobPos / 10f);
					sensor.AddObservation(Vector3.zero);
					continue;
				}

				float colliderSize = selfSize;
				if (b is CircleCollider2D cir)
				{
					colliderSize = cir.radius;
				}

				if (Vector3.Distance(b.transform.localPosition.ReplaceZ(0), LocalPos.ReplaceZ(0)) >= 3f + colliderSize + selfSize)
				{
					sensor.AddObservation(oobPos / 10f);
					sensor.AddObservation(Vector3.zero);
					continue;
				}

				Vector3 v = (b.transform.localPosition - LocalPos).ReplaceZ(b.transform.localScale.x * colliderSize + selfSize);
				float bSpeed = b.GetComponent<IHasSpeed>().Speed;
				sensor.AddObservation(v / 6f);
				sensor.AddObservation(b.transform.right * bSpeed / 2);
				bul.Remove(b);
				b.GetComponentInChildren<SpriteRenderer>().color = w;
			}

			for (int i = 0; i < 6; i++)
			{
				sensor.AddObservation(oobPos / 10f);
				sensor.AddObservation(Vector3.zero);
			}
		}

		internal Vector2 prevMove = Vector2.zero;
		private const float perUpdateScore = 0.005f;
		private float partialFrame = 0;
		private Dictionary<string, float> awards = new Dictionary<string, float>();

		public override void Heuristic(in ActionBuffers actionsOut)
		{
			var continuousActionsOut = actionsOut.ContinuousActions;
			continuousActionsOut[0] = Input.GetAxis("Horizontal");
			continuousActionsOut[1] = Input.GetAxis("Vertical");
			doMovement = !Input.GetKey(KeyCode.Space);
			if (!doMovement)
			{
				transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition).ReplaceZ(0);
			}
		}

		public void AddReward(float amt, string reason)
		{
			if (Mathf.Approximately(amt, 0)) return;
			AddReward(amt);
			awards.TryAdd(reason, 0);
			awards[reason] += amt;
		}

		public static Vector2 Rotate(Vector2 v, float delta)
		{
			return new Vector2(
				v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
				v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
			);
		}


		public override void OnActionReceived(ActionBuffers actionBuffers)
		{
			if (runInfiniteEpisode)
			{
				MaxStep++;
			}

			if (StepCount >= MaxStep - 1)
				regenerate = 1;
			//if (deterministicDebug)
			foreach ((Vector3 a, Vector3 b, Color c) in debugLines)
			{
				Debug.DrawLine(a, b, c, 0.02f);
			}

			Vector2 controlSignal = new Vector2(actionBuffers.ContinuousActions[0],actionBuffers.ContinuousActions[1]);
			float mag = Mathf.Clamp01(controlSignal.magnitude);
			controlSignal = controlSignal.normalized * mag;

			float d = Vector2.SignedAngle(PrevMove.normalized, controlSignal / mag);
			d /= 180;
			if (Mathf.Abs(d) > float.Epsilon)
				AddReward(-perUpdateScore * d * d * 5f * Mathf.Max(1 - rewardRatio, 0.1f), "turning");

			Vector3 mv = new Vector3(controlSignal.x, controlSignal.y, 0) * Time.fixedDeltaTime * 3;

			Color col = new Color(Mathf.Abs(d), 1 - Mathf.Abs(d), 0, Mathf.Max(1 - rewardRatio, 0.1f));

			if (drawDebug) Debug.DrawLine(transform.position + new Vector3(PrevMove.x, PrevMove.y, 0).normalized, transform.position + mv.normalized, col, 0.02f);
			if (drawDebug && textDebug) Debug.Log($"angle: {Vector2.SignedAngle(PrevMove.normalized, controlSignal / mag)} ({Mathf.Abs(d)})");
			float mx = Mathf.Lerp(.1f, .5f, Mathf.Clamp01(Mathf.Sqrt(Vector3.Distance(LocalPos, TargetLocation)) /2));

			float d1 = Vector2.Angle((TargetLocation - LocalPos).normalized, controlSignal / mag);
			d1 /= 180;
			float d1f = (mx-d1)/mx;
			if (Mathf.Abs(d1f) > float.Epsilon)
			{
				AddReward(perUpdateScore * Mathf.Abs(d1f) * d1f * 1.5f * Mathf.Max(rewardRatio, 0.1f), "aim");
			}

			List<float> allobs = GetObservations().ToList();
			List<float> obs = GetObservations().Skip(8).ToList();
			for (int i = 0; i < 9; i++)
			{
				int j = i * 6;
				Vector3 pos = new Vector3(obs[j + 0], obs[j + 1], 0);
				Vector3 dir = new Vector3(obs[j + 3], obs[j + 4], 0);
				float size = obs[j + 2];

				if (size < 0.1f || pos.y < -5)
				{
					//AddReward(perUpdateScore * 2f, "0dodge");
					continue;
				}

				float nDist = Mathf.Min(Mathf.Max((pos - mv).magnitude - size - 0.1f, 0.1f), 0.95f);
				float arc = (size / pos.magnitude) / Mathf.PI;
				float d2 = Vector2.Angle(pos.normalized, controlSignal / mag);
				d2 /= 180;
				float d3 = Mathf.Clamp(d2 - arc, -1f, 1f);
				//if (Mathf.Abs(d3) > float.Epsilon && d3 < 0)
				if (d3 < 0 && i < 3)
					AddReward(perUpdateScore * Mathf.Abs(d3) * d3 * 20f / nDist * mag, "-dodge");

				float d4 = Vector2.Angle(dir.normalized, controlSignal / mag);
				d4 /= 180;
				float d5 = 1 - d4;
				//float d5 = Mathf.Clamp(d4 - arc, -1f, 1f);
				if (d5 > 0)
					AddReward(-perUpdateScore * d5 * d5 * (i < 6 ? 1f : 0.1f), "-dodge2");

				//float d4 = d2 * (1-d1) - 0.1f;
				//if (mag > 0)
				//	AddReward( perUpdateScore * d4 * d4 * 1.5f * Mathf.Sign(d4) * (Mathf.Sign(d4) < 0 ? 2 : 1) * mag, (d4 > 0 ? "+" : "-") + "dodge");

				if (drawDebug && d3 < 0 && i < 3)
				{
					Color c = new Color(Mathf.Abs(d3) * (d3 < 0 ? 25 : 0), nDist, 0);
					Debug.DrawLine(WorldPos + pos - (pos.normalized * .2f), WorldPos + pos + (pos.normalized * (size + .5f)), c, 0.02f);
					//Debug.Log($"{Mathf.Abs(d3) * (d3 < 0 ? -1 : 0)}, +{d4 * (d4 > 0 ? 1 : 0)}");
				}
			}

			//float m = Mathf.Clamp01(Mathf.Abs(prevMove.magnitude - mag));
			//AddReward(-perUpdateScore * m * m * 10, "dv");

			float s = Vector3.Distance(LocalPos, TargetLocation) - Vector3.Distance(LocalPos + mv, TargetLocation);
			//if (s >= 0)
			//	s *= 0;
			AddReward(perUpdateScore * s * 2.5f,"move");
			//AddReward(perUpdateScore * 1/(s+0.1f),"dist");
			//AddReward(-perUpdateScore * 0.1f, "time");
			/*if (s < 1)
			{
				AddReward(perUpdateScore * 1 / (s * mag + 0.1f), "slow");
			}*/
			col = new Color(0 - d1f, d1f, 0, Mathf.Max(rewardRatio, 0));
			if (drawDebug && textDebug) Debug.Log($"dist: {mx}, angle: {Vector2.Angle((TargetLocation - LocalPos).normalized, controlSignal / mag)} ({d1}) = {mx - d1}");
			if (drawDebug) Debug.DrawLine(transform.position + mv.normalized * 1.3f, transform.position + mv.normalized * 1.1f, col, 0.02f);


			//var obs = GetObservations().Skip(6).ToArray();
			//float acc = Mathf.Abs(mag - PrevMove.magnitude);
			//float sig = Mathf.Sign(mag - PrevMove.magnitude);
			//col = new Color(acc, 1 - acc, 0);
			//if (drawDebug || deterministicDebug)
			//	Debug.DrawLine(Vector3.up*3 + Vector3.right * (Time.time % 1), Vector3.up*3 + Vector3.up * Mathf.Clamp01(acc * 4 + 0.01f) * 2 * sig + Vector3.right * (Time.time % 1), col, 1);

			Vector3 origPos = transform.position;

			if (doMovement)
			{
				/*if (stunTimer > 0)
					stunTimer -= Time.fixedDeltaTime;
				if (stunTimer <= 0)*/
				transform.Translate(mv, Space.Self);

				if (deterministicDebug) debugLines.Add((origPos, transform.position, Color.white));
			}
			else if (deterministicDebug)
				Debug.DrawLine(origPos + mv.normalized, origPos + mv.normalized * 1.1f, Color.white, 0.02f);

			if (LocalPos.x < -8f || LocalPos.x > 8f || LocalPos.y < -2f || LocalPos.y > 2f)
			{
				//AddReward(-perUpdateScore, "bounds");
				transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, -8.5f, 8.5f), Mathf.Clamp(transform.localPosition.y, -2.5f, 2.5f), transform.localPosition.z);
			}

			Color col2 = Color.yellow;
			s = 1 - Mathf.Clamp(Mathf.Abs(LocalPos.x - -8.75f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 1.5f * (controlSignal.x < 0.05 ? (1 + mag * 2) : -0.01f), "-X edge proximity");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.left * 1, (s < 0.001 ? Color.clear : (controlSignal.x < 0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(LocalPos.x - 8.75f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 1.5f * (controlSignal.x > -0.05 ? (1 + mag * 2) : -0.01f), "+X edge proximity");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.right * 1, (s < 0.001 ? Color.clear : (controlSignal.x > -0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(LocalPos.y - -2.25f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 1.5f * (controlSignal.y < 0.05 ? (1 + mag * 2) : -0.01f), "-Y edge proximity");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.down * 1, (s < 0.001 ? Color.clear : (controlSignal.y < 0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(LocalPos.y - 2.25f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 1.5f * (controlSignal.y > -0.05 ? (1 + mag * 2) : -0.01f), "+Y edge proximity");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.up * 1, (s < 0.001 ? Color.clear : (controlSignal.y > -0.05 ? Color.red : col2)), 0.02f);

			prevMove = controlSignal / mag;

			if (Vector3.Distance(LocalPos, TargetLocation) < 0.3f)
			{
				regenerate = 1;
				totalSteps += StepCount;
				rewardRatio = Mathf.Max(rewardRatio - (0.000035f * Mathf.Max(2_000 - StepCount, 100)) / 100f, 0);
				AddReward(1, "Arrive");

				List<Collider2D> objs = new List<Collider2D>();
				foreach (Transform t in gameContainer)
				{
					Collider2D c = t.GetComponent<Collider2D>();
					if (c == null) continue;
					objs.Add(c);
				}

				int max = 1000;
				do
				{
					targetLocation = new Vector3(Random.value * 16 - 8, Random.value * 4 - 2, 0);
				} while (Vector3.Distance(targetLocation, LocalPos) < 3 || (objs.Any(b => Vector3.Distance(targetLocation, b.transform.localPosition) < 2) && max-->0));

				if (max <= 0)
				{
					float range = Mathf.Max((totalSteps) / 2_000_000f, 1);
					Vector3 p;
					foreach (var c in objs.Where(b => Vector3.Distance(targetLocation, b.transform.localPosition) < 2)) 
					{
						do
						{
							p = new Vector3(Random.value * 18 - 9f, Random.value * range - range / 2, 0);
						} while (Vector3.Distance(p, transform.localPosition) < 2f);

						c.transform.localPosition = p;
					}
				}

				//EndEpisode();
				//td.EndEpisode();
				//int leftRight = Random.value > 0.5f ? 1 : -1;
				//s_trainingTargetLocation = new Vector3(Random.value * 2 * -leftRight + 6 * -leftRight, Random.value * 4 - 2, 0);
				if (drawDebug)
					DrawCircle(td.transform.TransformPoint(TargetLocation), 18, 0.3f, new Color(1f - mag, mag, 0));
			}
			else if (drawDebug)
				DrawCircle(td.transform.TransformPoint(TargetLocation), 18, 0.3f, Color.white);
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

		void OnTriggerEnter2D(Collider2D other)
		{
			if (totalSteps < 300_000) return;
			/*if (other.gameObject.layer == LayerMask.NameToLayer("Powerups"))
			{
				xpos += 0.1f;
				//AddReward(0.05f, "gem collect");
				//td.AddReward(perUpdateScore*0.15f, "gem collect");
				//SetReward(0.75f);
				//EndEpisode(); return;
				//Destroy(other.gameObject);

				Vector3 p = other.gameObject.transform.position;
				if (deterministicDebug) debugLines.Add((p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.green));
				if (deterministicDebug) debugLines.Add((p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.green));

				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.green, 1.5f);
				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.green, 1.5f);

				if (Random.value > 0.3) return;

				GameObject go = Instantiate(fighterPrefab, gameContainer);
				p = new Vector3((Random.value * 20 - 10), Random.value * 4 + 3, 0);
				go.transform.localPosition = p;
				go.transform.localRotation = Quaternion.Euler(0, 0, Mathf.RoundToInt(Random.value * 18) * 10 - 180);
			}*/
			if (other.gameObject.layer == LayerMask.NameToLayer("EnemyBullets"))
			{
				if (totalSteps < 2_000_000)
				{
					return;
				}
				else if (totalSteps < 4_000_000)
				{
					AddReward(-perUpdateScore * 100f, "damaged");
					return;
				}
				else if (totalSteps < 5_000_000)
				{
					regenerate = regenerate > 0 ? -5 : regenerate;
					AddReward(-1f, "damaged");
					//SetReward(-1);
					//td.AddReward(-perUpdateScore * 0.15f, "damaged");
					//EndEpisode(); return;
					OnEpisodeBegin();
					return;
				}
				else
				{
					AddReward(-1f, "damaged");
					EndEpisode(); return;
				}
				//Destroy(other.gameObject);

				Vector3 p = other.gameObject.transform.position;
				if (deterministicDebug) debugLines.Add((p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.red));
				if (deterministicDebug) debugLines.Add((p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.red));

				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.red, 1.5f);
				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.red, 1.5f);

				if (Random.value > 0.15) return;

				GameObject go = Instantiate(bulletPrefab, gameContainer);
				do
				{
					p = new Vector3(Random.value * 17 - 8.5f, Random.value * 6 - 3, 0);
				} while (Vector3.Distance(p, transform.localPosition) < 1.5f);
				go.transform.localPosition = p;
				go.transform.localScale = Vector3.one * ((Random.value / 2) + 0.5f);
			}
		}

		static float Cross(Vector3 point1, Vector3 point2)
		{
			//we don't care about z
			return point1.x * point2.y - point1.y * point2.x;
		}

		public void SetContainer(Transform bulletContainer)
		{
			gameContainer = bulletContainer;
		}

		private bool exactlyOnce = true;

		public void EndEpisode(bool doOnce)
		{
			if (!exactlyOnce) return;

			exactlyOnce = false;
			EndEpisode();
		}
	}
}
