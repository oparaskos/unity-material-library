using System;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace HestiaMaterialImporter
{
	static class Texture2DExt
	{

		public static Texture2D ToAsset(this Texture2D texture, string path, TextureImporterType type = TextureImporterType.Default)
		{
			string extension = path.Substring(path.LastIndexOf("."));
			$"{ Application.dataPath }/{ path }".MakeParents();
			byte[] imgbytes;
			if (extension.ToLower() == ".jpg")
				imgbytes = texture.EncodeToJPG();
			else if (extension.ToLower() == ".png")
				imgbytes = texture.EncodeToJPG();
			else if (extension.ToLower() == ".exr")
				imgbytes = texture.EncodeToEXR();
			else if (extension.ToLower() == ".tga")
				imgbytes = texture.EncodeToTGA();
			else throw new BadImageFormatException();

			File.WriteAllBytes("Assets/" + path, imgbytes);
			AssetDatabase.ImportAsset("Assets/" + path, ImportAssetOptions.ForceSynchronousImport);
			Thread.Sleep(10);
			TextureImporter importer = AssetImporter.GetAtPath("Assets/" + path) as TextureImporter;
			importer.isReadable = true;
			importer.textureType = type;
			AssetDatabase.ImportAsset("Assets/" + path, ImportAssetOptions.ForceSynchronousImport);
			Thread.Sleep(10);

			return AssetDatabase.LoadAssetAtPath("Assets/" + path, typeof(Texture2D)) as Texture2D;
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
	}
}