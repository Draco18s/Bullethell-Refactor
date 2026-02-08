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
	public class PlayerAgent : Agent
	{
		[SerializeField] private GameObject gemPrefab;
		[SerializeField] private GameObject bulletPrefab;
		[SerializeField] private Transform gameContainer;
		[SerializeField] private RenderTexture renderTexture;
		private Texture2D viewTexture;
		[SerializeField] private bool drawDebug = false;
		[SerializeField] private bool deterministicDebug = false;
		[SerializeField] private bool doMovement = true;
		[SerializeField] private bool runInfiniteEpisode = false;

		private Vector3 oobPos = new Vector3(0, -100, 0);

		[SerializeField] private long totalSteps = 0;

		private List<(Vector3, Vector3, Color)> debugLines = new List<(Vector3, Vector3, Color)>();

		protected override void Awake()
		{
			base.Awake();
			viewTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
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
			gameContainer.Clear();

			transform.localPosition = new Vector3(Random.value * 16 - 8, Random.value * 4 - 2, 0);

			int num = Mathf.FloorToInt(Random.value * 6 + Mathf.FloorToInt((totalSteps - 250_000) / 500_000f)) + 2 + Mathf.FloorToInt(totalSteps / 2_000_000f);
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
			for (int i = 0; i < 4; i++)
			{
				GameObject go = Instantiate(gemPrefab, gameContainer);
				Vector3 p;
				do
				{
					p = new Vector3(Random.value * 17 - 8.5f, Random.value * 6 - 3, 0);
				} while (Vector3.Distance(p, transform.localPosition) < 1.5f);
				go.transform.localPosition = p;
				go.transform.localScale = Vector3.one * ((Random.value / 2) + 0.5f);

				if (totalSteps < 2_000_000)
				{
					go.GetComponent<Magnet>().enabled = false;
				}
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
			// local pos
			sensor.AddObservation(transform.localPosition.x / 10f);
			sensor.AddObservation(transform.localPosition.y / 10f);

			RenderTexture.active = renderTexture;

			Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
			viewTexture.ReadPixels(rect, 0, 0);
			viewTexture.Apply();

			Color[] pix = viewTexture.GetPixels();
			for (int x = 0; x < viewTexture.height; x++)
			{
				for (int y = 0; y < viewTexture.width; y++)
				{
					Color c = pix[x * viewTexture.height + y];

					sensor.AddObservation(c.r);
					sensor.AddObservation(c.g);
					sensor.AddObservation(c.b);
				}
			}
			RenderTexture.active = null;
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
			Vector2 controlSignal = Vector2.zero;
			controlSignal.x = actionBuffers.ContinuousActions[0];
			controlSignal.y = actionBuffers.ContinuousActions[1];
			float mag = Mathf.Clamp01(controlSignal.magnitude);
			controlSignal = controlSignal.normalized * mag;
			Vector3 mv = new Vector3(controlSignal.x, controlSignal.y, 0) * Time.fixedDeltaTime * 3;

			Vector3 origPos = transform.position;

			if (doMovement)
			{
				transform.Translate(mv, Space.Self);

				if (deterministicDebug) debugLines.Add((origPos, transform.position, Color.white));
			}
			else if (deterministicDebug)
				Debug.DrawLine(origPos + mv.normalized, origPos + mv.normalized * 1.1f, Color.white, 0.02f);

			if (transform.localPosition.x < -8f || transform.localPosition.x > 8f || transform.localPosition.y < -2f || transform.localPosition.y > 2f)
			{
				transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, -8.5f, 8.5f), Mathf.Clamp(transform.localPosition.y, -2.5f, 2.5f), transform.localPosition.z);
			}
		}

		void OnTriggerEnter2D(Collider2D other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("Powerups"))
			{
				AddReward(0.75f, "gem collect");
				Destroy(other.gameObject);

				Vector3 p = other.gameObject.transform.position;
				if (deterministicDebug) debugLines.Add((p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.green));
				if (deterministicDebug) debugLines.Add((p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.green));

				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.green, 1.5f);
				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.green, 1.5f);
			}
			if (other.gameObject.layer == LayerMask.NameToLayer("EnemyBullets"))
			{
				AddReward(-0.75f, "damaged");
				Destroy(other.gameObject);

				Vector3 p = other.gameObject.transform.position;
				if (deterministicDebug) debugLines.Add((p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.red));
				if (deterministicDebug) debugLines.Add((p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.red));

				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(0.05f, 0.05f), p + new Vector3(-0.05f, -0.05f), Color.red, 1.5f);
				if (!deterministicDebug && drawDebug) Debug.DrawLine(p + new Vector3(-0.05f, 0.05f), p + new Vector3(0.05f, -0.05f), Color.red, 1.5f);
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
