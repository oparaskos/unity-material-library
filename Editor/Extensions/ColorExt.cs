using UnityEngine;

namespace HestiaMaterialImporter
{
	static class ColorExt
	{
		public static Color Invert(this Color original)
		{
			return new Color(1.0f - original.r, 1.0f - original.g, 1.0f - original.b);
		}
	}
}