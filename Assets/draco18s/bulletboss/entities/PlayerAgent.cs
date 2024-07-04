using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.draco18s.util;
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

		private float xpos = -4;
		private float prob = 1.15f;
		private Vector3 oobPos = new Vector3(0, -100, 0);

		protected override void Awake()
		{
			base.Awake();
			prob = 0.5f;
			xpos = -4;
		}

		public override void OnEpisodeBegin()
		{
			base.OnEpisodeBegin();

			int leftRight = Random.value > 0.5f ? 1 : -1;
			bool spawnGems = Random.value < 0.5f;
			bool spawnShots = Random.value < 0.5f;

			transform.localPosition = new Vector3(Random.value * 2 * -leftRight + 6 * -leftRight, Random.value * 4 - 2, 0);

			gameContainer.Clear();

			int num = prob > 0.999f ? 8 : Mathf.RoundToInt(Random.value * 8 * (1 - prob) + prob * 8);

			for (int i = 0; i < 8 && spawnGems; i++)
			{
				GameObject go = Instantiate(gemPrefab, gameContainer);
				Vector3 p;
				do
				{
					p = new Vector3((Random.value * (3.5f + Mathf.Clamp(xpos / 10f, 0, 3)) + Mathf.Clamp(xpos / 2 - 2, -5, 3)) * leftRight, Random.value * 4 - 2, 0);
				} while (Vector3.Distance(p, transform.localPosition) < 2.0f);
				go.transform.localPosition = p;
			}
			num = prob > 0.999f ? 16 : Mathf.RoundToInt(Random.value * 16 * (1 - prob) + prob * 16);
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
			prevMove = Vector2.zero;
			//MaxStep = 20000;
		}

		private void GenerateVerticalBar(float _x, float _y, GameObject prefab)
		{
			for (int i = 0; i < 15; i++)
			{
				float y = i * 0.5f - 3;

				if(Mathf.Abs(_y - y) <= 1f) continue;

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
				if(c == null) continue;
				objs.Add(c);
			}

			foreach (Collider2D c in objs)
			{
				c.gameObject.GetComponentInChildren<SpriteRenderer>().color = w;
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
			bul = bul = padObj.PadRight(Mathf.Max(padObj.Count, 3)).OrderBy(a => a == null ? 100000 : Vector3.Distance(a.transform.localPosition.ReplaceZ(0), transform.localPosition.ReplaceZ(0)));
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


		public override void OnActionReceived(ActionBuffers actionBuffers)
		{
			Vector2 controlSignal = Vector2.zero;
			controlSignal.x = actionBuffers.ContinuousActions[0];
			controlSignal.y = actionBuffers.ContinuousActions[1];
			float mag = Mathf.Clamp01(controlSignal.magnitude);
			controlSignal = controlSignal.normalized * mag;

			float d = Vector2.Angle(prevMove.normalized, controlSignal / mag);
			d /= 180;
			Color cc = new Color(1, 1-Mathf.Sqrt(d), 0, Mathf.Sqrt(d));
			if(d > float.Epsilon)
				AddReward(-perUpdateScore * d * d * 250);

			Vector3 mv = new Vector3(controlSignal.x, controlSignal.y, 0) * Time.fixedDeltaTime * 3;

			Debug.DrawLine(transform.position + new Vector3(prevMove.x, prevMove.y, 0).normalized, transform.position + mv.normalized, cc, 0.02f);
			//Debug.DrawLine(transform.position, transform.position + new Vector3(prevMove.x, prevMove.y, 0).normalized, cc, 0.02f);
			//Debug.DrawLine(transform.position, transform.position + mv.normalized, cc, 0.02f);

			prevMove = controlSignal;

			var obs = GetObservations().Skip(6).ToArray();
			bool movedTowardsAny = false;
			bool areGems = false;
			for (int j = 0; obs.Length > 0 && j < 3; j++)
			{
				int i = (5*6) + j * 3;
				Vector3 pos = new Vector3(obs[i], obs[i + 1], 0) * 10f;
				if (obs[i + 2] < 0.001f) continue;
				areGems = true;
				float dist1 = (pos).magnitude;
				float dist2 = (pos - mv).magnitude;

				float sc = (dist1 - dist2) / dist2 * (j == 0 ? 2.5f : 0.01f);
				if (dist2 < dist1) /* move closer to gem */
				{
					AddReward(perUpdateScore * sc * sc * 0.3f);
					movedTowardsAny = true;
				}

				Color col = (dist2 < dist1 ? Color.green : Color.clear);
				col.a = sc * 2;
				if (drawDebug) Debug.DrawLine(transform.position + mv, pos + transform.position, col, 0.02f);
			}

			if (!movedTowardsAny && areGems)
			{
				AddReward(-perUpdateScore * 5);
			}

			if (areGems)
			{
				if (mag < 0.2f)
					AddReward(-perUpdateScore * 5);
				Color col = (mag < 0.2f ? Color.red : Color.yellow);
				if (drawDebug) Debug.DrawLine(Vector3.up + Vector3.right * (Time.time % 1), Vector3.up + Vector3.up * mag * 2 + Vector3.right * (Time.time % 1), col, 1);
			}

			bool areShots = false;
			for (int j = 0; obs.Length > 0 && j < 5; j++)
			{
				int i = j * 6;

				Vector3 pos = new Vector3(obs[i], obs[i + 1], 0) * 10f;
				Vector3 dir = new Vector3(obs[i + 3], obs[i + 4], 0);
				
				if (obs[i + 2] < 0.001f) continue;
				areShots = true;
				float dist1 = (pos).magnitude;
				float dist2 = (pos - mv).magnitude;

				// if original distance is less than 0.5 seconds worth of movement + combined radius
				if (j < 3 && dist2 < dist1 && dist1 < 3) /*getting closer to a bullet*/
				{
					float sc = dist1 - dist2;
					AddReward(-perUpdateScore * sc * sc * 0.025f);
					Color col = (dist2 < dist1 ? Color.red : Color.clear);
					col.a = sc;
					if (drawDebug) Debug.DrawLine(transform.position + mv, pos + dir + gameContainer.position + transform.localPosition, col, 0.02f);
				}

				if (dir.magnitude < 0.001f) continue;

				RaycastHit2D hit = Physics2D.CircleCast(pos + gameContainer.position + transform.localPosition, obs[i + 2] + 0.15f, dir, dir.magnitude * 40, LayerMask.GetMask("AIPlayer"));
				
				if (hit.collider != null /*being in the path of the bullet*/)
				{
					float sc = Mathf.Clamp(4 / hit.distance, 0, 8);
					AddReward(-perUpdateScore * sc * 0.5f);
					Color col = Color.magenta;
					col.a = sc;
					if (drawDebug) Debug.DrawLine(pos + dir + gameContainer.position + transform.localPosition, hit.point, col, 0.02f);
				}
			}

			if (!areShots && !areGems)
			{
				// try increasing penalty for fast and/or increasing reward for slow
				// try making this integrate with the "distance from center" code
				AddReward(perUpdateScore * (mag > 0.2f ? -mag*5 : 0.2f / Mathf.Max(mag, 0.04f)));
				Color col = (mag > 0.3f ? (mag > 0.6f ? Color.red : Color.yellow) : Color.green);
				if (drawDebug) Debug.DrawLine(Vector3.up + Vector3.right * (Time.time % 1), Vector3.up + Vector3.up * mag * 2 + Vector3.right * (Time.time % 1), col, 1);
			}

			if (!areGems)
			{
				Vector3 p = transform.localPosition;
				p = p.ReplaceY(p.y * 3.4f).ReplaceZ(0);
				float sc = (6f - p.magnitude) / 6f * Mathf.Clamp01((StepCount - 250) / 1000f);
				float dot = Vector3.Dot(mv, transform.localPosition.ReplaceZ(0));
				if (sc < 0)
				{
					sc = -Mathf.Abs(sc) * Mathf.Sign(dot);
				}
				Color col = sc <= 0 ? Color.red : Color.green;
				col.a = sc < 0 ? 0 - sc : sc;
				if(drawDebug) Debug.DrawLine(gameContainer.position, transform.position, col, 0.02f);
				AddReward(perUpdateScore * Mathf.Clamp(sc, -1, 0.5f) * 10 * (!areShots ? 1.5f : 1));
			}

			// mag*20 is definitely too much
			// reaches into adjacent environments
			RaycastHit2D hit_bul = Physics2D.CircleCast(transform.position, 0.3f, controlSignal, mag * 2, LayerMask.GetMask("EnemyBullets"));
			if (hit_bul.collider != null && hit_bul.collider.transform.parent == gameContainer)
			{
				float sc = Mathf.Clamp(1 / hit_bul.distance, 0, 4);
				AddReward(-perUpdateScore * sc * 0.5f * mag);
				if (drawDebug) Debug.DrawLine(hit_bul.point, transform.position, Color.red, 0.02f);
			}
			RaycastHit2D hit_gem = Physics2D.CircleCast(transform.position, 0.3f, controlSignal, mag * 2, LayerMask.GetMask("Powerups"));
			if (hit_gem.collider != null && hit_gem.collider.transform.parent == gameContainer)
			{
				float sc = Mathf.Clamp(1 / hit_gem.distance, 0, 4);
				AddReward(perUpdateScore * sc * 0.5f * mag);
				if (drawDebug) Debug.DrawLine(hit_gem.point, transform.position, Color.cyan, 0.02f);
			}

			RaycastHit2D hit3 = Physics2D.CircleCast(transform.position, 0.3f, prevMove, mag * 2, LayerMask.GetMask("Powerups", "EnemyBullets"));
			if (hit3.collider != null && hit3.collider.transform.parent == gameContainer)
			{
				float sc = Mathf.Clamp(1 / (hit3.distance * 10), 0, 4);
				if (hit3.transform.gameObject.layer == LayerMask.NameToLayer("Powerups"))
				{
					AddReward(perUpdateScore * sc * mag * (hit_gem.collider != null && hit_gem.collider.transform.parent == gameContainer ? 1 : -1));
					if (drawDebug) Debug.DrawLine(hit3.point, transform.position, (hit_gem.collider != null && hit_gem.collider.transform.parent == gameContainer ? Color.magenta : Color.cyan), 0.02f);
				}
				if (hit3.transform.gameObject.layer == LayerMask.NameToLayer("EnemyBullets"))
				{
					AddReward(perUpdateScore * sc * mag * 0.4f * (hit_bul.collider != null && hit_bul.collider.transform.parent == gameContainer ? -1 : 1));
					if (drawDebug) Debug.DrawLine(hit3.point, transform.position, (hit_bul.collider != null && hit_bul.collider.transform.parent == gameContainer ? Color.cyan : Color.magenta), 0.02f);
				}
			}

			transform.Translate(mv, Space.Self);

			if (transform.localPosition.x < -8.5f || transform.localPosition.x > 8.5f || transform.localPosition.y < -2.5f || transform.localPosition.y > 2.5f)
			{
				transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, -8.5f, 8.5f), Mathf.Clamp(transform.localPosition.y, -2.5f, 2.5f), transform.localPosition.z);
			}
			Color col2 = Color.yellow;

			float s = 1 - Mathf.Clamp(Mathf.Abs(transform.localPosition.x - -8.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f * (controlSignal.x < 0.05 ? 2 : -0.75f));
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.left * 1, (s < 0.001 ? Color.clear : (controlSignal.x < 0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(transform.localPosition.x - 8.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f * (controlSignal.x > -0.05 ? 2 : -0.75f));
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.right * 1, (s < 0.001 ? Color.clear : (controlSignal.x > -0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(transform.localPosition.y - -2.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f * (controlSignal.y < 0.05 ? 2 : -0.75f));
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.down * 1, (s < 0.001 ? Color.clear : (controlSignal.y < 0.05 ? Color.red : col2)), 0.02f);

			s = 1 - Mathf.Clamp(Mathf.Abs(transform.localPosition.y - 2.5f), 0, 1);
			col2.a = s * s;
			if (s > 0)
				AddReward(-perUpdateScore * s * s * 2.5f * (controlSignal.y > -0.05 ? 2 : -0.75f));
			if (drawDebug) Debug.DrawLine(transform.position, transform.position + Vector3.up * 1, (s < 0.001 ? Color.clear : (controlSignal.y > -0.05 ? Color.red : col2)), 0.02f);
		}

		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("Powerups"))
			{
				xpos += 0.1f;
				//GameManager.instance.gemsCount++;
				//GameManager.instance.gemsTxt.text = GameManager.instance.gemsCount.ToString();
				AddReward(0.75f);
				//EndEpisode(); return;
				Destroy(other.gameObject);

				GameObject go = Instantiate(gemPrefab, gameContainer);
				go.transform.localPosition = new Vector3((Random.value * 3.5f + 5) * -Mathf.Sign(transform.position.x), Random.value * 4 - 2, 0);
				//bonus += 1;
				//if (bonus >= 3)
				//	EndEpisode();
				//bonus = Mathf.Max(1, bonus+1);
				//MaxStep += 100;
			}
			if (other.gameObject.layer == LayerMask.NameToLayer("EnemyBullets"))
			{
				//GameManager.instance.hitsCount++;
				//GameManager.instance.hitsTxt.text = GameManager.instance.hitsCount.ToString();
				AddReward(-0.75f);
				//EndEpisode(); return;
				Destroy(other.gameObject);

				GameObject go = Instantiate(bulletPrefab, gameContainer);
				Vector3 p;
				do
				{
					p = new Vector3(Random.value * 17 - 8.5f, Random.value * 6 - 3, 0);
				} while (Vector3.Distance(p, transform.localPosition) < 1.5f);
				go.transform.localPosition = p; 
				go.transform.localScale = Vector3.one * ((Random.value / 2) + 0.5f);
				//bonus -= 1;
				//MaxStep /= 2;
			}
		}

		static float Cross(Vector3 point1, Vector3 point2)
		{
			//we don't care about z
			return point1.x * point2.y - point1.y * point2.x;
		}
	}
}
