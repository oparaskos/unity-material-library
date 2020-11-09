using System.Net;
using System.Threading.Tasks;

namespace HestiaMaterialImporter
{
	static class StringExt { 
		public async static Task<byte[]> BytesFromUriAsync(this string uri)
		{
			WebRequest webRequest = WebRequest.Create(uri);
			WebResponse webResp = await webRequest.GetResponseAsync();
			return await webResp.GetResponseStream().ReadFullyAsync();
		}
	}
}
