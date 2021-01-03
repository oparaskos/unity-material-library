using System.Collections.Generic;
using System.Threading.Tasks;

namespace HestiaMaterialImporter.Core
{
    public interface IMaterialsAdapter
    {
        PreviewImage Favicon { get; }
        Task<IEnumerable<Task<IMaterialOption>>> GetMaterials(string name);
        void OnActivate();
        void OnGUI();
    }
}