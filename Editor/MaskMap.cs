using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace HestiaMaterialImporter
{
	static class MaskMap
	{

		public static Texture2D Create(
			Texture2D red, 
			Texture2D green,
			Texture2D blue,
			Texture2D alpha,
			float defaultRed = 1.0f,
			float defaultGreen = 1.0f,
			float defaultBlue = 1.0f,
			float defaultAlpha = 1.0f)
		{
			int width = red?.width ?? green?.width ?? alpha?.width ?? blue?.width ?? 512;
			int height = red?.height ?? green?.height ?? alpha?.height ?? blue?.height ?? 512;

			Texture2D maskMap = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
			maskMap.alphaIsTransparency = true;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					maskMap.SetPixel(i, j, new Color(
						// Red channel: Metallic mask. 0 = not metallic, 1 = metallic.
						red?.GetPixel(i, j).grayscale ?? defaultRed,
						//Green channel: Ambient occlusion.
						green?.GetPixel(i, j).grayscale ?? defaultGreen,
						//Blue channel: Detail map mask.
						blue?.GetPixel(i, j).grayscale ?? defaultBlue,
						//Alpha channel: Smoothness.
						alpha?.GetPixel(i, j).grayscale ?? defaultAlpha
					));
				}
			}
			return maskMap;
		}
	}
}