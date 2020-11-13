using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using UnityEngine;
using System.Runtime.Remoting.Messaging;

namespace HestiaMaterialImporter.Core
{
    [Serializable]
    public class ResultLoader
    {
        private IEnumerable<IMaterialOption> _results;
        public bool _completed = false;
        public bool failed = false;
        public string searchString;

        [NonSerialized]
        public Thread thread = null;
        [NonSerialized]
        public IMaterialsAdapter[] adapters;

        public bool completed {
            get {
                return _results != null && _completed;
            }
        }

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
                _completed = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                failed = true;
            }
        }
    }
}
