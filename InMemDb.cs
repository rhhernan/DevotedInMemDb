public sealed class InMemDb
{
    //We have a master transaction for the root database
    private MasterTransaction MasterTransaction;
    //We keep track of all other transactions here
    private List<Transaction> SubTransactions;

    public InMemDb()
    {
        MasterTransaction = new MasterTransaction();
        SubTransactions = new List<Transaction>();
    }

    /// <summary>
    /// Sets key in database with value
    /// </summary>
    /// <param name="key">Key to set</param>
    /// <param name="value">Value to set</param>
    public void Set(string key, string value)
    {
        var curTransaction = GetCurrentTransaction();
        curTransaction.Set(key, value);
    }

    /// <summary>
    /// Gets value from database based on key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string? Get(string key)
    {
        var val = MasterTransaction.Get(key);

        if(!SubTransactions.Any())
            return val?.Value;

        //traverse subtransactions in reverse to get most updated version
        for(var i = SubTransactions.Count - 1; i >= 0; i--)
        {
            var transaction = SubTransactions[i];
            var tVal = transaction.Get(key);
            if(tVal != null)
                return tVal?.Value;
        }

        //if none of the transactions have it then just return
        return val?.Value;
    }

    /// <summary>
    /// Deletes the key from the database
    /// </summary>
    /// <param name="key">Key to remove</param>
    public void Delete(string key)
    {
        var curTransaction = GetCurrentTransaction();
        curTransaction.Delete(key);
    }

    /// <summary>
    /// Return the COUNT of keys that have value
    /// </summary>
    /// <param name="value">Value to find</param>
    /// <returns>Count of values</returns>
    public int Count(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        var rawData = MasterTransaction.GetData();
        if (SubTransactions.Any())
        {
            foreach(var transaction in SubTransactions)
            {
                rawData = transaction.PeekChanges(rawData);
            }
        }

        return rawData.Where(x => x.Value == value).Count();
    }

    public void CreateTransaction()
    {
        SubTransactions.Add(new Transaction());
    }

    public void RollbackTransaction()
    {
        if(!SubTransactions.Any())
            return;
        SubTransactions.RemoveAt(SubTransactions.Count - 1);
    }

    //we just make Commit return a value for unit testing
    public Dictionary<string, string> Commit()
    {
        //if there's no sub transactions then we don't care since our master transaction contains our base db
        if (!SubTransactions.Any())
            return MasterTransaction.GetData();

        Dictionary<string, string> result = MasterTransaction.GetData();
        foreach (var transaction in SubTransactions)
        {
            result = transaction.MergeChanges(result);
        }

        //reset subtransactions and let GC take care of the clean up
        SubTransactions = new List<Transaction>();

        //pass the merged to the master transaction
        MasterTransaction.UpdateData(result);

        return result;
    }

    /// <summary>
    /// Get the value taking transactions into consideration
    /// </summary>
    /// <param name="key">Key to get</param>
    /// <returns>Value of key</returns>
    private string? FindValue(string key)
    {
        var defaultVal = MasterTransaction.Get(key);
        if (!SubTransactions.Any())
        {
            return defaultVal?.Value;
        }
        else
        {
            //since we store the transactions with the newest first, we just iterate through them all until we find the key
            foreach (var transaction in SubTransactions)
            {
                //Check if the transaction has modified the value
                var value = transaction.Get(key);
                if (value != null)
                    return value?.Value;
            }
            return defaultVal?.Value;
        }
    }

    private Transaction GetCurrentTransaction()
    {
        //return the last transaction since that's the latest
        if (SubTransactions.Any())
            return SubTransactions.Last();
        return MasterTransaction;
    }
}