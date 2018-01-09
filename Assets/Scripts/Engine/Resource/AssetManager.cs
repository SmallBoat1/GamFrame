/*  文件描述： AssetBundle封装层
 */

using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFrame
{
    /// <summary>
    /// AssetBundle包管理器
    /// </summary>
    public class AssetManager
    {
        /// <summary>
        /// 本地缓存资源目录
        /// </summary>
        private string _cacheResourcePath;

        /// <summary>
        /// 依赖关系清单
        /// </summary>
        private AssetBundleManifest _manifest;

        /// <summary>
        /// Assets
        /// </summary>
        private readonly Dictionary<string, Object> _assetCache = new Dictionary<string, Object>();

        public string CacheResourcePath
        {
            get { return _cacheResourcePath; }
        }

        // 设置缓存资源存放地址
        public void SetCacheResourcePath(string path)
        {
            _cacheResourcePath = path; 
        }

        // 添加资源进池
        public void AddCacheAssetName(string key, Object value)
        {
            try
            {
                if (_assetCache.ContainsKey(key))
                {
                    if (_assetCache[key] == null)
                    {
                        _assetCache[key] = value;
                    }
                    else
                    {
                        if (!_assetCache[key].name.Equals(value.name))
                        {
                            Debug.LogErrorFormat("AddCacheAssetName failed!duplicate key: {0},already value: {1},new vlaue: {2}", key, _assetCache[key].name, value.name);
                        }                        
                    }
                }
                else
                {
                    _assetCache.Add(key, value);
                }
            }
            catch (MissingReferenceException)
            {
                Debug.LogErrorFormat("AddCacheAssetName failed!key:{0},value:{1}", key, value.name);
            }
        }

        // 获取资源名称
        public Object GetCacheAssetName(string key)
        {
            if (_assetCache.ContainsKey(key))
            {
                return _assetCache[key];
            }
            return null;
        }

        // 读取资源清单文件
        public void LoadManifest(string name)
        {
#if LOCAL_RESOURCE
            if (Game.Instance.LocalResource)
            {
                return;
            }
#endif
            //name = name.ToLower();
            var assetbundle = LoadManifestAssetBundle(name);

            _manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
//            Unload(false);
        }

        // 读取资源清单文件
        private AssetBundle LoadManifestAssetBundle(string name)
        {
            return LoadAssetBundle(name);
        }

        /// 读取AssetBundles
        public void LoadAssetBundles(string bundleName,string resourceName = "")
        {
            //            Debug.LogFormat("<color=red>want load bundle:{0}</color>",name);
            var depends = _manifest.GetAllDependencies(bundleName);
            //            Debug.Log("depends number: " + depends.Length);

            for (var i = 0; i < depends.Length; i++)
            {
                LoadDependenceAssetBundle(depends[i], resourceName,bundleName);
            }

            LoadMainAssetBundle(bundleName, resourceName);
            //            Debug.LogFormat("<color=yellow>finish load bundle:{0}</color>",name);
        }

        /// 读取AssetBundles
        public T Load<T>(string bundleName, string resourceName) where T : Object
        {
#if LOCAL_RESOURCE
            if (Game.Instance.LocalResource)
            {
                return null;
            }
#endif
//            bundleName = bundleName.ToLower();
//            resourceName = resourceName.ToLower();

            var assetname = resourceName.Equals(string.Empty) ? bundleName : resourceName;
            var asset = GetCacheAssetName(assetname) as T;
            if (asset != null)
            {
                BundleReferenceManager.Instance.AddBundleRefValue(bundleName, resourceName);
                return asset;
            }

            LoadAssetBundles(bundleName, resourceName);

            var assetBundle = BundleReferenceManager.Instance.GetAssetBundle(bundleName);
            if (assetBundle == null)
            {
                Debug.LogErrorFormat("Load<T> can not load assetBundle,name is {0}", bundleName);
                return null;
            }

            if (resourceName.Equals(string.Empty))
            {
                resourceName = assetBundle.GetAllAssetNames()[0];
            }
            

            // 加载相关资源
            asset = assetBundle.LoadAsset<T>(resourceName);
            if (asset == null)
            {
                var assets = assetBundle.LoadAllAssets(typeof (T));
                if (assets.Length>0)
                {
                    asset = assets[0] as T;
                    return asset;
                }
                Debug.LogErrorFormat("Load<T> can not load asset", bundleName);
                return null;
            }


            AddCacheAssetName(assetname, asset);          
            return asset;
        }

        /// 清理全部AssetBundle
        public void UnloadAll()
        {
            BundleReferenceManager.Instance.UnloadAll();
            ShaderRefreshManager.Instance.Clear();
            _assetCache.Clear();
        }
         
        // 读取主资源
        private void LoadMainAssetBundle(string bundleName, string resourceName)
        {
           // var assetbundle = LoadAssetBundle(bundleName, resourceName);
#if UNITY_EDITOR
            //if (!Lanuch.Instance.TrackResourceLife)
            //{
            //    return;
            //}
            //if (assetbundle != null)
            //{
            //    var allAssetNames = assetbundle.GetAllAssetNames();
            //    ResourceReferenceDebug.Instance.InitResourceList(bundleName, allAssetNames);
            //}
#endif
        }

        // 读取依赖资源
        private void LoadDependenceAssetBundle(string bundleName, string resourceName,string parent)
        {
           // var assetbundle = LoadAssetBundle(bundleName, resourceName, parent);
//#if UNITY_EDITOR
//            if (!Lanuch.Instance.TrackResourceLife)
//            {
//                return;
//            }
//            if (assetbundle != null)
//            {
//                var allAssetNames = assetbundle.GetAllAssetNames();
//                ResourceReferenceDebug.Instance.InitResourceList(bundleName, allAssetNames);
//            }
//#endif
        }

        /// 文件流读取
        private AssetBundle LoadAssetBundle(string bundleName, string resourceName = "", string parent = "")
        {
            if (bundleName.Equals(string.Empty))
            {
                return null;
            }

            var assetBundle = BundleReferenceManager.Instance.GetAssetBundle(bundleName);
            if (assetBundle != null)
            {
                BundleReferenceManager.Instance.AddBundleRefValue(bundleName, assetBundle, parent, resourceName);
                return assetBundle;
            }

            var bytes = LoadFile(bundleName);
            if (bytes == null)
            {
                Debug.LogErrorFormat("Invaild bytes {0}", bundleName);
                return null;
            }

            //创建assetBundle
            assetBundle = AssetBundle.LoadFromMemory(bytes);
            if (assetBundle == null)
            {
                Debug.LogErrorFormat("Can not load assetBundle {0}", bundleName);
                return null;
            }

            BundleReferenceManager.Instance.AddBundleCache(bundleName, assetBundle);
            BundleReferenceManager.Instance.AddBundleRefValue(bundleName, assetBundle, parent, resourceName);
            ShaderRefreshManager.Instance.AddShaderRefreshFlag(bundleName);

            return assetBundle;
        }

        /// 读取到解密的assetBundle文件
        private byte[] LoadFile(string name)
        {
            var assetFile = new AssetFile();
            assetFile.Load(_cacheResourcePath + name);
            if (assetFile.Data == null)
            {
                return null;
            }
            return Decode(assetFile.Data);
        }

        /// 解密
        private byte[] Decode(byte[] bytes)
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= 0xAA;
            }
            return bytes;
        }


