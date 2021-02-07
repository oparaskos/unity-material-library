using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using HestiaMaterialImporter.Extensions;

namespace HestiaMaterialImporter.Core
{

	public class PreviewImage
	{
		private byte[] futureContent;
		private Texture2D textureContent;

		private PreviewImage(byte[] futureContent)
		{
			this.futureContent = futureContent;
		}

		private PreviewImage(Texture content)
		{
			this.textureContent = content as Texture2D;
		}
		private PreviewImage()
		{
		}

		public virtual Texture2D ToTexture2D()
		{
			if (textureContent == null && futureContent != null) {
				textureContent = futureContent.ToTexture2D();
				futureContent = null;
			}
			return textureContent;
		}

		public static async Task<PreviewImage> LoadUri(string uri)
		{
			return new PreviewImage(await uri.BytesFromUriAsync());
		}

		public static Task<PreviewImage> LoadFavicon(string url)
		{
			return LoadUri($"https://icons.duckduckgo.com/ip2/{url}.ico");
        }

		public static PreviewImage LoadIcon(string iconName) {
			return new IconPreviewImage(iconName);
		}

		public static PreviewImage LoadTextureAtPath(string path) {
			return new TexturePreviewImage(path);
		}

		public class TexturePreviewImage : PreviewImage {
			private string path;
			public TexturePreviewImage(string path) {
				this.path = path;
			}

			public override Texture2D ToTexture2D() {
				if (textureContent == null)
	            	textureContent = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));

				return textureContent;
			}
		}
		public class IconPreviewImage : PreviewImage {
			private string iconName;
			public IconPreviewImage(string iconName) {
				this.iconName = iconName;
			}

			public override Texture2D ToTexture2D() {
				if (textureContent == null)
					textureContent = (EditorGUIUtility.IconContent(iconName).image) as Texture2D;
				return textureContent;
			}
		}
	}
}