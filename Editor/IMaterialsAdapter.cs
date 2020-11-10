using System.Collections.Generic;
using System.Threading.Tasks;

namespace HestiaMaterialImporter
{
    public interface IMaterialsAdapter
    {
        PreviewImage Favicon { get; }
        Task<List<IMaterialOption>> GetMaterials(string name);
    }
}