//#region 异步加载相关API
//        // 读取AssetBundles,异步
//        public IEnumerator LoadAsync(string name)
//        {
//#if LOCAL_RESOURCE
//            if (Game.Instance.LocalResource)
//            {
//                yield break;
//            }
//#endif
//            name = name.ToLower();
//            var enumerator = LoadAssetBundlesAsync(name);
//            while (enumerator.MoveNext())
//            {
//                yield return enumerator;
//            }
//            var assetBundle = BundleReferenceManager.Instance.GetAssetBundle(name);
//            if (assetBundle == null)
//            {
//                Debug.LogErrorFormat("assetBundle is null : {0}", name);
//                yield break;
//            }
//            var asset = assetBundle.LoadAsset(name);
//            AddCacheAssetName(name, asset);
//            Debug.Log("_assetCache async Add :" + name);
//        }
//
//        // 文件流读取,异步
//        private IEnumerator LoadAssetBundleAsync(string name)
//        {
//            var assetBundle = BundleReferenceManager.Instance.GetAssetBundle(name);
//            if (assetBundle != null)
//            {
//                Debug.LogFormat("AssetBundle exist : {0}", name);
//                yield break;
//            }
//            var bytes = LoadFile(name);
//            if (bytes == null)
//            {
//                Debug.LogErrorFormat("Invaild bytes {0}", name);
//                yield break;
//            }
//            //创建assetBundle
//            var request = AssetBundle.LoadFromMemoryAsync(bytes);
//            if (request == null)
//            {
//                Debug.LogErrorFormat("request is null : {0}", name);
//                yield break;
//            }
//            while (!request.isDone)
//            {
//                yield return request;
//            }
//            assetBundle = request.assetBundle;
//            if (request.assetBundle == null)
//            {
//                Debug.LogErrorFormat("request.assetBundle is null : {0} {1}", name, bytes.Length);
//            }
//
//            BundleReferenceManager.Instance.AddBundleCache(name, assetBundle);
//            BundleReferenceManager.Instance.AddBundleRefValue(name, assetBundle, "");
//            ShaderRefreshManager.Instance.AddShaderRefreshFlag(name);
//
//            Debug.Log("_bundleCache async Add: " + name);
//        }
//
//        // 读取AssetBundles,异步
//        private IEnumerator LoadAssetBundlesAsync(string name)
//        {
//            IEnumerator enumerator = null;
//            var depends = _manifest.GetAllDependencies(name);
//            for (var i = 0; i < depends.Length; i++)
//            {
//                if (string.IsNullOrEmpty(depends[i]))
//                {
//                    Debug.LogErrorFormat("can not load depends : {0}", depends[i]);
//                    continue;
//                }
//                enumerator = LoadAssetBundleAsync(depends[i]);
//                while (enumerator.MoveNext())
//                {
//                    yield return enumerator;
//                }
//            }
//            enumerator = LoadAssetBundleAsync(name);
//            while (enumerator.MoveNext())
//            {
//                yield return enumerator;
//            }
//        }
//
//#endregion
    }
}
