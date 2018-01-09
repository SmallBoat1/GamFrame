/*  文件描述：消息接收器
 */

using System.Collections.Generic;

namespace GameFrame
{
    public interface MessageReceiver
    {
        GameMessage[] MainHobbys { get; }
        GameMessage[] SubHobbys { get; }
        void Process(List<object> buffer);         
    }
}