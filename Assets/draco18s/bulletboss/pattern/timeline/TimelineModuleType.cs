using UnityEngine;

namespace Assets.draco18s.bulletboss.pattern.timeline
{
	public abstract class TimelineModuleType : ScriptableObject
	{
		public enum ModuleType
		{
			Duration,
			LoopCount,
			Delay,
			ChildSlots
		}

		[SerializeField] private string description;
		[SerializeField] private ModuleType moduleType;
	}
}