
public static class PlayerManager
{
    /// <summary>
    /// 所有玩家
    /// </summary>
    public static Dictionary<uint, Player> MPlayersDic = new Dictionary<uint, Player>();


    public static Player GetPlayer(uint guid)
    {
        if (MPlayersDic.ContainsKey(guid))
        {
            return MPlayersDic[guid];
        }

        return null;
    }


    public static void AddPlayer(uint guid,Player player)
    {
        if (MPlayersDic.ContainsKey(guid))
        {
            Console.WriteLine("AddPlayer Exit Player "+guid);
        }
        MPlayersDic[guid]=player;
    }

    public static void RemovePlayer(uint guid)
    {
        if (MPlayersDic.ContainsKey(guid))
        {
            MPlayersDic.Remove(guid);
        }

    }
    
    
}