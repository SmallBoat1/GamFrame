/* 文件描述：负责文件的写入和读取
 
 */

using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace GameFrame
{
    public class AssetFile
    {
        public enum ErrorCode
        {
            Success,
            Header,
            TooNew,
            TooOld,
        }

        private static readonly byte[] MagicCode = new byte[] { 0xFF, 0xF1, 0x64, 0x43 };
        private const int SystemVersion = 1;
        private readonly byte[] _code = new byte[4];

        private readonly byte[] _md5 = new byte[16];

        public byte[] Data { get; set; }

        public byte[] Md5
        {
            get { return _md5; }
            set { value.CopyTo(_md5, 0); }
        }

        public void Write(string filePath)
        {
            using (var fileStream = Create(filePath))
            {
                var sysver = BitConverter.GetBytes(SystemVersion);
                var lengthBytes = BitConverter.GetBytes(Data.Length);
                fileStream.Write(MagicCode, 0, MagicCode.Length);
                fileStream.Write(sysver, 0, sysver.Length);
                fileStream.Write(_md5, 0, _md5.Length);
                fileStream.Write(lengthBytes, 0, lengthBytes.Length);
                fileStream.Write(Data, 0, Data.Length);
            }
        }

        public ErrorCode Load(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.Position = 0;
                var error = ReadHeader(fileStream);
                if (error != ErrorCode.Success)
                {
                    Debug.LogFormat("asset file load error  {0}", error);
                    return error;
                }
                var lengthBytes = new byte[4];
                fileStream.Read(lengthBytes, 0, lengthBytes.Length);
                var length = BitConverter.ToInt32(lengthBytes, 0);
                Data = new byte[length];
                fileStream.Read(Data, 0, Data.Length);
                return error;
            }
        }

        public string GetMd5()
        {
            var str = new StringBuilder(32);
            for (int i = 0; i < _md5.Length; i++)
            {
                str.AppendFormat(_md5[i].ToString("x"));
            }
            
            return str.ToString();
        }

        private ErrorCode ReadHeader(Stream fileStream)
        {
            fileStream.Read(_code, 0, _code.Length);
            for (var i = 0; i < _code.Length; i++)
            {
                if (_code[i] != MagicCode[i])
                {
                    return ErrorCode.Header;
                }
            }
            var versionBytes = new byte[4];
            fileStream.Read(versionBytes, 0, versionBytes.Length);
            var version = BitConverter.ToInt32(versionBytes, 0);
            if (version != SystemVersion)
            {
                if (version > SystemVersion)
                {
                    return ErrorCode.TooNew;
                }
                else
                {
                    return ErrorCode.TooOld;
                }
            }
            fileStream.Read(_md5, 0, _md5.Length);
            return ErrorCode.Success;
        }

        private static FileStream Create(string filePath)
        {
            filePath = filePath.Replace('\\', '/');
            var path = filePath.Substring(0, filePath.LastIndexOf('/') + 1);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        }



        public ErrorCode LoadHeader(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.Position = 0;
                var error = ReadHeader(fileStream);
                return error;
            }
        }

        public static IEnumerator Download(string url, string path)
        {
            WWW www = null;
            while (true)
            {
                www = new WWW(url);
                while (!www.isDone) yield return null;
                if (string.IsNullOrEmpty(www.error)) break;
                Debug.LogErrorFormat("asset : {0} error : {1}", url, www.error);
            }
            using (var fileStream = Create(path))
            {
                fileStream.Write(www.bytes, 0, www.bytes.Length);
            }
        }
    }
}
