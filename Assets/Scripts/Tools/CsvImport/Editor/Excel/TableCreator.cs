using System;
using System.Data;
using System.IO;
using Excel;
using UnityEditor;
using UnityEngine;

namespace GameFrame
{
    public class TableCreator
    {
        private readonly DataTable _table;
        public readonly string Name;
        private readonly int _rowCount;
        private readonly int _columnCount;
        private readonly string[] _types = null;
        private readonly string[] _names = null;
        private readonly string[] _describes = null;
        private readonly string[,] _datas = null;
        public TableCreator(string file)
        {
            var start = file.LastIndexOf("/", StringComparison.Ordinal);
            var end = file.LastIndexOf(".", StringComparison.Ordinal);
            Name = file.Substring(start + 1, end - start - 1);
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var excel = ExcelReaderFactory.CreateOpenXmlReader(stream);
                if (excel == null)
                {
                    Debug.LogErrorFormat("error to load excel {0}", Name);
                    return;
                }
                var dataset = excel.AsDataSet();
                _table = dataset.Tables[0];
            }
            if (_table == null) return;
            _columnCount = GetColumnCount();
            _rowCount = GetRowCount();
            _types = new string[_columnCount];
            _names = new string[_columnCount];
            _describes = new string[_columnCount];
            _datas = new string[_rowCount, _columnCount];
        }

        private int GetColumnCount()
        {
            int columnCount = -1;
            for (int col = 0; col < _table.Columns.Count; col++)
            {
                var type = _table.Rows[0][col].ToString();
                if (!string.IsNullOrEmpty(type)) columnCount = col;
            }
            return columnCount + 1;
        }

        private int GetRowCount()
        {
            int rowCount = -1;
            for (int row = 3; row < _table.Rows.Count; row++)
            {
                var type = _table.Rows[row][0].ToString();
                if (!string.IsNullOrEmpty(type)) rowCount = row;
            }
            return rowCount - 2;
        }

        public string GetCell(int row, int col)
        {
            return _table.Rows[row][col].ToString();
        }

        public void ImportCreate()
        {
            //读取数据
            ReadData();
            ExportTxt();
//            ExportCSharp();
            Debug.LogFormat("字典表 \"{0}\" 已更新", Name);
        }

        private void ReadData()
        {
            // 处理类型
            for (int col = 0; col < _columnCount; col++)
            {
                _types[col] = GetCell(0, col);
            }
            // 处理名字
            for (int col = 0; col < _columnCount; col++)
            {
                _names[col] = GetCell(1, col);
            }
            // 处理描述
            for (int col = 0; col < _columnCount; col++)
            {
                _describes[col] = GetCell(2, col);
            }

            for (int row = 0; row < _rowCount; row++)
            {
                for (int col = 0; col < _columnCount; col++)
                {
                    _datas[row, col] = GetCell(row + 3, col);
                }
            }
        }

        private void ExportTxt()
        {
            var path = string.Format("{0}/Resources/Tables/{1}.txt", Application.dataPath, Name);
            var writer = new StringWriter();

            // 处理类型
            for (int col = 0; col < _columnCount; col++)
            {
                writer.Write(_types[col]);
                if (col != _columnCount - 1) writer.Write("\t");
            }
            writer.WriteLine();
            // 处理名字
            for (int col = 0; col < _columnCount; col++)
            {
                writer.Write(_names[col]);
                if (col != _columnCount - 1) writer.Write("\t");
            }
            writer.WriteLine();
            // 处理描述
            for (int col = 0; col < _columnCount; col++)
            {
                writer.Write(_describes[col]);
                if (col != _columnCount - 1) writer.Write("\t");
            }
            writer.WriteLine();
            for (int row = 0; row < _rowCount; row++)
            {
                for (int col = 0; col < _columnCount; col++)
                {
                    writer.Write(_datas[row, col]);
                    if (col != _columnCount - 1) writer.Write("\t");
                }
                writer.WriteLine();
            }
            
            File.WriteAllText(path, writer.ToString());
//            EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<TextAsset>(path.Replace(Application.dataPath, "Assets")));
        }

        private void ExportCSharp()
        {
            var path = string.Format("{0}/Script/GameFrameLogic/GameTable/Table_{1}.cs", Application.dataPath, Name);
            var writer = new StringWriter();
            writer.WriteLine("using System;");
            writer.WriteLine();
            writer.WriteLine("[Serializable]");
            writer.WriteLine("public class Table_{0}", Name);
            writer.WriteLine("{");
            for (int col = 0; col < _columnCount; col++)
            {
                writer.WriteLine("\t/// <summary>");
                writer.WriteLine("\t///{0}", _describes[col]);
                writer.WriteLine("\t/// </summary>");
                writer.WriteLine("\t[ColumnMapping(\"{0}\")]", _names[col]);
                writer.WriteLine("\tpublic\t{0}\t{1};", FormatTypesName(_types[col]), _names[col]);
                writer.WriteLine();
            }
            writer.WriteLine("}");
            File.WriteAllText(path, writer.ToString());
        }

        private static Type SortoutTypes(string typeName)
        {
            if (typeName.StartsWith("string(")) return typeof(string);
            switch (typeName)
            {
                case "int":
                    return typeof(int);
                case "float":
                    return typeof(float);
                case "intarray":
                    return typeof(int[]);
                default:
                    Debug.LogErrorFormat("无法处理的类型{0}", typeName);
                    return null;
            }
        }

        private static string FormatTypesName(string typeName)
        {
            if (typeName.StartsWith("string(")) return "string";
            switch (typeName)
            {
                case "int":
                    return "int";
                case "float":
                    return "float";
                case "intarray":
                    return "int[]";
                default:
                    Debug.LogErrorFormat("无法处理的类型{0}", typeName);
                    return null;
            }
        }

    }
}