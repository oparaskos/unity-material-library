using System;
using UnityEngine;

static class Texture2DExt {

	public static Texture2D MaskMap(Texture2D metalness, Texture2D ambientOcclusion, Texture2D smoothness, Texture2D detailMapMask = null)
	{
		int width = metalness?.width ?? ambientOcclusion?.width ?? smoothness?.width ?? detailMapMask?.width ?? 512;
		int height = metalness?.height ?? ambientOcclusion?.height ?? smoothness?.height ?? detailMapMask?.height ?? 512;

		Texture2D maskMap = new Texture2D(width, height);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				maskMap.SetPixel(i, j, new Color(
					// Red channel: Metallic mask. 0 = not metallic, 1 = metallic.
					metalness?.GetPixel(i,j).grayscale ?? 0.0f,
					//Green channel: Ambient occlusion.
					ambientOcclusion?.GetPixel(i, j).grayscale ?? 0.0f,
					//Blue channel: Detail map mask.
					detailMapMask?.GetPixel(i, j).grayscale ?? 0.0f,
					//Alpha channel: Smoothness.
					smoothness?.GetPixel(i, j).grayscale ?? 0.0f
				));
			}
		}
		return maskMap;
	}
	public static Texture2D Inverted(this Texture2D original)
	{
		Texture2D inverted = new Texture2D(original.width, original.height);
		for (int i = 0; i < inverted.width; i++)
		{
			for (int j = 0; j < inverted.height; j++)
			{
				inverted.SetPixel(i, j, original.GetPixel(i, j).Invert());
			}
		}
		inverted.Apply();
		return inverted;
	}

	public static Color Invert(this Color original)
	{
		return new Color(1.0f - original.r, 1.0f - original.g, 1.0f - original.b);
	}
}