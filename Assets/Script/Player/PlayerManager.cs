using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    /// <summary>
    /// 玩家列表
    /// </summary>
    public Dictionary<uint, Player> players=new Dictionary<uint, Player>();
    /// <summary>
    /// 获取玩家
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public Player GetPlayer(uint guid)
    {
        if (players.ContainsKey(guid))
            return players[guid];
        return null;
    }
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="player"></param>
    public void Add(uint guid, Player player)
    {
        players.Add(guid, player);
    }
}
