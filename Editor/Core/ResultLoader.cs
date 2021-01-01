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
        private IEnumerable<Task<IEnumerable<IMaterialOption>>> _tasks;
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
                if(_results != null) {
                    return _results?.Select(it => it.InitOnMainThread());
                }
                return _tasks
                    .Where(task => task.IsCompleted)
                    .SelectMany(task => task.Result)
                    .Select(it => it.InitOnMainThread());
            }
        }

        public void LoadResults()
        {
            _completed = false;
            _results = null;
            try
            {
                _tasks = adapters
                    .Select(it => it.GetMaterials(searchString));
                _results = Task.WhenAll(_tasks).Result.SelectMany(it => it);
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
