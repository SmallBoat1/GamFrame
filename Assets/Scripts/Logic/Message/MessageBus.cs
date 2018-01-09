/*文件描述：消息总线
        写入消息时注意参数格式：主消息ID，副消息ID，消息参数为字符串，参数与参数之间以空格分开，每个参数需注明参数类型，
 *      MessageBus.Instance.WriteMsg(1, 1001, "string^lantis string^26963 int^5533");
 *      消息ID统一从GameMessageType里去取，ID为0的消息默认不处理
 *      消息参数类型均为基本数据类型，支持（byte,bool,short,ushort,int,uint,float,long,ulong,double,string）
 *      读消息时注意参数类型，需以不同长度读取，具体可参见下面的例子
 *      消息总线每帧可处理100条消息（暂定），若后期发现不适会调整。每条消息处理时间不能超过1S，否则会放弃
     ==========================================
    this is a example!
    MessageBus.Instance.WriteMsg(1, 1001, "string:lantis string:26963 int:5533");
 * 
    public class Receiver : MessageReceiver
    {
        public GameMessage[] MainHobbys
        {
            get
            {
                return new[] { GameMessage.BasicAttribute };
            }
        }

        public void Process(byte[] buffer)
        {
            int len = 0;
 
            var mid = MessageHelper.GetMessageType(buffer,ref len);

            var sid = MessageHelper.GetMessageType(buffer,ref len);

            var account = MessageHelper.GetString(buffer,ref len, 6);

            var password = MessageHelper.GetString(buffer,ref len, 5);

            var port = MessageHelper.GetInter(buffer,ref len);

            Debug.LogFormat("mainId:{0},subId:{1},account:{2},password:{3},port:{4}", mid, sid, account, password, port);
        }
    }
    ==========================================
 * 
 * 修改:分割为^分割
 * 添加MessageHelper.MakeParams简化发消息的格式书写
 * 
 * 修改:数据类型readonly byte[][]改为Dictionary<int, List<object>>，支持自定义类型
 */

using UnityEngine;
using System.Collections.Generic;

namespace GameFrame
{
    public class MessageBus : Singleton<MessageBus>
    {
        public class SubHobbyDictionary
        {
            Dictionary<int, List<MessageReceiver>> subHobbys = new Dictionary<int, List<MessageReceiver>>();

            public SubHobbyDictionary(MessageReceiver receiver)
            {
                AddReceive(receiver);
            }

            public void AddReceive(MessageReceiver receiver)
            {
                var subHobby = receiver.SubHobbys;
                for (int i = 0; i < subHobby.Length; i++)
                {
                    if (!subHobbys.ContainsKey((int)subHobby[i]))
                    {
                        var receives = new List<MessageReceiver>();
                        receives.Add(receiver);
                        subHobbys.Add((int)subHobby[i], receives);
                    }
                    else
                    {
                        if (!subHobbys[(int)subHobby[i]].Contains(receiver))
                        {
                            subHobbys[(int)subHobby[i]].Add(receiver);
                        }
                    
//                        Debug.Log("M:"+Mainhobby+"S"+subHobby[i] + ",count:" + subHobbys[subHobby[i]].Count);
                    }
                }               
            }

            public void RemoveReceive(MessageReceiver receiver, GameMessage subhobby)
            {
                if (subHobbys.ContainsKey((int)subhobby))
                {
                    if (subHobbys[(int)subhobby].Contains(receiver))
                    {
                        subHobbys[(int)subhobby].Remove(receiver);
                    }
                }
            }

            public List<MessageReceiver> this[int subHobby]
            {
                get
                {
                    if (subHobbys.ContainsKey(subHobby))
                    {
                        return subHobbys[subHobby];
                    }
                    return new List<MessageReceiver>();
                }
            }
        }


        private int size;
        private int writeIndex;
        private int readIndex;
        private int processIndex;
        private const int cap = 256;
        private const int limit = 100;
        
        Dictionary<int, List<object>> msgBuffer = new Dictionary<int, List<object>>();
        private readonly Dictionary<int, SubHobbyDictionary> hobbyInfos = new Dictionary<int, SubHobbyDictionary>();
        private readonly List<MessageReceiver> receivers = new List<MessageReceiver>();
        private readonly List<object> bufListImmediately = new List<object>();

        public MessageBus()
        {
            for (int i = 0; i < cap; i++)
            {
                msgBuffer.Add(i, new List<object>());
            }
        }

        public void Init()
        {
        }

        public void Clear()
        {
            for (int i = 0; i < cap; i++)
            {
                msgBuffer[i].Clear();
            }
            bufListImmediately.Clear();
            size = 0;
            readIndex = 0;
            writeIndex = 0;
            processIndex = 0;
        }

