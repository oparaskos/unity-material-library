using System.IO.Compression;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using HestiaMaterialImporter.Extensions;
using HestiaMaterialImporter.Core;

namespace HestiaMaterialImporter.Local
{
    public class LocalMaterialOption : BaseMaterialOption
    {

        protected override void DoImport()
        {
            return;
        }

        public static async Task<IMaterialOption> Create(string key, PreviewImage favicon)
        {
            int previewImageSize = 128;
            int orgImgSize = 16;
            return new LocalMaterialOption()
            {
                name = key,
                variants = new string[0],
                selectedVariant = 0,
                orgImage = favicon,
                previewImage = await GrabPreviewImage(previewImageSize),
                previewImageSize = previewImageSize,
                orgImgSize = orgImgSize
            };
        }

        private static async Task<PreviewImage> GrabPreviewImage(int previewImageSize)
        {
            // string preview = value.PreviewSphere[$"{previewImageSize}-PNG"];
            // if (preview == null)
            //     preview = value.PreviewSphere[$"{previewImageSize}-JPG"];
            // if (preview == null)
            //     preview = value.PreviewSphere.Last().Value;
            // if (preview != null)
            //     return await PreviewImage.LoadUri(preview);
            return null;
        }
    }
}