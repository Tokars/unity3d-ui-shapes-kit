using UnityEngine;

namespace ThisOtherThing.Utils
{
	public class MinNumAttribute : PropertyAttribute
	{
		public readonly float minFloat;
		public readonly int minInt;

		public MinNumAttribute(float min)
		{
			this.minFloat = min;
		}

		public MinNumAttribute(int min)
		{
			this.minInt = min;
		}
	}
}