        public bool CotainReceiver(MessageReceiver receiver)
        {
            if (receivers.Contains(receiver))
            {
                var targetreceive = receivers[receivers.IndexOf(receiver)];
                if (targetreceive != null)
                {
                    if (targetreceive == receiver)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void AddReceiver(MessageReceiver receiver)
        {
            if (receivers.Contains(receiver))
            {
                var targetreceive = receivers[receivers.IndexOf(receiver)];
                if (targetreceive != null)
                {
                    if (targetreceive == receiver)
                    {
                        Debug.LogError("AddReceiver failed!receiver type: " + targetreceive.GetType());
                        return;
                    }
                }
            }
            var mainhobby = receiver.MainHobbys;
            for (int i = 0; i < mainhobby.Length; i++)
            {
                if (!hobbyInfos.ContainsKey((int)mainhobby[i]))
                {
                    hobbyInfos.Add((int)mainhobby[i], new SubHobbyDictionary(receiver));
                }
                else
                {
                    hobbyInfos[(int)mainhobby[i]].AddReceive(receiver);
                }
            }
            receivers.Add(receiver);
        }

        public void RemoveReceiver(MessageReceiver receiver)
        {
            if (receivers.Contains(receiver))
            {
                receivers.Remove(receiver);
            }
            var mainhobby = receiver.MainHobbys;
            var subhobby = receiver.SubHobbys;
            for (int i = 0; i < mainhobby.Length; i++)
            {
                if (hobbyInfos.ContainsKey((int)mainhobby[i]))
                {
                    var info = hobbyInfos[(int)mainhobby[i]];
                    for (int j = 0; j < subhobby.Length; j++)
                    {
                        info.RemoveReceive(receiver, subhobby[j]);
                    }
                }
            }
        }

        public void RemoveReceiver<T>()
        {
            for (var index = 0; index < receivers.Count; index++)
            {
                var t = receivers[index];
                if (t is T)
                {
                    RemoveReceiver(t);
                }
            }
        }

        // 立即处理当前消息
        public void WriteMsgImmediately(GameMessage classid, GameMessage subid, params object[] args)
        {
            bufListImmediately.Clear();
            bufListImmediately.Add(classid);
            bufListImmediately.Add(subid);
            var length = args.Length;
            for (int i = 0; i < length; i++)
            {
                bufListImmediately.Add(args[i]);
            }
            ProcessImmediately(bufListImmediately);
        }

        private void ProcessImmediately(List<object> parambuf)
        {
            ProecssReceiver(parambuf);
        }

        // 不保证当前帧处理消息
        public void WriteMsg(GameMessage classid, GameMessage subid, params object[] args)
        {
            msgBuffer[writeIndex].Clear();
            msgBuffer[writeIndex].Add(classid);
            msgBuffer[writeIndex].Add(subid);
            for (int i = 0; i < args.Length; i++)
            {
                msgBuffer[writeIndex].Add(args[i]);
            }
            writeIndex++;
            size++;
            writeIndex = writeIndex % cap;
        }

        // 固定每帧处理
        public void ProcessFixedFrame()
        {
            int len = Mathf.Min(size, limit);

            for (int i = processIndex; i < processIndex + len; i++)
            {
                //                Debug.LogFormat("processIndex: {0},MainId: {1} SubId: {2}", i, MessageHelper.GetMessageType(msgBuffer, i, 0), MessageHelper.GetMessageType(msgBuffer, i, 1));
                ProecssReceiver(msgBuffer[readIndex]);
                Pop();
            }

            processIndex += len;
            processIndex %= cap;
        }

        private void ProecssReceiver(List<object> argsbuf)
        {
            if (argsbuf[0] == null)
            {
                Debug.LogError("ProecssReceiver failed! Don't have MainId");
                return;
            }

            if (argsbuf[1] == null)
            {
                Debug.LogError("ProecssReceiver failed! Don't have subId");
                return;
            }

            var mainId = (GameMessage)argsbuf[0];
            var subId = (GameMessage)argsbuf[1];

            if (hobbyInfos.ContainsKey((int)mainId))
            {
                var mainreceive = hobbyInfos[(int)mainId];
                var myreceives = mainreceive[(int)subId];

                for (int j = 0; j < myreceives.Count; j++)
                {
                    // Debug.Log(myreceives[j].GetType() + ":" + subId);
                    myreceives[j].Process(argsbuf);
                }
            }

        }

        private void Pop()
        {
            if (size <= 0)
            {
                return;
            }

            readIndex++;
            size--;
            readIndex = readIndex % cap;
        }
    }
}
