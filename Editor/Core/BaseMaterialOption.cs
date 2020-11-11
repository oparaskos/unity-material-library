using UnityEngine;
using UnityEditor;

namespace HestiaMaterialImporter.Core
{
    public abstract class BaseMaterialOption : IMaterialOption
    {
        public string name { get; internal set; }
        public string[] variants { get; internal set; }
        protected int selectedVariant;

        protected PreviewImage orgImage;
        protected PreviewImage previewImage;

        protected int previewImageSize = 128;
        protected int orgImgSize = 16;

        public Rect OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            var rect = EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.PrefixLabel(new GUIContent(name));
            if (previewImage != null)
            {
                GUILayout.Box(previewImage.ToTexture2D(), GUILayout.Width(previewImageSize), GUILayout.Height(previewImageSize));
                GUI.DrawTexture(new Rect(rect.x, rect.y, (rect.width * 2) - orgImgSize, orgImgSize), orgImage.ToTexture2D(), ScaleMode.ScaleToFit);
            }
            else
            {
                GUILayout.Box(orgImage.ToTexture2D(), GUILayout.Width(previewImageSize), GUILayout.Height(previewImageSize));
            }
            selectedVariant = EditorGUILayout.Popup(selectedVariant, variants);
            if (GUILayout.Button(new GUIContent("Import")))
            {
                DoImport();
            }
            EditorGUILayout.EndVertical();
            EditorGUI.EndChangeCheck();
            return rect;
        }

        protected abstract void DoImport();

        public IMaterialOption InitOnMainThread()
        {
            orgImage.ToTexture2D();
            previewImage.ToTexture2D();
            return this;
        }
    }
}