using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFrame
{
    public class Observer<TKey,TValue> where TValue : class
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        public Dictionary<TKey, TValue> DictionaryContext
        {
            get { return _dictionary; }
        }

        public void AddValue(TKey key, TValue value)
        {
            if (value == null)
            {
                return;
            }

            if (_dictionary.ContainsKey(key))
            {
                Debug.LogError("Observer duplicate key: " + key);
                return;
            }

            if (!_dictionary.ContainsKey(key))
            {
                _dictionary.Add(key, value);
            }
        }

        public void RemoveValue(TKey key)
        {
            if (_dictionary.ContainsKey(key))
            {
                _dictionary.Remove(key);                
            }
        }

        public TValue GetValue(TKey key)
        {
            TValue value;
            if (!_dictionary.TryGetValue(key, out value))
            {
                return null;
            }

            return value;
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public int Count()
        {
            return _dictionary.Count;
        }

//        public void TravalRed()
//        {
//            var enu = _dictionary.GetEnumerator();
//            while (enu.MoveNext())
//            {
//                Debug.LogFormat("<color=red>load bundle:{0}</color>", enu.Current.Value);
//            }
//        }
//
//        public void TravalGreen()
//        {
//            var enu = _dictionary.GetEnumerator();
//            while (enu.MoveNext())
//            {
//                Debug.LogFormat("<color=green>unload bundle:{0}</color>", enu.Current.Value);
//            }
//        }
//
//        public void Difference(Observer<TKey, TValue> other)
//        {
//            var list = _dictionary.Values.ToList();
//            var otherList = other._dictionary.Values.ToList();
//
//            var diff = list.Except(otherList).ToList();
//
//            Debug.LogFormat("<color=red>diff count: {0}</color> exit", diff.Count());
//
//            foreach (var value in diff)
//            {
//                Debug.Log(value);
//                Debug.Log(BundleReferenceManager.Instance.OutPutBeDependenceList(value.ToString()));
//            }
//        }
    }
}
