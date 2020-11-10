using UnityEngine;

namespace HestiaMaterialImporter
{
    public interface IMaterialOption
    {
        string name { get; }
        string[] variants { get; }
        Rect OnGUI();
        IMaterialOption InitOnMainThread();
    }
}