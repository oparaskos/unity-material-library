using UnityEngine;

namespace HestiaMaterialImporter.Core
{
    public interface IMaterialOption
    {
        string name { get; }
        string[] variants { get; }
        Rect OnGUI();
        IMaterialOption InitOnMainThread();
    }
}