
public static class MyGuid
{
    private static uint _id = 0;
    private static HashSet<uint> _idHashSet = new HashSet<uint>();

    /// <summary>
    /// 分配Guid
    /// </summary>
    /// <returns></returns>
    public static uint GetGuid()
    {
        if (_id==uint.MaxValue)
            _id = 0;
        
        uint res = _id++;
        while (_idHashSet.Contains(res))
        {
            if (_id==uint.MaxValue)
                _id = 0;
            _id++;
        }
        _idHashSet.Add(res);

        return res;
    }
    
}