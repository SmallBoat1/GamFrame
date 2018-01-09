using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public class BundleReference
    {
        public string BundleName;
        // Bundle
        public AssetBundle Bundle;
        // 计数器
        public int Count;
        // 依赖资源列表
        public List<string> DependenceList = new List<string>();
        // 被依赖资源列表
        public List<string> ReferenceList = new List<string>();

        public BundleReference(string name)
        {
            BundleName = name;
            Count = 0;
            DependenceList.Clear();
            ReferenceList.Clear();
            Bundle = null;
        }

        public void SetBundle(AssetBundle bundle)
        {
            Bundle = bundle;
        }

        public void AddDependence(string name)
        {
            if (!DependenceList.Contains(name))
            {
                DependenceList.Add(name);
            }
        }

        public void AddReference(string name)
        {
            if (!ReferenceList.Contains(name))
                ReferenceList.Add(name);
        }

        public bool IsNull()
        {
            return Bundle == null;
        }

        public void AddRef()
        {
            Count++;
        }

        public bool DecRef()
        {
            if (Count > 0)
            {
                Count--;
            }

            return Count == 0;
        }

        // 是否为主资源（即只依赖其它资源，不被其它资源依赖）
        public bool IsMainBundle()
        {
            return ReferenceList.Count == 0;
        }
    }

}