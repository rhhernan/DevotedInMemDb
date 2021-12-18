public class MasterTransaction : Transaction
{
    public void UpdateData(Dictionary<string, string> data)
    {
        Data = data;
    }
}

public class Transaction
{
    protected Dictionary<string, string> Data;

    public Transaction()
    {
        Data = new Dictionary<string, string>();
    }

    public KeyValuePair<string, string>? Get(string key)
    {
        //return a keyvaluepair to show that the key exists, but the data may have been deleted
        if (Data.ContainsKey(key))
            return new KeyValuePair<string, string>(key, Data[key]);

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
        //set this to null so we still have the key but no data
        //used to help get
        Data[key] = null;
    }

    public HashSet<string> FindValue(string value)
    {
        var found = Data.Where(kv => kv.Value == value);

        if (found == null || !found.Any())
            return new HashSet<string>();

        //return the keys of the found value
        return new HashSet<string>(found.Select(k => k.Key));
    }

    public Dictionary<string, string> PeekChanges(Dictionary<string, string> prevTransactionData)
    {
        if (prevTransactionData == null || prevTransactionData.Count == 0)
            return Data;
        //temporarly duplicate current changes since Peek shouldn't change source data
        var dict = new Dictionary<string, string>(prevTransactionData);

        foreach (var key in Data.Keys)
        {
            dict[key] = Data[key];
        }

        return dict;
    }

    public Dictionary<string, string> MergeChanges(Dictionary<string, string> changes)
    {
        if (changes == null || changes.Count == 0)
            return Data;

        foreach (var keyVal in changes)
        {
            //If the key from older doesn't exist in this then add the value
            if (!Data.ContainsKey(keyVal.Key))
            {
                Data[keyVal.Key] = keyVal.Value;
            }
        }

        return Data;
    }
}