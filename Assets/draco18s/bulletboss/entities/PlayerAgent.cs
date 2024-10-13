using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.draco18s.util;
using JetBrains.Annotations;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Sentis;
using Unity.Sentis.Layers;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Assets.draco18s.bulletboss.entities
{
	public class PlayerAgent : Agent
	{
		[SerializeField] private Player playerShip;
		[SerializeField] private GameObject gemPrefab;
		[SerializeField] private GameObject bulletPrefab;
		[SerializeField] private Transform gameContainer;
		[SerializeField] private bool drawDebug = false;
		[SerializeField] private bool deterministicDebug = false;
		[SerializeField] private bool doMovement = true;
		[SerializeField] private bool runInfiniteEpisode = false;

		private float xpos = -4;
		private float prob = 1.15f;
		private Vector3 oobPos = new Vector3(0, -100, 0);

		private int numGemsSpawned = -1;
		private int gemsCollected = -1;
		private float stunTimer = -1;
		[SerializeField] private long totalSteps = 552000;

		private List<(Vector3, Vector3, Color)> debugLines = new List<(Vector3, Vector3, Color)>();

		protected override void Awake()
		{
			base.Awake();
			prob = 0.5f;
			xpos = -4;
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

			if (drawDebug) Debug.Log($"Total: {awards.Select(kvp => kvp.Value).Sum():F3} | " + string.Join(", ", awards.OrderByDescending(kvp => Mathf.Abs(kvp.Value)).Select(kvp => $"{kvp.Key}: {kvp.Value:F3}")));
			debugLines.Clear();

			awards.Clear();

			int leftRight = Random.value > 0.5f ? 1 : -1;
			bool spawnGems = true;// Random.value < 0.75f;
			bool spawnShots = true;//Random.value < 0.75f;

			transform.localPosition = new Vector3(Random.value * 2 * -leftRight + 6 * -leftRight, Random.value * 4 - 2, 0);

			gameContainer.Clear();

			int num = Mathf.FloorToInt(Random.value * 4 + Mathf.FloorToInt(totalSteps/750000f)) + 1 + Mathf.FloorToInt(totalSteps / 1500000f);

			numGemsSpawned = spawnGems ? num : 0;
			gemsCollected = 0;

			for (int i = 0; i < num && spawnGems; i++)
			{
				GameObject go = Instantiate(gemPrefab, gameContainer);
				Vector3 p;
				do
				{
					p = new Vector3((Random.value * (3.5f + Mathf.Clamp(xpos / 10f, 0, 3)) + Mathf.Clamp(xpos / 2 - 2, -5, 3)) * leftRight, Random.value * 4, 0);
				} while (Vector3.Distance(p, transform.localPosition) < 2.0f);
				go.transform.localPosition = p;
			}
			num = Mathf.FloorToInt(Random.value * 6 + Mathf.FloorToInt((totalSteps-250000) / 500000f)) + 2 + Mathf.FloorToInt(totalSteps / 2000000f);
			for (int i = 0; i < num && spawnShots; i++)
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
			MaxStep = 3000;
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
			List<Collider2D> objs = new List<Collider2D>();
			foreach (Transform t in gameContainer)
			{
				Collider2D c = t.GetComponent<Collider2D>();
				if (c == null) continue;
				objs.Add(c);
			}

			foreach (Collider2D c in objs)
			{
				//c.gameObject.GetComponentInChildren<SpriteRenderer>().color = w;
			}

			sensor.AddObservation(prevMove.x);
			sensor.AddObservation(prevMove.y);

			float d = Mathf.Abs((transform.localPosition.x - -8.5f) / 10f);
			sensor.AddObservation(d);
			d = Mathf.Abs((transform.localPosition.x - 8.5f) / 10f);
			sensor.AddObservation(d);

			d = Mathf.Abs((transform.localPosition.y - -2.5f) / 10f);
			sensor.AddObservation(d);
			d = Mathf.Abs((transform.localPosition.y - 2.5f) / 10f);
			sensor.AddObservation(d);

			int l1 = LayerMask.NameToLayer("EnemyBullets");

			List<Collider2D> padObj = objs.Where(c => c.gameObject.layer == l1).ToList();

			IOrderedEnumerable<Collider2D> bul = padObj.PadRight(Mathf.Max(padObj.Count, 5)).OrderBy(a => a == null ? 100000 : Vector3.Distance(a.transform.localPosition.ReplaceZ(0), transform.localPosition.ReplaceZ(0)));

			//int q = 0;
			foreach (Collider2D b in bul.Take(3))
			{
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

				Vector3 v = (b.transform.localPosition - transform.localPosition).ReplaceZ(b.transform.localScale.x * colliderSize);

				float bSpeed = b.GetComponent<IHasSpeed>().Speed;

				//sensor.AddObservation(b.transform.localPosition.ReplaceZ(b.transform.localScale.x));
				sensor.AddObservation(v / 10f);
				sensor.AddObservation(b.transform.right * bSpeed * Time.fixedDeltaTime);
				b.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
				padObj.Remove(b);
			}

			for (int i = 0; i < 2; i++)
			{
				Collider2D b = padObj.GetRandom();
				if (b == null)
				{
					sensor.AddObservation(oobPos / 10f);
					sensor.AddObservation(Vector3.zero);
					continue;
				}
				float bSpeed = b.GetComponent<IHasSpeed>().Speed;

				Vector3 v = (b.transform.localPosition - transform.localPosition).ReplaceZ(b.transform.localScale.x);
				sensor.AddObservation(v / 10f);
				sensor.AddObservation(b.transform.right * bSpeed * Time.fixedDeltaTime);
				b.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
				padObj.Remove(b);
			}


			int l2 = LayerMask.NameToLayer("Powerups");
			padObj = objs.Where(c => c.gameObject.layer == l2).ToList();
			bul = padObj.PadRight(Mathf.Max(padObj.Count, 3)).OrderBy(a => a == null ? 100000 : Vector3.Distance(a.transform.position.ReplaceZ(0), transform.position.ReplaceZ(0)));
			foreach (Collider2D b in bul.Take(3))
			{
				if (b == null)
				{
					sensor.AddObservation(oobPos / 10f);
					continue;
				}

				Vector3 v = (b.transform.localPosition - transform.localPosition).ReplaceZ(1);
				//sensor.AddObservation(b.transform.localPosition.ReplaceZ(1));
				sensor.AddObservation(v / 10f);
				b.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
				padObj.Remove(b);
			}

			/*for (int i = 0; i < 2; i++)
			{
				Collider2D b = padObj.GetRandom();
				if (b == null)
				{
					sensor.AddObservation(oobPos);
					continue;
				}
				Vector3 v = (b.transform.localPosition - transform.localPosition).ReplaceZ(1);
				//sensor.AddObservation(b.transform.localPosition.ReplaceZ(1));
				sensor.AddObservation(v);
				b.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
				padObj.Remove(b);
			}*/
		}

		private Vector2 prevMove = Vector2.zero;
		private const float perUpdateScore = 0.005f;
		private float partialFrame = 0;
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

			Vector2 controlSignal = Vector2.zero;
			controlSignal.x = actionBuffers.ContinuousActions[0];
			controlSignal.y = actionBuffers.ContinuousActions[1];
			float mag = Mathf.Clamp01(controlSignal.magnitude);
			controlSignal = controlSignal.normalized * mag;
			
			float d = Vector2.SignedAngle(prevMove.normalized, controlSignal / mag);
			d /= 180;
			if (Mathf.Abs(d) > float.Epsilon)
				AddReward(-perUpdateScore * d * d * 100, "turning");

			float m = Mathf.Clamp01(Mathf.Abs(prevMove.magnitude - mag));
			AddReward(-perUpdateScore * m * m * 100, "speed");

			Vector3 mv = new Vector3(controlSignal.x, controlSignal.y, 0) * Time.fixedDeltaTime * 3;

			Color col = new Color(d, 1-d, 0);

			if(drawDebug) Debug.DrawLine(transform.position + new Vector3(prevMove.x, prevMove.y, 0).normalized, transform.position + mv.normalized, col, 0.02f);

			var obs = GetObservations().Skip(6).ToArray();
			
			//for (int j = 0; obs.Length > 0 && j < 3; j++)
			{
				int j = 0;
				int i = (5 * 6) + j * 3;
				Vector3 pos = new Vector3(obs[i], obs[i + 1], 0) * 10f;
				bool stop = false;
				if (obs[i + 2] < 0.001f)
				{
					stop = j == 0;
					pos = stop ? - transform.localPosition : Vector3.zero;
				}

				Vector3 op = pos;
				Vector3 np = pos - mv;
				Vector3 pp = pos - new Vector3(prevMove.x, prevMove.y, 0) * Time.fixedDeltaTime * 3;
				bool movedAway = false;
				if (np.sqrMagnitude > op.sqrMagnitude || np.sqrMagnitude > pp.sqrMagnitude)
				{
					col = new Color(stop ? mag : 1 - mag, stop ? 1 - mag : mag, 0, mag * mag * (3 - j) * 0.25f);
					movedAway = true;
					AddReward(-perUpdateScore * (stop ? (1 + mag) : mag) * (np.magnitude) * 2f * (3 - j) * 0.25f * (j == 0 ? 1 : 0.2f), "-center a");
				}
				else
				{
					col = new Color(stop ? mag : 1 - mag, stop ? 1 - mag : mag, 0, mag * mag * (3 - j) * 0.25f);
					AddReward(perUpdateScore * (stop ? (1 + mag) : mag) * 2 * (4 - j) * 0.25f, "+center a");
				}

				if (drawDebug) Debug.DrawLine(transform.position + mv, pos + transform.position, col, 0.02f);

				col = new Color((stop || movedAway) ? mag * mag : (1 - mag) * (1 - mag), (stop || movedAway) ? (1 - mag) * (1 - mag) : mag * mag, 0);
				if (drawDebug || deterministicDebug)
					Debug.DrawLine(Vector3.up + Vector3.left * 3 + Vector3.right * (Time.time % 1), Vector3.up + Vector3.up * mag * 2 + Vector3.left * 3 + Vector3.right * (Time.time % 1), col, 1);
			}

			/*for (int j = 0; obs.Length > 0 && j < 5; j++)
			{
				int i = j * 6;

				Vector3 pos = new Vector3(obs[i], obs[i + 1], 0) * 10f;
				Vector3 dir = new Vector3(obs[i + 3], obs[i + 4], 0);
				
				bool stop = false;
				if (obs[i + 2] < 0.001f)
				{
					stop = j == 0;
					pos = stop ? -transform.localPosition : Vector3.zero;
				}

				Vector3 op = pos;
				Vector3 np = pos - mv + dir;
				if ((op + dir).sqrMagnitude < 1)
				{
					if (np.sqrMagnitude < op.sqrMagnitude)
					{
						col = new Color(stop ? mag : 1 - mag, stop ? 1 - mag : mag, 0, mag * mag * (3 - j) * 0.25f);
						AddReward(-perUpdateScore * (stop ? (1 + mag) : mag) * (np.magnitude) * 2f * (3 - j) * 0.25f * (j == 0 ? 1 : 0.2f), "-center b");
					}
					else
					{
						col = new Color(stop ? mag : 1 - mag, stop ? 1 - mag : mag, 0, mag * mag * (3 - j) * 0.25f);
						AddReward(perUpdateScore * (stop ? (1 + mag) : mag) * 2 * (4 - j) * 0.25f, "+center b");
					}

					if (drawDebug) Debug.DrawLine(transform.position + mv, pos + transform.position, col, 0.02f);
				}
			}*/

			float acc = Mathf.Abs(mag - prevMove.magnitude);
			float sig = Mathf.Sign(mag - prevMove.magnitude);
			col = new Color(acc, 1 - acc, 0);
			if (drawDebug || deterministicDebug) 
				Debug.DrawLine(Vector3.up + Vector3.right * (Time.time % 1), Vector3.up + Vector3.up * Mathf.Clamp01(acc * 4 + 0.01f) * 2 * sig + Vector3.right * (Time.time % 1), col, 1);

			/*float d = Vector2.Angle(prevMove.normalized, controlSignal / mag);
			d /= 180;
			Color cc = new Color(1, 1 - Mathf.Sqrt(d), 0, Mathf.Sqrt(d));
			//if (d > float.Epsilon)
			//	AddReward(-perUpdateScore * d * d * 250, "alive");

			Vector3 mv = new Vector3(controlSignal.x, controlSignal.y, 0) * Time.fixedDeltaTime * 3;

			//Debug.DrawLine(transform.position + new Vector3(prevMove.x, prevMove.y, 0).normalized, transform.position + mv.normalized, Color.blue, 0.02f);
			//Debug.DrawLine(transform.position, transform.position + new Vector3(prevMove.x, prevMove.y, 0).normalized, cc, 0.02f);
			//Debug.DrawLine(transform.position, transform.position + mv.normalized, cc, 0.02f);

			float m = Mathf.Clamp01(Mathf.Abs(prevMove.magnitude - mag));
			AddReward(-perUpdateScore * m * m * 100, "speed");

			var obs = GetObservations().Skip(6).ToArray();
			bool movedTowardsAny = false;
			bool areGems = false;
			for (int j = 0; obs.Length > 0 && j < 3; j++)
			{
				int i = (5 * 6) + j * 3;
				Vector3 pos = new Vector3(obs[i], obs[i + 1], 0) * 10f;
				if (obs[i + 2] < 0.001f)
				{
					AddReward(perUpdateScore * 3.1f * (3 - j), "no gem proximity");
					continue;
				}
				areGems = true;
				float dist1 = (pos).magnitude;
				float dist2 = (pos - mv).magnitude;

				float sc = (dist1 - dist2) / dist2 * (3 - j);
				if (dist2 < dist1) // move closer to gem
				{
					AddReward(perUpdateScore * sc * sc * 0.3f, "gem proximity");
					movedTowardsAny = true;
				}

				Color col = (dist2 < dist1 ? Color.green : Color.clear);
				col.a = sc * 2;
				if (drawDebug) Debug.DrawLine(transform.position + mv, pos + transform.position, col, 0.02f);
			}

			//if (!movedTowardsAny && areGems)
			//{
				//AddReward(-perUpdateScore * 0.5f, "didn't gem move");
			//}
			
			bool areShots = false;
			for (int j = 0; obs.Length > 0 && j < 5; j++)
			{
				int i = j * 6;

				Vector3 pos = new Vector3(obs[i], obs[i + 1], 0) * 10f;
				Vector3 dir = new Vector3(obs[i + 3], obs[i + 4], 0);

				if (obs[i + 2] < 0.001f)
				{
					AddReward(-perUpdateScore * 0.025f * (5 - j), "no bullet");
					continue;
				}
				areShots = true;
				float dist1 = (pos).magnitude;
				float dist2 = (pos - mv).magnitude;

				// if original distance is less than 0.5 seconds worth of movement + combined radius
				if (j < 3 && dist2 < dist1) //getting closer to a bullet
				{
					float sc = dist1 - dist2;
					AddReward(-perUpdateScore * sc * sc * 0.05f * (1 + mag), "move near bullet");
					Color col = (dist2 < dist1 ? Color.red : Color.clear);
					col.a = sc;
					if (drawDebug) Debug.DrawLine(transform.position + mv, pos + dir + gameContainer.position + transform.localPosition, col, 0.02f);
				}

				if (dir.magnitude < 0.001f)
				{
					//AddReward(-perUpdateScore * 0.025f, "stationary bullet");
					continue;
				}

				RaycastHit2D hit = Physics2D.CircleCast(pos + gameContainer.position + transform.localPosition, obs[i + 2] + 0.15f, dir, dir.magnitude * 40, LayerMask.GetMask("AIPlayer"));

				if (hit.collider != null) // being in the path of the bullet
				{
					float sc = Mathf.Clamp(4 / hit.distance, 0, 8);
					AddReward(-perUpdateScore * sc * (1 + mag), "in bullet path");
					Color col = Color.magenta;
					col.a = sc;
					if (drawDebug) Debug.DrawLine(pos + dir + gameContainer.position + transform.localPosition, hit.point, col, 0.02f);
				}
			}

			//if (!areShots && !areGems)
			{
				// try increasing penalty for fast and/or increasing reward for slow
				// try making this integrate with the "distance from center" code
				//AddReward(perUpdateScore * (mag > 0.2f ? -mag * 5 : 0.2f / Mathf.Max(mag, 0.04f)), "centering a");
				Color col = (mag > 0.3f ? (mag > 0.6f ? Color.red : Color.yellow) : Color.green);
				//if (drawDebug) Debug.DrawLine(Vector3.up + Vector3.right * (Time.time % 1), Vector3.up + Vector3.up * mag * 2 + Vector3.right * (Time.time % 1), col, 1);
				if (drawDebug || deterministicDebug) Debug.DrawLine(Vector3.up + Vector3.right * (Time.time % 1), Vector3.up + Vector3.up * (mag - prevMove.magnitude) * 2 + Vector3.right * (Time.time % 1), col, 1);
			}

			if (!areGems)
			{
				Vector3 p = transform.localPosition;
				p = p.ReplaceY(p.y * 3.4f).ReplaceZ(0);
				float sl = .2f / (mag + .2f);
				float sc = (6f - p.magnitude) / 6f * Mathf.Clamp01((StepCount - 250) / 1000f) * sl;
				float dot = Vector3.Dot(mv, transform.localPosition.ReplaceZ(0));
				if (sc < 0)
				{
					sc = -Mathf.Abs(sc) * Mathf.Sign(dot);
				}
				Color col = sc <= 0 ? Color.red : Color.green;
				col.a = sc < 0 ? 0 - sc : sc;
				if (drawDebug) Debug.DrawLine(gameContainer.position, transform.position, col, 0.02f);
				AddReward(perUpdateScore * Mathf.Clamp(sc, -1, 0.5f) * 10 * (!areShots ? 1.5f : 1), "centering");
			}

			// mag*20 is definitely too much
			// reaches into adjacent environments
			RaycastHit2D hit_bul = Physics2D.CircleCast(transform.position, 0.3f, controlSignal, mag * 2, LayerMask.GetMask("EnemyBullets"));
			if (hit_bul.collider != null && hit_bul.collider.transform.parent == gameContainer)
			{
				float sc = Mathf.Clamp(1 / hit_bul.distance, 0, 4);
				AddReward(-perUpdateScore * sc * 0.5f * (1 + mag), "bullet in path");
				if (drawDebug) Debug.DrawLine(hit_bul.point, transform.position, Color.red, 0.02f);
			}
			else if (!areShots)
			{
				AddReward(-perUpdateScore, "no shots (path)");
			}
			RaycastHit2D hit_gem = Physics2D.CircleCast(transform.position, 0.3f, controlSignal, mag * 2, LayerMask.GetMask("Powerups"));
			if (hit_gem.collider != null && hit_gem.collider.transform.parent == gameContainer)
			{
				float sc = Mathf.Clamp(1 / hit_gem.distance, 0, 4);
				AddReward(perUpdateScore * sc * 0.5f * mag, "gem in path");
				if (drawDebug) Debug.DrawLine(hit_gem.point, transform.position, Color.cyan, 0.02f);
			}
			else if (!areGems)
			{
				AddReward(perUpdateScore, "no gems (path)");
			}

			RaycastHit2D hit3 = Physics2D.CircleCast(transform.position, 0.3f, prevMove, mag * 2, LayerMask.GetMask("Powerups", "EnemyBullets"));
			if (hit3.collider != null && hit3.collider.transform.parent == gameContainer)
			{
				float sc = Mathf.Clamp(1 / (hit3.distance * 10), 0, 4);
				if (hit3.transform.gameObject.layer == LayerMask.NameToLayer("Powerups"))
				{
					AddReward(perUpdateScore * sc * mag * (hit_gem.collider != null && hit_gem.collider.transform.parent == gameContainer ? 1 : -1), "gem in prevpath");
					if (drawDebug) Debug.DrawLine(hit3.point, transform.position, (hit_gem.collider != null && hit_gem.collider.transform.parent == gameContainer ? Color.magenta : Color.cyan), 0.02f);
				}
				if (hit3.transform.gameObject.layer == LayerMask.NameToLayer("EnemyBullets"))
				{
					AddReward(perUpdateScore * sc * mag * 0.4f * (hit_bul.collider != null && hit_bul.collider.transform.parent == gameContainer ? -1 : 1), "bullet in prepath");
					if (drawDebug) Debug.DrawLine(hit3.point, transform.position, (hit_bul.collider != null && hit_bul.collider.transform.parent == gameContainer ? Color.cyan : Color.magenta), 0.02f);
				}
			}*/

			Vector3 origPos = transform.position;

			if (doMovement)
			{
				/*if (stunTimer > 0)
					stunTimer -= Time.fixedDeltaTime;
				if (stunTimer <= 0)*/
					transform.Translate(mv, Space.Self);

				if (deterministicDebug) debugLines.Add((origPos, transform.position, Color.white));
			}
			else if(deterministicDebug)
				Debug.DrawLine(origPos + mv.normalized, origPos + mv.normalized * 1.1f, Color.white, 0.02f);

			if (transform.localPosition.x < -8f || transform.localPosition.x > 8f || transform.localPosition.y < -2f || transform.localPosition.y > 2f)
			{
				transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, -8.5f, 8.5f), Mathf.Clamp(transform.localPosition.y, -2.5f, 2.5f), transform.localPosition.z);

				/*Vector2 pos = new Vector2(transform.localPosition.x, transform.localPosition.y).normalized;
				float d1 = Vector2.Angle(pos, controlSignal / mag);
				d1 -= 15;
				d1 /= 180;

				if (Mathf.Abs(d1) > float.Epsilon)
					AddReward(perUpdateScore * d1 * d1 * 20f * Mathf.Sign(d1), "u-turning");
				cc.b = cc.g;
				cc.g = 1;
				cc.r = 0;
				if (drawDebug) Debug.DrawLine(origPos + new Vector3(prevMove.x, prevMove.y, 0).normalized, origPos + mv.normalized, cc, 0.02f);*/
			}
			else
			{
				//if (Mathf.Abs(d) > float.Epsilon)
				//	AddReward(-perUpdateScore * d * d * 20f, "turning");
				//if(numGemsSpawned > 0)
				//	AddReward(-perUpdateScore * 10f, "alive");

				//if (drawDebug) Debug.DrawLine(origPos + new Vector3(prevMove.x, prevMove.y, 0).normalized, origPos + mv.normalized, cc, 0.02f);
				//Debug.DrawLine(transform.position, transform.position + new Vector3(prevMove.x, prevMove.y, 0).normalized, cc, 0.02f);
				//Debug.DrawLine(transform.position, transform.position + mv.normalized, cc, 0.02f);
			}

			prevMove = controlSignal;

			/*Color col2 = Color.yellow;

			float s = 1 - Mathf.Clamp(Mathf.Abs(transform.localPosition.x - -8.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f * (controlSignal.x < 0.05 ? (1+mag * 2) : -0.75f), "-X edge proximity");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.left * 1, (s < 0.001 ? Color.clear : (controlSignal.x < 0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(transform.localPosition.x - 8.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f * (controlSignal.x > -0.05 ? (1 + mag * 2) : -0.75f), "+X edge proximity");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.right * 1, (s < 0.001 ? Color.clear : (controlSignal.x > -0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(transform.localPosition.y - -2.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f * (controlSignal.y < 0.05 ? (1 + mag * 2) : -0.75f), "-Y edge proximity");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.down * 1, (s < 0.001 ? Color.clear : (controlSignal.y < 0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(transform.localPosition.y - 2.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f * (controlSignal.y > -0.05 ? (1 + mag * 2) : -0.75f), "+Y edge proximity");
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.up * 1, (s < 0.001 ? Color.clear : (controlSignal.y > -0.05 ? Color.red : col2)), 0.02f);*/
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
