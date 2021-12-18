public sealed class InMemDb
{
    //We have a master transaction for the root database
    private Transaction MasterTransaction;
    //We keep track of all other transactions here
    private List<Transaction> SubTransactions;

    public InMemDb()
    {
        MasterTransaction = new Transaction();
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
        var curTransaction = GetCurrentTransaction();
        return curTransaction.Get(key);
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
        throw new NotImplementedException();
    }

    public void CreateTransaction()
    {
        SubTransactions.Insert(0, new Transaction());
    }

    public void RollbackTransaction()
    {
        SubTransactions.RemoveAt(0);
    }

    public void Commit()
    {
        //if there's no sub transactions then we don't care since our master transaction contains our base db
        if(!SubTransactions.Any())
            return;

        Dictionary<string, string> result = new Dictionary<string, string>();
        foreach(var transaction in SubTransactions)
        {
            result = transaction.MergeChanges(result);
        }

        MasterTransaction.MergeChanges(result);
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
            return defaultVal;
        }
        else
        {
            //since we store the transactions with the newest first, we just iterate through them all until we find the key
            foreach(var transaction in SubTransactions)
            {
                //Check if the transaction has modified the value
                var value = transaction.Get(key);
                if(value != null)
                    return value;
            }
            return defaultVal;
        }
    }

    private Transaction GetCurrentTransaction()
    {
        //return the first transaction since that's the latest
        if(SubTransactions.Any())
            return SubTransactions[0];
        return MasterTransaction;
    }
}