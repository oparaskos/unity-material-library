using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace HestiaMaterialImporter
{
    class ResultLoader
    {
        public Thread thread = null;
        private IEnumerable<IMaterialOption> _results;
        public bool completed = false;
        public bool failed = false;
        public string searchString;
        public IMaterialsAdapter[] adapters;

        public IEnumerable<IMaterialOption> Results
        {
            get
            {
                return _results?.Select(it => it.InitOnMainThread());
            }
        }

        public void LoadResults()
        {
            try
            {
                _results = Task.WhenAll(adapters
                    .Select(it => it.GetMaterials(searchString))).Result.SelectMany(it => it);
                completed = true;
            }
            catch (Exception e)
            {
                failed = true;
            }
        }
    }
}
