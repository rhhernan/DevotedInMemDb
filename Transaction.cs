public class Transaction
{
    private Dictionary<string, string> Data;

    public Transaction()
    {
        Data = new Dictionary<string, string>();
    }

    public string? Get(string key)
    {
        if(Data.ContainsKey(key))
            return Data[key];

        return null;
    }

    public void Set(string key, string value)
    {
        Data[key] = value;
    }

    public Dictionary<string, string> GetData()
    {
        return Data;
    }

    public void Delete(string key)
    {
        Data.Remove(key);
    }

    public Dictionary<string, string> MergeChanges(Dictionary<string, string> changes)
    {
        if(changes == null || changes.Count == 0)
            return Data;

        foreach(var keyVal in changes)
        {
            if(Data.ContainsKey(keyVal.Key))
            {
                Data[keyVal.Key] = keyVal.Value;
            }
        }

        return Data;
    }
}