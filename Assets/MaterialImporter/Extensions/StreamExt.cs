using System.IO;
using System.Threading.Tasks;

namespace HestiaMaterialImporter
{
	static class StreamExt
	{
		public static byte[] ReadFully(this Stream input)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				input.CopyTo(ms);
				return ms.ToArray();
			}
		}

		public static async Task<byte[]> ReadFullyAsync(this Stream input)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				await input.CopyToAsync(ms);
				return ms.ToArray();
			}
		}
	}
}