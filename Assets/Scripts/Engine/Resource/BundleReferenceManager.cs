using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFrame
{
    public class BundleReferenceManager : Singleton<BundleReferenceManager>
    {
        public BundleReferenceManager() { }

        /// <summary>
        /// bundleReference
        /// </summary>
        private readonly Dictionary<string, BundleReference> _bundleRefCache = new Dictionary<string, BundleReference>();

        // ab缓存池
        private readonly Dictionary<string, AssetBundle> _bundleCache = new Dictionary<string, AssetBundle>();

        public Dictionary<string, AssetBundle> BundleCache
        {
            get { return _bundleCache; }
        }

        /// <summary>
        /// bundleReference
        /// </summary>
        public Dictionary<string, BundleReference> BundleRefCache
        {
            get { return _bundleRefCache; }
        }

        public void UnloadAll()
        {
            var enumerator = _bundleCache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value != null)
                {
                    enumerator.Current.Value.Unload(true);
                }
            }
            _bundleCache.Clear();
        }

        /// 尝试获取一个AssetBundle
        public AssetBundle GetAssetBundle(string name)
        {
            AssetBundle assetBundle = null;
            _bundleCache.TryGetValue(name, out assetBundle);
            if (assetBundle != null)
            {
                return assetBundle;
            }
            return null;
        }

        // 增加bundle引用
        public void AddBundleRefValue(string bundleName,string resourceName)
        {
            if (!_bundleRefCache.ContainsKey(bundleName))
            {
                Debug.LogError("cannot find bundle: " + bundleName);
                return;
            }

            var bundle = _bundleRefCache[bundleName];
            bundle.AddRef();

//#if UNITY_EDITOR
//            if (Lanuch.Instance.TrackResourceLife)
//            {
//                ResourceReferenceDebug.Instance.AddReferenceCount(bundleName);
//                ResourceReferenceDebug.Instance.AddHistoryOperator(ReferenceState.Add, bundleName, bundleName, resourceName);
//            }
//#endif

            for (int i = 0; i < bundle.DependenceList.Count; i++)
            {
                var dependence = bundle.DependenceList[i];
                var dependencebundle = GetBundleReference(dependence);
                dependencebundle.AddRef();

//#if UNITY_EDITOR
//                if (Lanuch.Instance.TrackResourceLife)
//                {
//                    ResourceReferenceDebug.Instance.AddReferenceCount(dependence);
//                    ResourceReferenceDebug.Instance.AddHistoryOperator(ReferenceState.Add, dependence, bundleName);
//                }
//#endif
            }
        }

        /// <summary>
        /// 增加bundle引用
        /// </summary>
        /// <param name="dependenceBundle">资源名称</param>
        /// <param name="bundle"></param>
        /// <param name="sourceBundle">被依赖资源的名称</param>
        /// <param name="resource"></param>
        public void AddBundleRefValue(string dependenceBundle, AssetBundle bundle, string sourceBundle,string resource)
        {
            if (!_bundleRefCache.ContainsKey(dependenceBundle))
            {
                CreateBundleRef(dependenceBundle, new BundleReference(dependenceBundle));
            }

            if (_bundleRefCache[dependenceBundle].IsNull())
            {
                _bundleRefCache[dependenceBundle].Bundle = bundle;
            }

            _bundleRefCache[dependenceBundle].AddRef();

//#if UNITY_EDITOR
//            if (Lanuch.Instance.TrackResourceLife)
//            {
//                ResourceReferenceDebug.Instance.AddReferenceCount(dependenceBundle);
//                ResourceReferenceDebug.Instance.AddHistoryOperator(ReferenceState.Add, dependenceBundle, sourceBundle, resource);
//            }
//#endif

            if (string.Equals(sourceBundle, string.Empty))
            {
                return;
            }

            _bundleRefCache[dependenceBundle].AddReference(sourceBundle);

//#if UNITY_EDITOR
//            if (Lanuch.Instance.TrackResourceLife)
//            {
//                ResourceReferenceDebug.Instance.AddReference(dependenceBundle, sourceBundle, resource);
//            }
//#endif

            var sourcebundle = GetBundleReference(sourceBundle);
            if (sourcebundle == null)
            {
                sourcebundle = new BundleReference(sourceBundle);
                CreateBundleRef(sourceBundle, sourcebundle);
            }

            sourcebundle.AddDependence(dependenceBundle);

//#if UNITY_EDITOR
//            if (Lanuch.Instance.TrackResourceLife)
//            {
//                ResourceReferenceDebug.Instance.AddDependence(dependenceBundle, sourceBundle, resource);
//            }
//#endif
        }

        // 减少bundle引用
        public void DecBundleRefValue(string bundlename,string resourcename)
        {
            if (!_bundleRefCache.ContainsKey(bundlename))
            {
                //if (!Lanuch.Instance.LoadLocalResource)
                //{
                //    Debug.LogError("DecBundleRefValue failed,can not find key: " + bundlename);
                //}
                return;
            }

            var bundle = _bundleRefCache[bundlename];

            var dependeces = bundle.DependenceList;

            for (var i = 0; i < dependeces.Count; i++)
            {
                DecRefBundle(dependeces[i], bundlename, resourcename);
            }

            DecRefBundle(bundlename, bundlename, resourcename);
        }

        private void DecRefBundle(string name, string sourcename, string resourcename)
        {
            if (!_bundleRefCache.ContainsKey(name))
            {
                Debug.LogErrorFormat("DecBundleRefValue failed,can not find key: {0},sourcename: {1}", name, sourcename);
                return;
            }

            var bundle = _bundleRefCache[name];
            if (bundle.DecRef())
            {
                RemoveBundleCache(name);
                _bundleRefCache.Remove(name);
            }

//#if UNITY_EDITOR
//            if (Lanuch.Instance.TrackResourceLife)
//            {
//                ResourceReferenceDebug.Instance.DecReferSenceCount(name, sourcename);
//                ResourceReferenceDebug.Instance.AddHistoryOperator(ReferenceState.Sub, name, sourcename, resourcename);
//            }
//#endif
        }

        public BundleReference GetBundleReference(string name)
        {
            if (_bundleRefCache.ContainsKey(name))
            {
                return _bundleRefCache[name];
            }

            return null;
        }

        private void CreateBundleRef(string name, BundleReference bundle)
        {
            if (!_bundleRefCache.ContainsKey(name))
            {
                _bundleRefCache.Add(name, bundle);
            }
        }

        public void AddBundleCache(string name, AssetBundle bundle)
        {
            if (!_bundleCache.ContainsKey(name))
            {
                _bundleCache.Add(name, bundle);
            }
        }

        public void RemoveBundleCache(string name)
        {
            if (_bundleCache.ContainsKey(name))
            {
                _bundleCache[name].Unload(true);
                _bundleCache[name] = null;
            }

            ShaderRefreshManager.Instance.Remove(name);

            _bundleCache.Remove(name);
        }
    }
}