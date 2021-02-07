using UnityEngine;
using UnityEditor;
using System;
using HestiaMaterialImporter.Extensions;

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
            else if(orgImage != null)
            {
                GUILayout.Box(orgImage.ToTexture2D(), GUILayout.Width(previewImageSize), GUILayout.Height(previewImageSize));
            }
            selectedVariant = EditorGUILayout.Popup(selectedVariant, variants);
            if (GUILayout.Button(new GUIContent("Import")))
            {
                Import();
            }
            EditorGUILayout.EndVertical();
            EditorGUI.EndChangeCheck();
            return rect;
        }

        private void Import() {
            try {
                // Ensure folder heirarchy
                EditorUtility.DisplayProgressBar("Importing", "Creating Folders...", 0f);
                string texturePath = $"{Application.dataPath}/Textures/{name}/";
                Debug.Log($"Textures will be stored at '{texturePath}'");
                texturePath.MakeParents();
                string materialPath = $"{Application.dataPath}/Materials/";
                Debug.Log($"Materials will be stored at '{materialPath}'");
                materialPath.MakeParents();
                DoImport(texturePath, materialPath);
            } catch (Exception e) {
                Debug.LogError(e);
                EditorGUILayout.HelpBox("Error importing", MessageType.Error);
            }
        }

        protected abstract void DoImport(string texturePath, string materialPath);

        public IMaterialOption InitOnMainThread()
        {
            orgImage?.ToTexture2D();
            previewImage?.ToTexture2D();
            return this;
        }
    }
}