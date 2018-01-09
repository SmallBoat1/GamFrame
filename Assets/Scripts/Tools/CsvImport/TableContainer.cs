using System;
using System.Collections.Generic;
using UnityEngine;
    /// <summary>
    /// 表容器接口
    /// </summary>
    public interface ITableContainer
    {
        string Name { get; }
        bool Init();
    }
    /// <summary>
    /// 表容器，用于自动化构建
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class TableContainer<T> : ITableContainer where T : class
    {
        private string _name = null;
        private readonly Dictionary<int, T> _table = new Dictionary<int, T>();

        public delegate TextAsset LoadDeleg(string path);
        public static LoadDeleg LoadRes;
        public string Name
        {
            get { return _name; }
        }

        public int Count
        {
            get
            {
                return _table.Count;
            }
        }

        public Dictionary<int, T> Table
        {
            get { return _table; }
        }

        public Dictionary<int, T>.Enumerator GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        public T this[int id]
        {
            get
            {
                T tmp = null;
                _table.TryGetValue(id, out tmp);
                return tmp;
            }
        }

        public T GetField(int id)
        {
            T tmp = null;
            _table.TryGetValue(id, out tmp);
            return tmp;
        }

        public bool Init()
        {
            _table.Clear();
            var type = typeof (T);
            _name = type.Name.Replace("Table_", "Tables/");

            //本地数据优先读取
            var asset = Resources.Load<TextAsset>(_name);
            // 自定义加载
            if(!asset && LoadRes != null)  asset = LoadRes(_name);

            if (asset == null || asset.bytes == null) return false;
            var idField = typeof (T).GetField("Id");
            if (idField == null) return false;
            T[] parser = null;
            try
            {
                parser = CsvImporter.Parser<T>(asset.bytes);
            }
            catch (Exception e)
            {
                Debug.LogError("table：" + _name + "Error" + e.Message);
            }

            if (parser == null) return false;
            var count = parser.Length;
            for (var i = 0; i < count; i++)
            {
                var id = (int)idField.GetValue(parser[i]);
                if (!_table.ContainsKey(id))
                {
                    _table.Add(id, parser[i]);
                }
                else
                {
                    Debug.LogWarningFormat("{0} table.ID {1} is duplicated!", _name, id);
                }
            }
            return true;
        }

        public bool InitLocal()
        {
            _table.Clear();
            var type = typeof(T);
            _name = type.Name.Replace("Table_", "Tables/");
            //本地数据优先读取
            var asset = Resources.Load<TextAsset>(_name);
            // 自定义加载
            if (!asset && LoadRes != null) asset = LoadRes(_name);
            if (asset == null || asset.bytes == null) return false;
            var idField = typeof(T).GetField("Id");
            if (idField == null) return false;
            T[] parser = null;
            try
            {
                parser = CsvImporter.Parser<T>(asset.bytes);
            }
            catch (Exception e)
            {
                Debug.LogError("table：" + _name + "Error" + e.Message);
            }

            if (parser == null) return false;
            var count = parser.Length;
            for (var i = 0; i < count; i++)
            {
                var id = (int)idField.GetValue(parser[i]);
                if (!_table.ContainsKey(id))
                {
                    _table.Add(id, parser[i]);
                }
                else
                {
                    Debug.LogWarningFormat("{0} table.ID {1} is duplicated!", _name, id);
                }
            }
            return true;
        }
    }
