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
        private IEnumerable<Task<IMaterialOption>> _results;
        private IEnumerable<Task<IEnumerable<Task<IMaterialOption>>>> _tasks = new List<Task<IEnumerable<Task<IMaterialOption>>>>();
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
                try {
                    if(_results != null) {
                        return _results.Select(it => it.Result.InitOnMainThread());
                    }
                    return _tasks
                        .Where(task => task.IsCompleted)
                        .Where(task => !task.IsCanceled)
                        .Where(task => !task.IsFaulted)
                        .Where(task => task.Status == TaskStatus.RanToCompletion)
                        .SelectMany(task => task.Result)
                        .Where(task => task.IsCompleted)
                        .Where(task => !task.IsCanceled)
                        .Where(task => !task.IsFaulted)
                        .Where(task => task.Status == TaskStatus.RanToCompletion)
                        .Select(it => it.Result.InitOnMainThread());
                } catch(Exception e) {
                    Debug.LogException(e);
                    failed = true;
                    return new List<IMaterialOption>();
                }
            }
        }

        public void LoadResults()
        {
            _completed = false;
            _results = null;
            try
            {
                _tasks = adapters
                    .Select(it => it.GetMaterials(searchString))
                    .ToList();
                Debug.Log("Loading results");
                _results = Task.WhenAll(_tasks).Result
                    .SelectMany(i => i)
                    .ToList();
                _completed = true;
                Debug.Log("Finished Gathering Results");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                failed = true;
            }
        }
    }
}
