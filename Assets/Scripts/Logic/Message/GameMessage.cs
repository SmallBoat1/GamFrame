/*文件描述： 游戏消息类型
    主消息类分为
 *  GameFolw:游戏流程，一些游戏初始化、进入场景、退出场景等流程类消息
 *  UI：界面逻辑类消息，区别于UI系统内部的消息
 *  GameLogic：游戏逻辑类消息
 *  Network：网络类消息
 *  副消息由大家自己定义，注意区分在各大类中，开发过程中需加上注释，方便查询!!
 */

public enum GameMessage
{
    None,
    GameFlow = 1, //1-99
    GameSetting = 50,
    UI = 100, //101-499
    MainPlayer = 1000, //1000-1500
    Network = 1500,     // 网络功能
    FreshManLogic = 2000, //新手引导
    Sdk = 3000,
    Num,
}