using UnityEngine.UI;
using UnityEngine;

namespace Assets.draco18s.ui
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class UILineRenderer : MaskableGraphic
	{
		public float LineThikness = 2;
		public bool UseMargins;
		public Vector2 Margin;
		public Vector2[] Points;

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			Rect r = GetPixelAdjustedRect();
			Vector2 o = new Vector2(r.x, r.y);

			float sizeX = 1; //rectTransform.rect.width;
			float sizeY = 1; //rectTransform.rect.height;
			float offsetX = 0; //-rectTransform.pivot.x * rectTransform.rect.width;
			float offsetY = 0; //-rectTransform.pivot.y * rectTransform.rect.height;

			if (UseMargins)
			{
				sizeX -= Margin.x;
				sizeY -= Margin.y;
				offsetX += Margin.x / 2f;
				offsetY += Margin.y / 2f;
			}

			vh.Clear();

			Vector2 prevV1 = Vector2.zero;
			Vector2 prevV2 = Vector2.zero;

			if (Points == null) return;

			for (int i = 1; i < Points.Length; i++)
			{
				Vector2 prev = Points[i - 1];
				Vector2 cur = Points[i];
				prev = new Vector2(prev.x * sizeX + offsetX, prev.y * sizeY + offsetY);
				cur = new Vector2(cur.x * sizeX + offsetX, cur.y * sizeY + offsetY);

				float angle = Mathf.Atan2(cur.y - prev.y, cur.x - prev.x) * 180f / Mathf.PI;

				Vector2 v1 = prev + new Vector2(0, -LineThikness / 2);
				Vector2 v2 = prev + new Vector2(0, +LineThikness / 2);
				Vector2 v3 = cur + new Vector2(0, +LineThikness / 2);
				Vector2 v4 = cur + new Vector2(0, -LineThikness / 2);

				v1 = RotatePointAroundPivot(v1, prev, new Vector3(0, 0, angle));
				v2 = RotatePointAroundPivot(v2, prev, new Vector3(0, 0, angle));
				v3 = RotatePointAroundPivot(v3, cur, new Vector3(0, 0, angle));
				v4 = RotatePointAroundPivot(v4, cur, new Vector3(0, 0, angle));

				if (i > 1)
					SetVbo(vh, new[] { prevV1 + o, prevV2 + o, v1 + o, v2 + o }, i);
				SetVbo(vh, new[] { v1 + o, v2 + o, v3 + o, v4 + o }, 0);

				prevV1 = v3;
				prevV2 = v4;
			}
		}

		protected void SetVbo(VertexHelper vh, Vector2[] vertices, int i)
		{
			Color32 color32 = color;
			vh.AddVert(vertices[0], color32, new Vector2(0f, 0f));
			vh.AddVert(vertices[1], color32, new Vector2(0f, 1f));
			vh.AddVert(vertices[2], color32, new Vector2(1f, 1f));
			vh.AddVert(vertices[3], color32, new Vector2(1f, 0f));

			vh.AddTriangle(vh.currentVertCount - 4, vh.currentVertCount - 3, vh.currentVertCount - 2);
			vh.AddTriangle(vh.currentVertCount - 2, vh.currentVertCount - 1, vh.currentVertCount - 4);
		}

		public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
		{
			Vector3 dir = point - pivot; // get point direction relative to pivot
			dir = Quaternion.Euler(angles) * dir; // rotate it
			point = dir + pivot; // calculate rotated point
			return point; // return it
		}
	}
}
