using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.draco18s.util;
using JetBrains.Annotations;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.draco18s.bulletboss.entities
{
	public class PlayerAgent : Agent
	{
		public bool IsTraining => !runInfiniteEpisode;
		[SerializeField] private GameObject gemPrefab;
		[SerializeField] private GameObject bulletPrefab;
		[SerializeField] private Transform gameContainer;
		[SerializeField] private bool drawDebug = false;
		[SerializeField] private bool drawGizmo = false;
		[SerializeField] private bool deterministicDebug = false;
		[SerializeField] private bool doMovement = true;
		[SerializeField] private bool runInfiniteEpisode = false;

		private float xpos = -4;
		private float prob = 1.15f;
		//private Vector3 oobPos = new Vector3(0, -10, -10);

		[SerializeField] private long totalSteps = 0;

		[SerializeField] private Vector3Int _gridSize;
		[SerializeField] private Vector3 _gridCellSize;
		[SerializeField] private Vector3Int _gridOffset;

		private void OnDrawGizmos()
		{
			if (!drawGizmo) return;

			for (int ox = -_gridSize.x / 2; ox <= _gridSize.x / 2; ox++)
			{
				for (int oy = -_gridSize.y / 2; oy <= _gridSize.y / 2; oy++)
				{
					float x = transform.position.x + (ox + _gridOffset.x) * _gridCellSize.x;
					float y = transform.position.y + (oy + _gridOffset.y) * _gridCellSize.y;
					Vector2 v = new Vector3(x, y);

					Collider2D[] hits = Physics2D.OverlapBoxAll(v, _gridCellSize, 0, LayerMask.GetMask("Powerups", "EnemyBullets", "Enemy"));

					Collider2D hit = hits.OrderByDescending(c => c.gameObject.layer).FirstOrDefault();
					float f = GetNeuronValue(hit);

					float x2 = transform.localPosition.x + (ox + _gridOffset.x) * _gridCellSize.x;
					float y2 = transform.localPosition.y + (oy + _gridOffset.y) * _gridCellSize.y;
					v = new Vector3(x2, y2, 0);

					if (Mathf.Abs(v.x) > 8.5f || Mathf.Abs(v.y) > 3f)
					{
						f = Mathf.Min(f, -0.25f);
					}

					Color c = new Color(f <= 0 ? -f : 0, f > 0 ? 1 : 0, 0);
					if (f == 0)
					{
						c = Color.white;
					}

					c.a = 0.25f;
					if(Mathf.Approximately(f, 0))
						c.b = 1 - Mathf.Clamp01(v.y / 3f);

					float p = new Vector3(v.x / 20, v.y / 6, 0).magnitude - 0.2f;
					c.g -= Mathf.Max(p, 0);
					c.b -= Mathf.Max(p, 0);
					//AddReward(-perUpdateScore * Mathf.Max(posOffset, 0) / 4, "position");

					Gizmos.color = c;

					Gizmos.DrawCube(new Vector3(x, y, transform.position.z), _gridCellSize * 0.9f);
				}
			}
			Gizmos.color = Color.white;
		}

		private List<(Vector3, Vector3, Color)> debugLines = new List<(Vector3, Vector3, Color)>();

		protected override void Awake()
		{
			base.Awake();
			prob = 0.5f;
			xpos = -4;
			if (!IsTraining)
			{
				GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;
			}
		}

		public long GetTotalSteps()
		{
			return totalSteps;
		}

		[UsedImplicitly]
		public override void OnEpisodeBegin()
		{
			if (gameContainer == null)
			{
				StartCoroutine(TryAgain());
				return;
			}
			base.OnEpisodeBegin();

			if (drawDebug) Debug.Log($"Total: {awards.Select(kvp => kvp.Value).Sum():F3} | " + string.Join(", ", awards.OrderByDescending(kvp => Mathf.Abs(kvp.Value)).Where(kvp => Mathf.Abs(kvp.Value) > 0.001).Select(kvp => $"{kvp.Key}: {kvp.Value:F3}")));
			debugLines.Clear();

			awards.Clear();

			int leftRight = Random.value > 0.5f ? 1 : -1;
			//bool spawnGems = true;// Random.value < 0.75f;
			//bool spawnShots = true;//Random.value < 0.75f;

			transform.localPosition = new Vector3(Random.value * 6 * -leftRight + 2 * -leftRight, Random.value * 4 - 2, 0);

			if (!IsTraining) return;

			gameContainer.Clear();

			int num = Mathf.FloorToInt(Random.value * (4 + totalSteps/500_000f)) + 2;
			if (totalSteps > 9_000_000)
				num = 0;

			for (int i = 0; i < num; i++)
			{
				GameObject go = Instantiate(gemPrefab, gameContainer);
				Vector3 p;
				do
				{
					p = new Vector3((Random.value * (3.5f + Mathf.Clamp(xpos / 10f, 0, 3)) + Mathf.Clamp(xpos / 2 - 2, -5, 3)) * leftRight, Random.value * 3 + (totalSteps > 1_500_000 ? 2 : 0), 0);
				} while (Vector3.Distance(p, transform.localPosition) < 2.0f);
				go.transform.localPosition = p;
				go.GetComponent<Magnet>().enabled = totalSteps > 3_000_000;
			}
			num = Mathf.FloorToInt(Random.value * 6 + Mathf.FloorToInt(totalSteps / 500_000f)) + 2 + Mathf.FloorToInt(totalSteps / 3_000_000f);
			for (int i = 0; i < num; i++)
			{
				GameObject go = Instantiate(bulletPrefab, gameContainer);
				Vector3 p;
				do
				{
					p = new Vector3(Random.value * 17 - 8.5f, Random.value * 6 - 3, 0);
				} while (Vector3.Distance(p, transform.localPosition) < 1.5f);
				go.transform.localPosition = p;
				go.transform.localScale = Vector3.one * ((Random.value / 2) + 0.5f);
			}
			/*for (int i = 0; i < 5; i++)
			{
				float x = (Random.value * 0.25f) + ((i+2) * 3.25f);
				float y = Random.value * 4.5f - 2.25f;
				GenerateVerticalBar(x, y, bulletPrefab);
			}*/
			totalSteps += MaxStep;
			prevMove = Vector2.zero;
			MaxStep = 5_000;
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
			w.a = 0.1f;
			sensor.AddObservation(transform.localPosition.x / 20);
			sensor.AddObservation(transform.localPosition.y / 6);

			sensor.AddObservation(prevMove.x);
			sensor.AddObservation(prevMove.y);

			float d = Mathf.Abs((transform.localPosition.x - -8.5f) / 10f);
			sensor.AddObservation(Mathf.Min(d, 1));
			d = Mathf.Abs((transform.localPosition.x - 8.5f) / 10f);
			sensor.AddObservation(Mathf.Min(d, 1));

			d = Mathf.Abs((transform.localPosition.y - -3f) / 10f);
			sensor.AddObservation(Mathf.Min(d, 1));
			d = Mathf.Abs((transform.localPosition.y - 3f) / 10f);
			sensor.AddObservation(Mathf.Min(d, 1));

			for (int ox = -_gridSize.x / 2; ox <= _gridSize.x / 2; ox++)
			{
				for (int oy = -_gridSize.y / 2; oy <= _gridSize.y / 2; oy++)
				{
					float x = transform.position.x + (ox + _gridOffset.x) * _gridCellSize.x;
					float y = transform.position.y + (oy + _gridOffset.y) * _gridCellSize.y;
					Vector2 v = new Vector3(x, y);

					Collider2D[] hits = Physics2D.OverlapBoxAll(v, _gridCellSize, 0, LayerMask.GetMask("Powerups", "EnemyBullets", "Enemy"));

					Collider2D hit = hits.OrderByDescending(c => c.gameObject.layer).FirstOrDefault();
					float f = GetNeuronValue(hit);

					float x2 = transform.localPosition.x + (ox + _gridOffset.x) * _gridCellSize.x;
					float y2 = transform.localPosition.y + (oy + _gridOffset.y) * _gridCellSize.y;
					v = new Vector3(x2, y2, 0);

					if (Mathf.Abs(v.x) > 8.5f || Mathf.Abs(v.y) > 3f)
					{
						f = Mathf.Min(f, -0.25f);
					}

					sensor.AddObservation(f);
				}
			}
		}

		private float GetNeuronValue(Collider2D hit)
		{
			if (hit == null) return 0;
			switch (LayerMask.LayerToName(hit.gameObject.layer))
			{
				case "Powerups":
					return 1;
				case "EnemyBullets":
					return -1;
				case "Enemy":
					return -0.5f;
				default:
					return 0;
			}
		}

		private Vector2 prevMove = Vector2.zero;
		private float prevSpeed = 0;
		private const float perUpdateScore = 0.005f;
		private Dictionary<string, float> awards = new Dictionary<string, float>();

		public void AddReward(float amt, string reason)
		{
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
			if(deterministicDebug)
				foreach ((Vector3 a, Vector3 b, Color c) in debugLines)
				{
					Debug.DrawLine(a, b, c, 0.02f);
				}

			/*Vector2 controlSignal = Vector2.zero;
			controlSignal.x = actionBuffers.ContinuousActions[0];
			controlSignal.y = actionBuffers.ContinuousActions[1];
			float mag = Mathf.Clamp01(controlSignal.magnitude);
			controlSignal = controlSignal.normalized * mag;
			
			float d = Vector2.SignedAngle(prevMove.normalized, controlSignal / mag);
			d /= 180;
			if (Mathf.Abs(d) > float.Epsilon)
				AddReward(-perUpdateScore * d * d * 100, "turning");

			float accl = Mathf.Clamp01(Mathf.Abs(prevMove.magnitude - mag) + 0.01f);
			AddReward(-perUpdateScore * accl * accl * 100, "accel");
			AddReward(-perUpdateScore * Mathf.Max(Mathf.Pow(mag, 1f / 12f) - 0.5f, 0) * 10, "speed");

			Vector3 mv = new Vector3(controlSignal.x, controlSignal.y, 0) * Time.fixedDeltaTime * 3;*/

			Vector2 moveDir = new Vector2(actionBuffers.ContinuousActions[0], actionBuffers.ContinuousActions[1]);
			float speed = (actionBuffers.ContinuousActions[2] + 1f) / 2f;
			Vector2 convertedInput = moveDir.normalized;
			Vector3 mv = convertedInput * speed * Time.fixedDeltaTime * 3;

			float d = Vector2.SignedAngle(prevMove, convertedInput);
			d /= 180;

			Color col = new Color(Mathf.Abs(d), 1 - Mathf.Abs(d), 0);
			if (Mathf.Abs(d) > float.Epsilon)
				AddReward(-perUpdateScore * d * d * 100, "turning");

			float accl = prevSpeed - speed;
			float m = Mathf.Min(accl, accl / 4);
			AddReward(-perUpdateScore * accl * m * 100, "accel");
			AddReward(-perUpdateScore * speed, "speed");

			if (drawDebug) Debug.DrawLine(transform.position + new Vector3(prevMove.x, prevMove.y, 0).normalized, transform.position + mv.normalized, col, 0.02f);

			if (drawDebug || deterministicDebug)
			{
				col = new Color(Mathf.Abs(accl), 1 - Mathf.Abs(accl), 0);
				Debug.DrawLine(
					Vector3.up * 4 + (Vector3.right * (Time.time % 1)) + Vector3.right,
					Vector3.up * 4 + Vector3.up * Mathf.Clamp(accl, -1, 1) * 2 + Vector3.right * (Time.time % 1) + Vector3.right,
					col, 1);
				col = new Color(0, 1 - speed, speed);
				Debug.DrawLine(
					Vector3.up * 3 + Vector3.up * Mathf.Clamp01(prevSpeed) * 2 + Vector3.right * ((Time.time % 1) - Time.fixedDeltaTime) + Vector3.left * 2,
					Vector3.up * 3 + Vector3.up * Mathf.Clamp01(speed) * 2 + Vector3.right * (Time.time % 1) + Vector3.left * 2,
					col, 1);
			}

			Vector3 origPos = transform.position;

			if (doMovement)
			{
				transform.Translate(mv, Space.Self);

				if (deterministicDebug) debugLines.Add((origPos, transform.position, Color.white));
			}
			else if(deterministicDebug)
				Debug.DrawLine(origPos + mv.normalized, origPos + mv.normalized * 1.1f, Color.white, 0.02f);

			if (transform.localPosition.x < -8f || transform.localPosition.x > 8f || transform.localPosition.y < -2.5f || transform.localPosition.y > 2.5f)
			{
				Vector3 v = new Vector3(Mathf.Clamp(transform.localPosition.x, -8.5f, 8.5f), Mathf.Clamp(transform.localPosition.y, -3f, 3f), transform.localPosition.z);
				float dist = Vector3.Distance(v, transform.localPosition) / Time.fixedDeltaTime;
				if (dist > float.Epsilon)
				{
					AddReward(-perUpdateScore * Mathf.Clamp01(dist) * 30, "edge");
				}
				transform.localPosition = v;
			}

			AddReward(-perUpdateScore * Mathf.Clamp01(transform.localPosition.y / 3f), "gravity");

			prevMove = moveDir;
			prevSpeed = speed;

			float posOffset = new Vector3(transform.localPosition.x / 20, transform.localPosition.y / 6, 0).magnitude - 0.2f;
			AddReward(-perUpdateScore * Mathf.Max(posOffset, 0) / 4, "position");
		}

		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("Powerups"))
			{
				xpos += 0.1f;
				AddReward(0.75f, "gem collect");
				//SetReward(0.75f);
				//EndEpisode(); return;
				Destroy(other.gameObject);

				Vector3 p = other.gameObject.transform.position;
				if (deterministicDebug) debugLines.Add((p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.green));
				if (deterministicDebug) debugLines.Add((p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.green));

				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.green, 1.5f);
				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.green, 1.5f);

				//GameObject go = Instantiate(gemPrefab, gameContainer);
				//go.transform.localPosition = new Vector3((Random.value * 3.5f + 5) * -Mathf.Sign(transform.position.x), Random.value * 4 - 2, 0);
				//bonus += 1;
				//if (bonus >= 3)
				//	EndEpisode();
				//bonus = Mathf.Max(1, bonus+1);
				//MaxStep += 100;
			}
			if (other.gameObject.layer == LayerMask.NameToLayer("EnemyBullets"))
			{
				AddReward(-0.75f, "damaged");
				//EndEpisode(); return;
				Destroy(other.gameObject);

				Vector3 p = other.gameObject.transform.position;
				if (deterministicDebug) debugLines.Add((p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.red));
				if (deterministicDebug) debugLines.Add((p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.red));

				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.red, 1.5f);
				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.red, 1.5f);

				//GameObject go = Instantiate(bulletPrefab, gameContainer);
				//Vector3 p;
				//do
				//{
				//	p = new Vector3(Random.value * 17 - 8.5f, Random.value * 6 - 3, 0);
				//} while (Vector3.Distance(p, transform.localPosition) < 1.5f);
				//go.transform.localPosition = p;
				//go.transform.localScale = Vector3.one * ((Random.value / 2) + 0.5f);
				//bonus -= 1;
				//MaxStep /= 2;
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
	}
}
