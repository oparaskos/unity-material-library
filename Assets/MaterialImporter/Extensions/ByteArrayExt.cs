using UnityEngine;

namespace HestiaMaterialImporter
{
	static class ByteArrayExt
	{
		public static Texture2D ToTexture2D(this byte[] rawData)
		{
			Texture2D img = new Texture2D(2, 2);
			img.alphaIsTransparency = true;
			img.LoadImage(rawData);
			return img;
		}
	}
}