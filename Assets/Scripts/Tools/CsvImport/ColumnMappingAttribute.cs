using System;
using System.Collections.Generic;
using System.Reflection;
using Kent.Boogaart.KBCsv;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ColumnMappingAttribute : Attribute
{
    private static readonly Dictionary<Type, Action<ColumnMappingAttribute,HeaderRecord>> s_inits
         = new Dictionary<Type, Action<ColumnMappingAttribute, HeaderRecord>>();

    static ColumnMappingAttribute()
    {
        var methods = typeof(ColumnMappingAttribute).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
        var methodCount = methods.Length;
        for (int i = 0; i < methodCount; i++)
        {
            var methodInfo = methods[i];
            ParameterInfo[] parameters;
            if (methodInfo.Name.StartsWith("Init")
                && !methodInfo.Name.Equals("Init")
                && (parameters = methodInfo.GetParameters()).Length == 2)
            {
                var tname = methodInfo.Name.Substring(4, methodInfo.Name.Length - 4);
                var type = Type.GetType(tname, false);
                if (type == null)
                    type = Type.GetType("System." + tname, false);
                if (type == null)
                    type = Assembly.Load("UnityEngine").GetType("UnityEngine." + tname, false);
                if (type == null)
                {
                    if (tname.EndsWith("Array"))
                    {
                        type = Type.GetType("System." + tname.Substring(0, tname.Length - 5) + "[]", false);
                    }
                }
                if (type != null
                    && parameters[0].ParameterType == typeof(ColumnMappingAttribute)
                    && parameters[1].ParameterType == typeof(HeaderRecord))
                {
                    if (!s_inits.ContainsKey(type))
                    {
                        s_inits.Add(
                            type,
                            (Action<ColumnMappingAttribute, HeaderRecord>)
                            Delegate.CreateDelegate(typeof(Action<ColumnMappingAttribute, HeaderRecord>), methodInfo)
                            );
                    }
                }
                else
                {
                    Debug.LogErrorFormat("ColumnMappingAttribute Failed,type = null{0},", tname);
                }
            }
        }
    }

    private bool isinited;
    private string[] m_header;
    private Func<DataRecord, object> getObject;
    public FieldInfo TargetField;

    public object ImportParam;

    public string Header
    {
        get
        {
            if (m_header == null || m_header.Length == 0)
            {
                return "";
            }
            else
            {
                return m_header[0];
            }
        }
    }

    public ColumnMappingAttribute() : this(null)
    {
        
    }

    public ColumnMappingAttribute(params string[] header)
    {
        m_header = header;
        if(m_header == null)
            m_header = new string[0];
    }

    public void ParseTo(DataRecord record, object target)
    {
        var fvalue = getObject(record);
        this.TargetField.SetValue(target, fvalue);
    }

    public void Init(HeaderRecord headers)
    {
        if(this.isinited)
            throw new InvalidOperationException();
        
        Action<ColumnMappingAttribute, HeaderRecord> initHandler;
        if (s_inits.TryGetValue(this.TargetField.FieldType, out initHandler))
            initHandler(this, headers);
        else
            Debug.LogErrorFormat("typeof:" + this.TargetField.DeclaringType + ", with field:" + this.TargetField.Name + " not support");
        
        this.isinited = true;
    }

    private static void InitString(ColumnMappingAttribute attribute, HeaderRecord record)
    {
        int index = SearchHeadIndex(attribute, record);

        if(index != -1)
                attribute.getObject = r => r[index];
        else
            attribute.getObject = r => string.Empty;
    }
    
    private static void InitInt32Array(ColumnMappingAttribute attribute, HeaderRecord record)
    {
        int index = SearchHeadIndex(attribute, record);
        if (index != -1)

        attribute.getObject = r =>
                                  {
                                      var str = r[index];
                                      var strs = str.Split('_');
                                      var count = strs.Length;
									if(string.IsNullOrEmpty(str))
									{
										count = 0;
									}
                                      int[] values = new int[count];
                                      int id;
                                      for (int i = 0; i < count; i++)
                                      {
                                          if(string.IsNullOrEmpty(strs[i]))
                                          {
                                              values[i] = 0;
                                              continue;
                                          }
                                          if(int.TryParse(strs[i],out id))
                                          {
                                              values[i] = id;
                                          }
                                          else
                                          {
                                              Debug.LogErrorFormat("InitInt32Array int.TryParse(strs[i],out id Failed,strs:{0}:{1}:Count is {2}", str,attribute.TargetField.Name,count);
                                          }
                                      }
                                      return values;
                                  };
        else
            attribute.getObject = r => string.Empty;
    }

    private static void InitSingleArray(ColumnMappingAttribute attribute, HeaderRecord record)
    {
        int index = SearchHeadIndex(attribute, record);
        if (index != -1)

            attribute.getObject = r =>
            {
                var str = r[index];
                var strs = str.Split('_');
                var count = strs.Length;
                float[] values = new float[count];
                float id;
                for (int i = 0; i < count; i++)
                {
                    if (string.IsNullOrEmpty(strs[i]))
                    {
                        values[i] = 0f;
                        continue;
                    }
                    if (float.TryParse(strs[i], out id))
                    {
                        values[i] = id;
                    }
                    else
                    {
                        Debug.LogErrorFormat("InitSingleArray float.TryParse(strs[i],out id Failed,strs:{0}:{1}:Count is {2}", str, attribute.TargetField.Name, count);
                    }
                }
                return values;
            };
        else
            attribute.getObject = r => string.Empty;
    }

    private static void InitBoolean(ColumnMappingAttribute attribute, HeaderRecord record)
    {
        var importP = attribute.ImportParam as string;
        if(importP != null)
        {
            int index = SearchHeadIndex(attribute, record);

            if (index != -1)
                attribute.getObject = r => string.Equals(r[index], importP, StringComparison.InvariantCultureIgnoreCase);
            else
                attribute.getObject = r => false;
        }else
            ConverToInternal<bool>(attribute, record);
    }

    private static void InitInt32(ColumnMappingAttribute attribute, HeaderRecord record)
    { ConverToInternal<int>(attribute, record); }

    private static void InitSingle(ColumnMappingAttribute attribute, HeaderRecord record)
    { ConverToInternal<float>(attribute, record); }

    private static void InitAudioClip(ColumnMappingAttribute attribute, HeaderRecord record)
    {
        LoadObjectFromResource(attribute,record);
    }

    private static void InitTexture2D(ColumnMappingAttribute attribute, HeaderRecord record)
    {
        LoadObjectFromResource(attribute,record);
    }
    private static void InitGameObject(ColumnMappingAttribute attribute, HeaderRecord record)
    {
        LoadObjectFromResource(attribute, record);
    }

    private static int SearchHeadIndex(ColumnMappingAttribute attribute, HeaderRecord record)
    {
        var searchs = new List<string>();
        if (attribute.m_header.Length == 0)
            searchs.Add(attribute.TargetField.Name);
        else
            searchs.AddRange(attribute.m_header);

        var index = -1;
        var searchCount = searchs.Count;
        for (int i = 0; i < searchCount; i++)
        {
            var search = searchs[i];
            if ((index = record.IndexOf(search)) != -1)
                return index;
        }
        return index;
    }

    private static void ConverToInternal<T>(ColumnMappingAttribute attribute, HeaderRecord record)
    {
        int index = SearchHeadIndex(attribute, record);

        var type = typeof (T);
        if (index != -1)
            attribute.getObject = r =>
                                      {
                                          try { return Convert.ChangeType(r[index], type); }
                                          catch (Exception) { return default(T); }
                                      };
        else
            attribute.getObject = r => default(T);
    }

    private static void LoadObjectFromResource(ColumnMappingAttribute attribute, HeaderRecord record)
    {
        int index = SearchHeadIndex(attribute, record);

        if(index != -1)
                attribute.getObject = r =>
                                          {
                                              var path = r[index];
                                              if(string.IsNullOrEmpty(path))
                                                  return null;

                                              var format = attribute.ImportParam as String;
                                              if(format != null)
                                                  path = string.Format(format, path);

                                              return Resources.Load(path);
                                          };
        else
            attribute.getObject = r => null;
    }
}