using System.Collections.Generic;

namespace HestiaMaterialImporter.CC0
{
    public class CC0Asset
    {
        public string Tags;
        public string AssetReleasedate;
        public string DownloadCount;
        public string AssetDataTypeID;
        public string CreationMethodID;
        public string CreatedUsingAssetID = null;
        public string PopularityScore;
        public string Weblink;
        public Dictionary<string, string> PreviewSphere;
        public Dictionary<string, CC0Download> Downloads;
    }
}