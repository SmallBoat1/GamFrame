using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public class ReFreshMaterial
    {
        public bool isfresh;
        public AssetBundle bundle;

        public ReFreshMaterial(AssetBundle ab)
        {
            isfresh = false;
            bundle = ab;
        }
    }

    public class ShaderRefreshManager : Singleton<ShaderRefreshManager>
    {
        public ShaderRefreshManager()
        {
            
        }

        // shader是否刷新表
        private readonly Dictionary<string, bool> _shaderRefreshs = new Dictionary<string, bool>();

        public void Clear()
        {
            _shaderRefreshs.Clear();
        }

        // 刷新shader
        public void RefreshShader()
        {
            var materials = new List<Material>();

            var enu = BundleReferenceManager.Instance.BundleCache.GetEnumerator();

            while (enu.MoveNext())
            {
                var name = enu.Current.Key;
                if (!GetShaderRefreshFlag(enu.Current.Key))
                {
                    if (!name.Contains("mat") && !name.Contains("Mat") && name.Contains("scenes"))
                    {
                        AddShaderRefreshFlag(enu.Current.Key);
                        continue;
                    }
                    materials.AddRange(enu.Current.Value.LoadAllAssets<Material>());
                    SetShaderRefreshed(name);
                }
            }

            for (var i = 0; i < materials.Count; i++)
            {
                var material = materials[i];
                var shadername = material.shader.name;
                var newshader = Shader.Find(shadername);
                if (newshader != null)
                {
                    material.shader = newshader;
                }
            }
        }

        public void AddShaderRefreshFlag(string name)
        {

            if (!_shaderRefreshs.ContainsKey(name))
            {
                _shaderRefreshs.Add(name, false);
            }
        }

        public bool GetShaderRefreshFlag(string name)
        {
            if (!_shaderRefreshs.ContainsKey(name))
            {
                Debug.LogWarning("cannot find shader: " + name);
                return false;
            }
            return _shaderRefreshs[name];
        }

        public void SetShaderRefreshed(string name)
        {
            if (!_shaderRefreshs.ContainsKey(name))
            {
                Debug.LogWarning("cannot find shader: " + name);
            }
            _shaderRefreshs[name] = true;
        }

        public void Remove(string name)
        {
            if (_shaderRefreshs.ContainsKey(name))
            {
                _shaderRefreshs.Remove(name);
            }
        }
    }
}