using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameFrame
{
    public class TableImporter : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var tables = new List<TableCreator>();
            for (var i = 0; i < importedAssets.Length; i++)
            {
                if (!importedAssets[i].StartsWith("Assets/Excels/")) continue;
                if (importedAssets[i].StartsWith("Assets/Excels/~$")) continue;
                var file = importedAssets[i].Replace("Assets", Application.dataPath);
                tables.Add(new TableCreator(file));
            }
            for (var i = 0; i < tables.Count; i++)
            {
                EditorUtility.DisplayProgressBar("导入数据表", tables[i].Name, Mathf.InverseLerp(0, tables.Count, i));
                tables[i].ImportCreate();
            }
            for (int i = 0; i < deletedAssets.Length; i++)
            {
                if (deletedAssets[i].Contains(".xlsx"))
                {
                    DeleteTxt(deletedAssets[i]);
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void DeleteTxt(string path)
        {
            path = path.Substring(path.LastIndexOf('/'), path.Length - path.LastIndexOf('/'));
            string tableName = path.Split('.')[0];
            if (File.Exists(Application.dataPath + "/Resources/Tables" + tableName + ".txt"))
            {
                File.Delete(Application.dataPath + "/Resources/Tables" + tableName + ".txt");
                Debug.Log("已删除：" + Application.dataPath + "/Resources/Tables" + tableName + ".txt");
                AssetDatabase.Refresh();
            }
        }
    }
}