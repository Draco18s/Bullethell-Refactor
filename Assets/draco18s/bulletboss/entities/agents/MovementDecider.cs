using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.draco18s.util;
using JetBrains.Annotations;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
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
		//public Vector3 TrainingTargetLocation => trainingTargetLocation;


		public Vector3 targetLocation = Vector3.zero;
		//public static Vector3 s_trainingTargetLocation = Vector3.zero;
		//public Vector3 trainingTargetLocation = Vector3.zero;
		private TargetDecider td;

		[SerializeField] private float rewardRatio = 2.25f;
		private float xpos = -4;
		private Vector3 oobPos = new Vector3(0, -100, 0);
		private int regenerate = 1;

		public long totalSteps;
		private long lastSteps = 0;

		private List<(Vector3, Vector3, Color)> debugLines = new List<(Vector3, Vector3, Color)>();
		private List<(Transform, Vector3)> startPositions = new List<(Transform, Vector3)>();
		private Color black = Color.black;

		protected override void Awake()
		{
			base.Awake();
			regenerate = 1;
			xpos = -4;
			targetLocation = oobPos;
			rewardRatio = Mathf.Min(rewardRatio, 1);
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
					if(t == null) continue;
					t.localPosition = p;
				}
				return;
			}
			if (drawDebug) Debug.Log($"Total: {awards.Select(kvp => kvp.Value).Sum():F3} | " + string.Join(", ", awards.OrderByDescending(kvp => Mathf.Abs(kvp.Value)).Select(kvp => $"{kvp.Key}: {kvp.Value:F3}")) + $"\nReward Ratio: {rewardRatio}; {totalSteps}");
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

			if (totalSteps < 250_000)
				targetLocation = new Vector3(Random.value * 16 - 8, Random.value * 4 - 2, 0);

			while (Vector3.Distance(TargetLocation, newPos) < 3)
			{
				newPos = new Vector3(Random.value * 2 * -leftRight + 6 * -leftRight, Random.value * 4 - 2, 0);
				leftRight = -leftRight;
			}
			transform.localPosition = newPos;

			if (transform.GetSiblingIndex() > 0) return;
			if (totalSteps - lastSteps < 3_000) return;

			gameContainer.Clear();
			lastSteps = totalSteps;

			int num = Mathf.FloorToInt(Random.value * 3 + Mathf.FloorToInt(totalSteps / 2_000_000f)) + 1;
			num = Mathf.Min(num, 20);

			for (int i = 0; i < Mathf.Min(num, 10) && spawnGems; i++)
			{
				GameObject go = Instantiate(gemPrefab, gameContainer);
				Vector3 p;
				do
				{
					p = new Vector3((Random.value * (3.5f + Mathf.Clamp(xpos / 10f, 0, 3)) + Mathf.Clamp(xpos / 2 - 2, -5, 3)) * leftRight, Random.value * 3 + 2, 0);
				} while (Vector3.Distance(p, transform.localPosition) < 2.0f);
				go.transform.localPosition = p;
				startPositions.Add((go.transform, p));
			}
			for (int i = 0; i < num/2 && totalSteps > 100_000 && spawnGems; i++)
			{
				GameObject go = Instantiate(fighterPrefab, gameContainer);
				Vector3 p;
				p = new Vector3((Random.value * 20 -10), Random.value * 4 + 3, 0);
				go.transform.localPosition = p;
				go.transform.localRotation = Quaternion.Euler(0, 0, Mathf.RoundToInt(Random.value * 18) * 10 - 180);
			}

			float range = Mathf.Max((totalSteps) / 1_500_000f, 1);
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
				if (totalSteps > 6_000_000f)
				{
					IHasSpeed mover = go.GetComponent<IHasSpeed>();
					mover.SpecialSetup(0, (Random.value / 4 + 0.25f) * 2);
				}
			}

			prevMove = Vector2.zero;
			MaxStep = 3_000;

			lastStartPos = transform.localPosition;
		}

		private void SetupGuns()
		{
			foreach (MountPoint mp in GetComponentsInChildren<MountPoint>())
			{
				mp.enabled = Random.value > 0.7;
			}
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
				if (transform.GetSiblingIndex() == 0 && drawDebug && LayerMask.LayerToName(t.gameObject.layer) != "Enemy")
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

			List<Collider2D> bul = padObj.PadRight(Mathf.Max(padObj.Count, 15)).OrderBy(a => a == null ? 100_000 : Vector3.Distance(a.transform.localPosition.ReplaceZ(0), LocalPos.ReplaceZ(0))).ToList();

			float selfSize = GetComponent<CircleCollider2D>().radius;

			if (drawDebug) DrawCircle(transform.position, 16, 0.5f + selfSize, new Color(1, 0.333f, 0, 2f / Time.timeScale));

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
			if (drawDebug) DrawCircle(transform.position, 16, 0.9f + selfSize, new Color(1, 0.666f, 0, 2f / Time.timeScale));
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
			if (drawDebug) DrawCircle(transform.position, 16, 3f + selfSize, new Color(1, 1f, 0, 2f / Time.timeScale));
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

			for (int i = 0; i < 3; i++)
			{
				sensor.AddObservation(oobPos / 10f);
				sensor.AddObservation(oobPos / 10f);
			}

			if (totalSteps < 50_000)
			{
				for (int i = 0; i < 6; i++)
				{
					sensor.AddObservation(oobPos / 10f);
				}
			}
			else
			{
				int l2 = LayerMask.NameToLayer("Powerups");
				padObj = objs.Where(c => c.gameObject.layer == l2).ToList();
				IOrderedEnumerable<Collider2D> colliders = padObj.OrderBy(a => a == null ? 100_000 : a.transform.localPosition.y);
				//foreach (Collider2D b in colliders.PadRight(5))
				colliders = padObj.PadRight(6).OrderBy(a => a == null ? 100_000 : Vector3.Distance(a.transform.localPosition.ReplaceZ(0), LocalPos.ReplaceZ(0)));
				if (drawDebug) DrawCircle(transform.position, 16, 1.5f + selfSize, new Color(0, 1f, 0, 2f / Time.timeScale));

				float[] allobs = GetObservations().ToArray();

				foreach (Collider2D b in colliders)
				{
					if (b == null)
					{
						sensor.AddObservation(oobPos / 10f);
						continue;
					}

					if (Vector3.Distance(b.transform.localPosition.ReplaceZ(0), LocalPos.ReplaceZ(0)) >= 1.5f + selfSize)
					{
						sensor.AddObservation(oobPos / 10f);
						continue;
					}

					Vector3 v = (b.transform.localPosition).ReplaceZ(1);
					sensor.AddObservation(v / 2f);
					padObj.Remove(b);
					if (transform.GetSiblingIndex() == 0)
						b.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
				}
			}
		}

		internal Vector2 prevMove = Vector2.zero;
		private const float perUpdateScore = 0.0025f;
		private float partialFrame = 0;
		private Dictionary<string, float> awards = new Dictionary<string, float>();

		public override void Heuristic(in ActionBuffers actionsOut)
		{
			ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
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
			black.a = 1 / Time.timeScale;
			if (runInfiniteEpisode)
			{
				MaxStep++;
			}
			totalSteps++;
			if (StepCount >= MaxStep - 1)
				regenerate = 1;
			//if (deterministicDebug)
			foreach ((Vector3 a, Vector3 b, Color c) in debugLines)
			{
				Debug.DrawLine(a, b, c, 0.02f);
			}

			Vector2 controlSignal = new Vector2(actionBuffers.ContinuousActions[0],actionBuffers.ContinuousActions[1]);
			float mag = Mathf.Clamp01(controlSignal.magnitude);
			Vector2 controlSignalNorm = controlSignal.normalized;
			controlSignal = controlSignalNorm * mag;

			Vector3 mv = new Vector3(controlSignal.x, controlSignal.y, 0) * Time.fixedDeltaTime * 3;
			float tDist = Vector3.Distance(LocalPos + mv, TargetLocation);

			bool goLong = totalSteps > 5_500_000;

			float locRatio = tDist < 0.3f ? 0 : rewardRatio;

			float d = Vector2.Distance(PrevMove, controlSignal);
			//d /= 180;
			if (Mathf.Abs(d) > float.Epsilon)
				AddReward(-perUpdateScore * d * d * 5f * Mathf.Max(1 - locRatio, 0.1f) * mag * (goLong ? 0.5f : 1), "turning");

			d *= mag;
			Color col = new Color(Mathf.Abs(d), 1 - Mathf.Abs(d), 0, Mathf.Max(1 - locRatio, 0.1f));

			if (drawDebug) Debug.DrawLine(transform.position + new Vector3(PrevMove.x, PrevMove.y, 0).normalized, transform.position + mv.normalized, col, 0.02f);
			//if (drawDebug && textDebug) Debug.Log($"angle: {Vector2.SignedAngle(PrevMove.normalized, controlSignalNorm)} ({Mathf.Abs(d)})");
			
			List<float> allobs = GetObservations().ToList();
			List<float> obs = GetObservations().Skip(8).ToList();
			float posMult = 1;
			for (int i = 0; i < 9; i++)
			{
				if (i < 3) posMult = 1f;
				else if (i < 6) posMult = 2f;
				else posMult = 6;
				int j = i * 6;
				Vector3 pos = new Vector3(obs[j + 0], obs[j + 1], 0) * posMult;
				Vector3 dir = new Vector3(obs[j + 3], obs[j + 4], 0);
				float size = obs[j + 2];

				if (size < 0.1f || pos.y < -5)
				{
					continue;
				}

				float nDist = Mathf.Min(Mathf.Max((pos - mv).magnitude - size - 0.1f, 0.1f), 0.95f);
				float arc = (size / pos.magnitude) / Mathf.PI;
				float d2 = Vector2.Angle(pos.normalized, controlSignalNorm);
				d2 /= 180;
				float d3 = Mathf.Clamp(d2 - arc, -1f, 1f);
				if (d3 < 0 && i < 3)
					AddReward(perUpdateScore * Mathf.Abs(d3) * d3 * 20f / nDist * mag, "-dodge");

				float d4 = Vector2.Angle(dir.normalized, controlSignalNorm);
				d4 /= 180;
				float d5 = 1 - d4;
				if (d5 > 0)
					AddReward(-perUpdateScore * d5 * d5 * (i < 6 ? 1f : 0.1f), "-dodge2");

				if (drawDebug && d3 < 0 && i < 3)
				{
					Color c = new Color(Mathf.Abs(d3) * (d3 < 0 ? 25 : 0), nDist, 0);
					Debug.DrawLine(WorldPos + pos - (pos.normalized * .2f), WorldPos + pos + (pos.normalized * (size + .5f)), c, 0.02f);
				}
			}
			posMult = 2f;
			int visibleGems = 0;

			for (int i = 0; i < 6; i++)
			{
				int j = i * 3 + (12 * 6);
				Vector3 pos = new Vector3(obs[j + 0], obs[j + 1], 0) * posMult;
				float value = obs[j + 2]* posMult;
				if (value <= 0)
					continue;
				visibleGems++;

				float mx = Mathf.Lerp(.1f, .5f, Mathf.Clamp01(Mathf.Sqrt(Vector3.Distance(LocalPos, pos)) / 2));

				float d1 = Vector2.Angle((pos - LocalPos).normalized, controlSignalNorm);
				d1 /= 180;
				float d1f = (mx - d1) / mx;
				if (Mathf.Abs(d1f) > float.Epsilon /*&& Vector3.Distance(pos, LocalPos) > 0.25f*/)
				{
					AddReward(perUpdateScore * Mathf.Abs(d1f) * d1f * 3f / (i+1), "aim");
				}

				col = new Color(0 - d1f, d1f, 1, 1f / (i + 1));
				if (drawDebug && i == 0)
				{
					Debug.DrawLine(transform.position + mv.normalized * 1.3f, transform.position + mv.normalized * 1.1f, col, 0.02f);
					Vector3 center = new Vector3(0, -6, 0);
					Debug.DrawLine(center + mv.normalized * 1.5f, center + mv.normalized * 1.2f, col, 0.5f / Time.timeScale);
				}
			}
			float s = Vector3.Distance(LocalPos, TargetLocation) - Vector3.Distance(LocalPos + mv, TargetLocation);
			
			AddReward(perUpdateScore * s * 2.5f * (goLong ? 0 : 1), "move");

			if (visibleGems == 0)
			{
				float mx = Mathf.Lerp(.1f, .5f, Mathf.Clamp01(Mathf.Sqrt(Vector3.Distance(LocalPos, TargetLocation)) / 2));

				float d1 = Vector2.Angle((TargetLocation - LocalPos).normalized, controlSignalNorm);
				d1 /= 180;
				float d1f = (mx - d1) / mx;
				if (Mathf.Abs(d1f) > float.Epsilon && Vector3.Distance(TargetLocation, LocalPos) > 0.25f)
				{
					AddReward(perUpdateScore * Mathf.Abs(d1f) * d1f * 1.5f * Mathf.Max(locRatio, 0.15f), "aim");
				}

				col = new Color(0 - d1f, d1f, 0, Mathf.Max(locRatio, 0.3f));
				if (drawDebug)
				{
					Debug.DrawLine(transform.position + mv.normalized * 1.3f, transform.position + mv.normalized * 1.1f, col, 0.02f);

					Vector3 center = new Vector3(0, -6, 0);
					DrawCircle(center + Vector3.back, 16, Mathf.Log10(1.80f), black); //one sixty-fourth
					DrawCircle(center + Vector3.back, 16, Mathf.Log10(3.25f), black); //quarter
					DrawCircle(center + Vector3.back, 16, Mathf.Log10(5.50f), black); //half
					DrawCircle(center + Vector3.back, 24, Mathf.Log10(10.0f), black); //one
					Debug.DrawLine(center + mv.normalized * 1.5f, center + mv.normalized * 1.2f, col, 0.2f / Time.timeScale);

					d = Vector2.Distance(PrevMove, controlSignal);
					col = new Color(Mathf.Abs(d), 1 - Mathf.Abs(d), 0, Mathf.Max(1 - locRatio, 0.1f));

					Vector3 l1 = Log9It(prevMove);
					Vector3 l2 = Log9It(controlSignal);
					Debug.DrawLine(center + l1, center + l2, col, 0.2f / Time.timeScale);
				}
			}
			else if (drawDebug)
			{
				Vector3 center = new Vector3(0, -6, 0);
				DrawCircle(center + Vector3.back, 16, Mathf.Log10(1.80f), black);
				DrawCircle(center + Vector3.back, 16, Mathf.Log10(3.25f), black);
				DrawCircle(center + Vector3.back, 16, Mathf.Log10(5.50f), black);
				DrawCircle(center + Vector3.back, 24, Mathf.Log10(10.0f), black);
				d = Vector2.Distance(PrevMove, controlSignal);
				col = new Color(Mathf.Abs(d), 1 - Mathf.Abs(d), 0, Mathf.Max(1 - locRatio, 0.1f));

				Vector3 l1 = Log9It(prevMove);
				Vector3 l2 = Log9It(controlSignal);
				Debug.DrawLine(center + l1, center + l2, col, 0.2f / Time.timeScale);
			}

			Vector3 origPos = transform.position;

			if (doMovement)
			{
				transform.Translate(mv, Space.Self);

				if (deterministicDebug) debugLines.Add((origPos, transform.position, Color.white));
			}
			else if (deterministicDebug)
				Debug.DrawLine(origPos + mv.normalized, origPos + mv.normalized * 1.1f, Color.white, 0.02f);

			if (LocalPos.x < -8f || LocalPos.x > 8f || LocalPos.y < -2f || LocalPos.y > 2f)
			{
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

			prevMove = controlSignal;

			if (totalSteps > 150_000)
			{
				if (tDist <= 0.3f)
				{
					AddReward(perUpdateScore * ((visibleGems > 0 ? -1f : 1) - mag) * 0.75f, "Arrive");
					awards.TryAdd("mag", 0);
					awards["mag"] += -(perUpdateScore * mag);
					DrawCircle(td.transform.TransformPoint(TargetLocation), 18, 0.3f, visibleGems > 0 ? Color.black : new Color(1f - mag, mag, 0));
					rewardRatio = Mathf.Max(rewardRatio - (0.000035f * Mathf.Max(2_000 - StepCount, 100)) / 100f, -1);
				}
				else if (visibleGems > 0 && tDist < 1.5f)
				{
					//AddReward(perUpdateScore * 2.5f, "near");
					DrawCircle(td.transform.TransformPoint(TargetLocation), 18, 0.3f, Color.cyan);
				}
				else
				{
					DrawCircle(td.transform.TransformPoint(TargetLocation), 18, 0.3f, Color.white);
				}
			}
			else
			{
				if (tDist <= 0.3f)
				{
					regenerate = 1;
					rewardRatio = Mathf.Max(rewardRatio - (0.000035f * Mathf.Max(2_000 - StepCount, 100)) / 100f, -1);
					AddReward(1, "Arrive");

					List<Collider2D> objs = new List<Collider2D>();
					foreach (Transform t in gameContainer)
					{
						Collider2D c = t.GetComponent<Collider2D>();
						if (c == null) continue;
						objs.Add(c);
					}

					int max = 1000;
					Vector3? nGem = null;
					float dist = 0;
					do
					{
						targetLocation = new Vector3(Random.value * 16 - 8, Random.value * 4 - 2, 0);
						dist = Vector3.Distance(targetLocation, LocalPos);
						if (totalSteps > 50_000 && dist < 1 && dist > 0.2f)
						{
							nGem ??= targetLocation;
							Vector2 r = (Random.insideUnitCircle * 0.3f);
							targetLocation = LocalPos + new Vector3(r.x,r.y,0);
							break;
						}
					} while (dist < 3 || (objs.Any(b => Vector3.Distance(targetLocation, b.transform.localPosition) < 2) && max-- > 0));

					if (nGem.HasValue)
					{
						GameObject go = Instantiate(gemPrefab, gameContainer);
						go.transform.localPosition = nGem.Value;
					}

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
				}
				if (drawDebug)
					DrawCircle(td.transform.TransformPoint(TargetLocation), 18, 0.3f, Color.white);
			}
			if (goLong)
				AddReward(perUpdateScore * 0.75f, "Go Long");
		}

		private Vector3 Log9It(Vector3 v)
		{
			float m = Mathf.Log10(v.magnitude * 9 +1);
			return v.normalized * m;
			//return new Vector3(Mathf.Sign(v.x) * Mathf.Log10(Mathf.Abs(v.x) * 9 + 1), Mathf.Sign(v.y) * Mathf.Log10(Mathf.Abs(v.y) * 9 + 1), Mathf.Sign(v.z) * Mathf.Log10(Mathf.Abs(v.z) * 9 + 1));
		}

		private void DrawCircle(Vector3 point, int seg, float rad, Color col)
		{
			if (rad <= 0.3f)
				seg = Mathf.Min(seg, 8);
			if (rad <= 2)
				seg = Mathf.Min(seg, 16);
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

		void OnTriggerEnter2D(Collider2D other)
		{
			if (totalSteps < 50_000) return;
			//Application.Quit(1);
			if (other.gameObject.layer == LayerMask.NameToLayer("Powerups"))
			{
				xpos += 0.1f;
				AddReward(1f, "gem collect");
				td.AddReward(0.1f, "gem collect");
				//SetReward(0.75f);
				//EndEpisode(); return;
				Destroy(other.gameObject);

				Vector3 p = other.gameObject.transform.position;
				if (deterministicDebug) debugLines.Add((p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.green));
				if (deterministicDebug) debugLines.Add((p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.green));

				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.green, 1.5f);
				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.green, 1.5f);

				if (Random.value > 0.85 || totalSteps > 1_000_000) return;

				SpawnGem(new Vector3((Random.value * 18 - 9), Random.value * 3, 0));
			}
			if (other.gameObject.layer == LayerMask.NameToLayer("EnemyBullets"))
			{
				if (totalSteps < 3_000_000)
				{
					AddReward(-perUpdateScore * 1f, "damaged");
					return;
				}
				/*else if (totalSteps < 5_000_000)
				{
					regenerate = regenerate > 0 ? -5 : regenerate;
					AddReward(-0.3f, "damaged");
					//SetReward(-1);
					//td.AddReward(-perUpdateScore * 0.15f, "damaged");
					//EndEpisode(); return;
					OnEpisodeBegin();
					return;
				}*/
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

		public void SpawnGem(Vector3 p)
		{
			GameObject go = Instantiate(gemPrefab, gameContainer);
			go.transform.localPosition = p;
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
