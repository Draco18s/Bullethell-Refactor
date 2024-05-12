using UnityEngine;
using UnityEngine.UI;

namespace Assets.draco18s.ui
{
	public class CustomScrollbar : Scrollbar
	{
		[SerializeField]
		private int CustomNumberOfSteps = 0;

		protected override void OnValidate()
		{
			base.OnValidate();
			numberOfSteps = CustomNumberOfSteps;
		}
	}
}
