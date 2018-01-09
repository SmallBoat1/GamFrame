using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GameFrame
{
    public class TableManager
    {
        #region 常量与字段
        private readonly Dictionary<int, ITableContainer> _containers = new Dictionary<int, ITableContainer>();
        #endregion

        #region 属性
     

        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public void Init()
        {
            InitTable();
        }

        public void Destroy() { }
     
        public bool InitTable()
        {
            //反射到所有的表字段
            _containers.Clear();
            var fields = GetType().GetFields();
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var container = field.GetValue(this) as ITableContainer;
                if (container == null) continue;
                _containers.Add(container.GetType().GetHashCode(), container);    
            }

            //尝试初始化表
            var success = true;
            var e = _containers.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.Init()) continue;
                Debug.LogErrorFormat("can not load table {0}", e.Current.Value.Name);
                success = false;
            }
            return success;
        }

        /// <summary>
        /// 获取某表字段
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="id">table id</param>
        /// <param name="fieldName">字段名</param>
        /// <returns>字段值</returns>
        public object GetTableField(string tableName,int id,string fieldName)
        {
            Type gt = Type.GetType(tableName);
            if (gt == null)
            {
                return null;
            }
            MethodInfo mi = GetType().GetMethod("GetField");
            if (mi == null)
            {
                return null;
            }
            object table = mi.MakeGenericMethod(gt).Invoke(this, new object[] {id});
            if (table == null)
            {
                return null;
            }
            object name;
            try//防止表找不到该字段
            {
                name = table.GetType().GetField(fieldName).GetValue(table);
            }
            catch (Exception e)
            {
                Debug.LogError("table filed is Error!!:"+e);
                name = "";
            }
            return name;
        }

        /// <summary>
        /// 获取某个表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>返回对应的表容器</returns>
        public T GetTable<T>() where T : class
        {
            ITableContainer container = null;
            _containers.TryGetValue(typeof(T).GetHashCode(), out container);
            return container as T;
        }

        /// <summary>
        /// 获取某个表的字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetField<T>(int id) where T : class
        {
            ITableContainer container = null;
            if (!_containers.TryGetValue(typeof (TableContainer<T>).GetHashCode(), out container)) return null;
            var table = container as TableContainer<T>;
            return table != null ? table.GetField(id) : null;
        }

    }
}
