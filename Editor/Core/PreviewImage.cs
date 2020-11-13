using System.Threading.Tasks;
using UnityEngine;
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

		public Texture2D ToTexture2D()
		{
			if (textureContent == null)
				textureContent = futureContent.ToTexture2D();
			futureContent = null;
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
	}
}