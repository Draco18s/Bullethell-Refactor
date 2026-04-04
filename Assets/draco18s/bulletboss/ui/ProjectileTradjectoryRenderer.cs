using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.pattern;
using Assets.draco18s.bulletboss.pattern.timeline;
using Assets.draco18s.ui;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.draco18s.bulletboss.ui
{
	public class ProjectileTradjectoryRenderer : MonoBehaviour
	{
		public static ProjectileTradjectoryRenderer instance;
		[SerializeField] private Material lineMaterial;

		private List<LineRenderer> lineRenderers = new();
		public RenderOptions originalFlags;

		[Flags]
		public enum RenderOptions
		{
			None = 0,
			Clear = 1 << 0,
			DrawPath = 1 << 1,
			IsStationary = 1 << 2,
			IncludeNextChild = 1 << 3,
			MoveAlongParentPath = 1 << 4,
			FadeOutPathAlongLength = 1 << 5,
			FadeOutPathInstances = 1 << 6
		}

		void Awake()
		{
			instance = this;
		}

		public void DrawTimeline(Bullet parent, Timeline pattern, RenderOptions opts, bool fresh=true)
		{
			if(fresh)
				originalFlags = opts;
			if (opts.HasFlag(RenderOptions.Clear))
				ClearExisting();

			if (opts.HasFlag(RenderOptions.IsStationary))
			{
				if (!opts.HasFlag(RenderOptions.IncludeNextChild))
					return;
				/*foreach (KeyValuePair<int, Card> kvp in pattern.GetModules())
				{
					///if (kvp.Value.pattern is SpawnModuleType.SpawnModule spawnMod)
					//{
				    //	spawnMod.ResetForNewLoopIteration(parent);
				    //	DrawTimeline(spawnMod.SpawnNewBullet(parent), opts & ~(RenderOptions.IncludeNextChild | RenderOptions.IsStationary));
					//	continue;
					//}

					if (kvp.Value.pattern is ChangeModuleType.ChangeModule) continue;
					if (kvp.Value.pattern is IntangibleModuleType.IntangibleModule) continue;
					if (kvp.Value.pattern is HomingTurnModuleType.HomingTurnModule) continue;

					GameObject go2 = Instantiate(GameAssets.defaultBulletPrefab);
					Bullet bul = go2.GetComponent<Bullet>();
					bul.SetPattern(pattern);
					bul.SetParent(parent);
					bul.InitForSim(1);
					kvp.Value.pattern.ResetForNewLoopIteration(bul);
					DrawTimeline(bul, opts & ~(RenderOptions.IncludeNextChild | RenderOptions.IsStationary));
					Destroy(go2);
				}*/
				GameObject go2 = Instantiate(GameAssets.defaultBulletPrefab);
				Bullet bul = go2.GetComponent<Bullet>();
				bul.SetPattern(pattern);
				bul.SetParent(parent.ParentShot);
				bul.InitForSim(0);
				bul.transform.position = parent.transform.position;
				bul.transform.rotation = parent.transform.rotation;
				DrawTimeline(bul, opts & ~(RenderOptions.IncludeNextChild | RenderOptions.IsStationary));
				Destroy(go2);
			}
			else
			{
				GameObject go = new GameObject("<path 1>", typeof(LineRenderer));
				LineRenderer r = go.GetComponent<LineRenderer>();
				r.startWidth = 0.025f;
				r.endWidth = 0.025f;
				r.material = lineMaterial;
				float a1 = opts.HasFlag(RenderOptions.FadeOutPathAlongLength) ? 0.9f : 0.5f;
				float a2 = opts.HasFlag(RenderOptions.FadeOutPathAlongLength) ? 0.1f : 0.5f;
				r.startColor = new Color(1, 1, 1, a1);
				r.endColor = new Color(1, 1, 1, a2);

				Vector3[] pts = GetPredictedPath(parent, pattern, opts);
				if (pts.Length > 2)
				{
					r.positionCount = pts.Length;
					r.SetPositions(pts);
					lineRenderers.Add(r);
				}
				else
					Destroy(go);
			}
		}

		private void DrawTimeline(Bullet shot, RenderOptions opts)
		{
			GameObject go = new GameObject("<path 2>", typeof(LineRenderer));
			LineRenderer r = go.GetComponent<LineRenderer>();
			r.startWidth = 0.025f;
			r.endWidth = 0.025f;
			r.material = lineMaterial;
			float a1 = opts.HasFlag(RenderOptions.FadeOutPathAlongLength) ? 0.9f : 0.5f;
			float a2 = opts.HasFlag(RenderOptions.FadeOutPathAlongLength) ? 0.1f : 0.5f;
			r.startColor = new Color(1, 1, 1, a1);
			r.endColor = new Color(1, 1, 1, a2);

			Vector3[] pts = GetPredictedPath(shot);
			if (pts.Length > 2 && shot.Speed > float.Epsilon)
			{
				r.positionCount = pts.Length;
				r.SetPositions(pts);
				lineRenderers.Add(r);
			}
			else
				Destroy(go);
		}

		private Vector3[] GetPredictedPath(Bullet shot)
		{
			List<Vector3> pts = new List<Vector3>();
			float dt = Time.fixedDeltaTime;
			//pattern.InitOrReset();
			do
			{
				pts.Add(shot.transform.position);
			} while (!shot.Simulate(dt));

			Destroy(shot.gameObject);

			return pts.ToArray();
		}

		private Vector3[] GetPredictedPath(Bullet parent, Timeline pattern, RenderOptions opts)
		{
			List<Vector3> pts = new List<Vector3>();
			GameObject go = new GameObject("<temp bullet>", typeof(Bullet));
			Bullet b = go.GetComponent<Bullet>();

			b.SetParent(parent.ParentShot);
			b.SetPattern(pattern);
			b.InitForSim(opts.HasFlag(RenderOptions.IsStationary) ? 0 : 1);
			b.transform.position = parent.transform.position;
			b.transform.rotation = parent.transform.rotation;
			float dt = Time.fixedDeltaTime;
			//Timeline pattern = tl.pattern;
			pattern.InitOrReset();
			do
			{
				pts.Add(b.transform.position);
			} while (!b.Simulate(dt));

			Destroy(go);

			return pts.ToArray();
		}

		public void ClearExisting()
		{
			foreach (LineRenderer r in lineRenderers)
			{
				Destroy(r.gameObject);
			}
			lineRenderers.Clear();
		}
	}
}
