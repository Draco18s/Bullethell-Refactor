using System;
using UnityEngine;

namespace Assets.draco18s.bulletboss
{
	[Serializable]
	public enum NamedRarity
	{
		Starting,
		Common,
		Uncommon,
		Rare,
		Epic,
		UltraRare,
		Legendary,
		Artifact,
	}

	internal static class EnumExtensions
	{
		private static readonly Color dkGray = new Color(77 / 255f, 77 / 255f,  77 / 255f, 1);
		private static readonly Color purple = new Color(54 / 255f, 16 / 255f, 255 / 255f, 1);
		private static readonly Color orange = new Color(255 / 255f, 88 / 255f, 16 / 255f, 1);
		private static readonly Color yellow = new Color(255 / 255f, 192 / 255f, 8 / 255f, 1);
		private static readonly Color seagrn = new Color(8 / 255f, 255 / 255f, 147 / 255f, 1);
		public static Color GetColor(this NamedRarity rarity)
		{
			switch (rarity)
			{
				case NamedRarity.Starting:
				case NamedRarity.Common:
					return dkGray;
				case NamedRarity.Uncommon:
					return Color.green;
				case NamedRarity.Rare:
					return seagrn;
				case NamedRarity.Epic:
					return Color.blue;
				case NamedRarity.Artifact:
					return purple;
				case NamedRarity.UltraRare:
					return orange;
				case NamedRarity.Legendary:
					return yellow;
			}
			return Color.gray;
		}
	}
}
