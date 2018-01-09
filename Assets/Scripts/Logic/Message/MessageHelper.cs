using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public class MessageHelper
    {
        /// <summary>
        /// 获取参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer">参数BUF</param>
        /// <param name="count">参数索引</param>
        /// <returns></returns>
        public static T GetNextParam<T>(object[] buffer, ref int count)
        {
            if (count >= buffer.Length)
            {
                return default(T);
            }
            try
            {
                var param = (T) Convert.ChangeType(buffer[count], typeof (T));
                count++;
                return param;
            }
            catch (InvalidCastException)
            {
                Debug.LogErrorFormat("GetNextParam error!MessageId:{0}.{1},param index:{2}", buffer[0], buffer[1], count);
            }
            return default(T);
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer">参数BUF</param>
        /// <param name="count">参数索引</param>
        /// <returns></returns>
        public static T GetNextParam<T>(List<object> buffer,ref int count)
        {
            if (count >= buffer.Count)
            {
                return default(T);
            }
            try
            {
                var param = (T)buffer[count];
                count++;
                return param;
            }
            catch (InvalidCastException)
            {
                Debug.LogErrorFormat("GetNextParam error!MessageId:{0}.{1},param index:{2}", buffer[0], buffer[1], count);
            }
            return default(T);
        }

        /// <summary>
        /// 获取消息类型
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="pos">0：为主类型(main)，1：为次类型(sub)</param>
        /// <returns></returns>
        public static GameMessage GetMessageType(Dictionary<int, List<object>> buffer,int index,int pos)
        {
            if (buffer == null)
            {
                return GameMessage.None;
            }

            if (buffer.Count == 0)
            {
                return GameMessage.None;
            }

            if (buffer[index].Count == 0)
            {
                return GameMessage.None;
            }

            if (pos > 1)
            {
                Debug.LogError("GetMessageType failed!error pos: " + pos);
                return GameMessage.None;
            }

            return (GameMessage)buffer[index][pos];
        }
    }
}