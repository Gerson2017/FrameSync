

public static class RoomManager
{
    /// <summary>
    /// 最大房间号
    /// </summary>
    private static int _maxId;

    public static Dictionary<int, Room> MRooms = new Dictionary<int, Room>();

    public static Room GetRoom(int id)
    {
        if (MRooms.ContainsKey(id))
            return MRooms[id];
        return null;
    }


    /// <summary>
    /// 添加房间
    /// </summary>
    /// <returns></returns>
    public static Room AddRoom()
    {
        _maxId++;
        Room room = new Room();
        room.MId=_maxId;

        room.MLockStepManager.MRoom = room;
        MRooms.Add(_maxId,room);

        return room;
    }
    
    
}