

// class Program
// {
//     public static void Main()
//     {
//         Console.WriteLine("Welcome To The In-Memory Database");
//         Console.WriteLine("Please Enter Your Command Below");
//         Console.WriteLine("Type HELP For A List Of Commands");
//         Console.WriteLine();

//         InMemDb database = new InMemDb();

//         string? input = Console.ReadLine();
//         while (input != Commands.END)
//         {
//             if (input == null || String.IsNullOrWhiteSpace(input))
//             {
//                 System.Console.WriteLine("Please Enter A Command Or Type END To Finish");
//                 input = Console.ReadLine();
//                 continue;
//             }
            
//             //split the input, and remove any blank array spaces
//             var split = input.Split(' ').Where(s => !String.IsNullOrWhiteSpace(s)).ToArray();
            
//             switch (split[0].ToUpper())
//             {
//                 case Commands.SET:
//                     if (split.Length != 3)
//                     {
//                         IncorrectArguments(Commands.SET, split.Length, 3);
//                         break;
//                     }
//                     database.Set(split[1], split[2]);
//                     break;

//                 case Commands.GET:
//                     if (split.Length != 2)
//                     {
//                         IncorrectArguments(Commands.GET, split.Length, 2);
//                         break;
//                     }
//                     //I set null here to not hardcode it in the Get function
//                     Console.WriteLine(database.Get(split[1]) ?? "NULL");
//                     break;

//                 case Commands.DELETE:
//                     if (split.Length != 2)
//                     {
//                         IncorrectArguments(Commands.DELETE, split.Length, 2);
//                         break;
//                     }
//                     database.Delete(split[1]);
//                     break;
//                 case Commands.COUNT:
//                     if (split.Length != 2)
//                     {
//                         IncorrectArguments(Commands.COUNT, split.Length, 2);
//                         break;
//                     }
//                     Console.WriteLine(database.Count(split[1]));
//                     break;

//                 case Commands.END:
//                     System.Console.WriteLine("Application Ending...");
//                     Environment.Exit(0);
//                     break;

//                 case Commands.BEGIN_TRANSACTION:
//                     database.CreateTransaction();
//                     break;

//                 case Commands.ROLLBACK_TRANSACTION:
//                     database.RollbackTransaction();
//                     break;

//                 case Commands.COMMIT_ALL_TRANSACTIONS:
//                     database.Commit();
//                     break;

//                 case Commands.HELP:
//                     PrintHelp();
//                     break;

//                 case Commands.RUN_TESTS:
//                     var unitTest = new UnitTests();
//                     unitTest.Tests();
//                     break;

//                 default:
//                     System.Console.WriteLine("Unknown Command. Please Type HELP For A List Of Commands Or END To Finish");
//                     break;
//             }

//             input = Console.ReadLine();
//         }
//     }


//     private static void IncorrectArguments(string command, int parameters, int expectedParameters)
//     {
//         Console.WriteLine($"Invalid Number Of Arguments For Command {command}. Expected {expectedParameters} | Got {parameters}");
//     }

//     private static void PrintHelp()
//     {
//         Console.WriteLine("SET [key] [value]");
//         Console.WriteLine("GET [key]");
//         Console.WriteLine("DELETE [key]");
//         Console.WriteLine("COUNT [value]");
//         Console.WriteLine("BEGIN");
//         Console.WriteLine("ROLLBACK");
//         Console.WriteLine("COMMIT");
//         Console.WriteLine("END");
//         Console.WriteLine();
//     }
// }

// public sealed class InMemDb
// {
//     //We have a master transaction for the root database
//     private MasterTransaction MasterTransaction;
//     //We keep track of all other transactions here
//     private List<Transaction> SubTransactions;

//     public InMemDb()
//     {
//         MasterTransaction = new MasterTransaction();
//         SubTransactions = new List<Transaction>();
//     }

//     /// <summary>
//     /// Sets key in database with value
//     /// </summary>
//     /// <param name="key">Key to set</param>
//     /// <param name="value">Value to set</param>
//     public void Set(string key, string value)
//     {
//         var curTransaction = GetCurrentTransaction();
//         curTransaction.Set(key, value);
//     }

//     /// <summary>
//     /// Gets value from database based on key
//     /// </summary>
//     /// <param name="key"></param>
//     /// <returns></returns>
//     public string? Get(string key)
//     {
//         var val = MasterTransaction.Get(key);

//         if(!SubTransactions.Any())
//             return val?.Value;

//         //traverse subtransactions in reverse to get most updated version
//         for(var i = SubTransactions.Count - 1; i >= 0; i--)
//         {
//             var transaction = SubTransactions[i];
//             var tVal = transaction.Get(key);
//             if(tVal != null)
//                 return tVal?.Value;
//         }

//         //if none of the transactions have it then just return
//         return val?.Value;
//     }

//     /// <summary>
//     /// Deletes the key from the database
//     /// </summary>
//     /// <param name="key">Key to remove</param>
//     public void Delete(string key)
//     {
//         var curTransaction = GetCurrentTransaction();
//         curTransaction.Delete(key);
//     }

//     /// <summary>
//     /// Return the COUNT of keys that have value
//     /// </summary>
//     /// <param name="value">Value to find</param>
//     /// <returns>Count of values</returns>
//     public int Count(string value)
//     {
//         if (string.IsNullOrWhiteSpace(value))
//             return 0;

//         var rawData = MasterTransaction.GetData();
//         if (SubTransactions.Any())
//         {
//             foreach(var transaction in SubTransactions)
//             {
//                 rawData = transaction.PeekChanges(rawData);
//             }
//         }

//         return rawData.Where(x => x.Value == value).Count();
//     }

//     public void CreateTransaction()
//     {
//         SubTransactions.Add(new Transaction());
//     }

//     public void RollbackTransaction()
//     {
//         if(!SubTransactions.Any())
//             return;
//         SubTransactions.RemoveAt(SubTransactions.Count - 1);
//     }

//     //we just make Commit return a value for unit testing
//     public Dictionary<string, string> Commit()
//     {
//         //if there's no sub transactions then we don't care since our master transaction contains our base db
//         if (!SubTransactions.Any())
//             return MasterTransaction.GetData();

//         Dictionary<string, string> result = MasterTransaction.GetData();
//         foreach (var transaction in SubTransactions)
//         {
//             result = transaction.MergeChanges(result);
//         }

//         //reset subtransactions and let GC take care of the clean up
//         SubTransactions = new List<Transaction>();

//         //pass the merged to the master transaction
//         MasterTransaction.UpdateData(result);

//         return result;
//     }

//     /// <summary>
//     /// Get the value taking transactions into consideration
//     /// </summary>
//     /// <param name="key">Key to get</param>
//     /// <returns>Value of key</returns>
//     private string? FindValue(string key)
//     {
//         var defaultVal = MasterTransaction.Get(key);
//         if (!SubTransactions.Any())
//         {
//             return defaultVal?.Value;
//         }
//         else
//         {
//             //since we store the transactions with the newest first, we just iterate through them all until we find the key
//             foreach (var transaction in SubTransactions)
//             {
//                 //Check if the transaction has modified the value
//                 var value = transaction.Get(key);
//                 if (value != null)
//                     return value?.Value;
//             }
//             return defaultVal?.Value;
//         }
//     }

//     private Transaction GetCurrentTransaction()
//     {
//         //return the last transaction since that's the latest
//         if (SubTransactions.Any())
//             return SubTransactions.Last();
//         return MasterTransaction;
//     }
// }

// public class MasterTransaction : Transaction
// {
//     public void UpdateData(Dictionary<string, string> data)
//     {
//         Data = data;
//     }
// }

// public class Transaction
// {
//     protected Dictionary<string, string> Data;

//     public Transaction()
//     {
//         Data = new Dictionary<string, string>();
//     }

//     public KeyValuePair<string, string>? Get(string key)
//     {
//         //return a keyvaluepair to show that the key exists, but the data may have been deleted
//         if (Data.ContainsKey(key))
//             return new KeyValuePair<string, string>(key, Data[key]);

//         return null;
//     }

//     public void Set(string key, string value)
//     {
//         Data[key] = value;
//     }

//     public Dictionary<string, string> GetData()
//     {
//         return Data;
//     }

//     public void Delete(string key)
//     {
//         //set this to null so we still have the key but no data
//         //used to help get
//         Data[key] = null;
//     }

//     public HashSet<string> FindValue(string value)
//     {
//         var found = Data.Where(kv => kv.Value == value);

//         if (found == null || !found.Any())
//             return new HashSet<string>();

//         //return the keys of the found value
//         return new HashSet<string>(found.Select(k => k.Key));
//     }

//     public Dictionary<string, string> PeekChanges(Dictionary<string, string> prevTransactionData)
//     {
//         if (prevTransactionData == null || prevTransactionData.Count == 0)
//             return Data;
//         //temporarly duplicate current changes since Peek shouldn't change source data
//         var dict = new Dictionary<string, string>(prevTransactionData);

//         foreach (var key in Data.Keys)
//         {
//             dict[key] = Data[key];
//         }

//         return dict;
//     }

//     public Dictionary<string, string> MergeChanges(Dictionary<string, string> changes)
//     {
//         if (changes == null || changes.Count == 0)
//             return Data;

//         foreach (var keyVal in changes)
//         {
//             //If the key from older doesn't exist in this then add the value
//             if (!Data.ContainsKey(keyVal.Key))
//             {
//                 Data[keyVal.Key] = keyVal.Value;
//             }
//         }

//         return Data;
//     }
// }

// public static class Commands
// {
//     public const string END = "END";
//     public const string SET = "SET";
//     public const string GET = "GET";
//     public const string HELP = "HELP";
//     public const string RUN_TESTS = "TESTS";
//     public const string DELETE = "DELETE";
//     public const string COUNT = "COUNT";
//     public const string BEGIN_TRANSACTION = "BEGIN";
//     public const string ROLLBACK_TRANSACTION = "ROLLBACK";
//     public const string COMMIT_ALL_TRANSACTIONS = "COMMIT";
// }

// using System.Data.Common;
// using System.Diagnostics;


// //for some reason i couldn't add a unit test framework to my project
// //so i cobbled together a simple testing to test the 4 examples on the sheet

// public abstract class UnitTest
// {
//     public abstract bool RunTest();
//     private InMemDb db = new InMemDb();

//     public bool Run(Action<InMemDb> func)
//     {
//         try
//         {
//             func.Invoke(db);
//         }
//         catch (Exception e)
//         {
//             System.Console.WriteLine(e.Message);
//             throw;
//         }
//         return true;
//     }

//     public bool Expected<T>(T got, T expected)
//     {
//         if (expected == null && got == null)
//             return true;

//         if (!expected?.Equals(got) ?? true)
//             throw new Exception($"Expected was {expected} | Got was {got}");

//         return true;
//     }

//     public void TimeMethod(Action action, string name)
//     {
//         var timer = new Stopwatch();
//         timer.Start();
//         action.Invoke();
//         timer.Stop();
//         System.Console.WriteLine($"Time Taken For {name}: " + timer.Elapsed.TotalMilliseconds.ToString() + "ms");
//     }
// }

// public class Example1 : UnitTest
// {
//     public override bool RunTest()
//     {
//         var db = new InMemDb();

//         return Run(db =>
//         {
//             Expected(db.Get("a"), null);
//             db.Set("a", "foo");
//             db.Set("b", "foo");
//             Expected(db.Count("foo"), 2);
//             Expected(db.Count("bar"), 0);
//             db.Delete("a");
//             Expected(db.Count("foo"), 1);
//             db.Set("b", "baz");
//             Expected(db.Count("foo"), 0);
//             Expected(db.Get("b"), "baz");
//             Expected(db.Get("B"), null);
//         });


//     }

// }


// public class Example2 : UnitTest
// {
//     public override bool RunTest()
//     {
//         return Run(db =>
//         {
//             db.Set("a", "foo");
//             db.Set("a", "foo");
//             Expected(db.Count("foo"), 1);
//             Expected(db.Get("a"), "foo");
//             db.Delete("a");
//             Expected(db.Get("a"), null);
//             Expected(db.Count("a"), 0);
//         });
//     }
// }

// public class Example3 : UnitTest
// {
//     public override bool RunTest()
//     {
//         return Run(db =>
//         {
//             db.CreateTransaction();
//             db.Set("a", "foo");
//             Expected(db.Get("a"), "foo");
//             db.CreateTransaction();
//             db.Set("a", "bar");
//             Expected(db.Get("a"), "bar");
//             db.RollbackTransaction();
//             Expected(db.Get("a"), "foo");
//             db.RollbackTransaction();
//             Expected(db.Get("a"), null);
//         });
//     }
// }

// public class Example4 : UnitTest
// {
//     public override bool RunTest()
//     {
//         return Run(db =>
//         {
//             db.Set("a", "foo");
//             db.Set("b", "baz");
//             db.CreateTransaction();
//             Expected(db.Get("a"), "foo");
//             db.Set("a", "bar");
//             Expected(db.Count("bar"), 1);
//             db.CreateTransaction();
//             Expected(db.Count("bar"), 1);
//             db.Delete("a");
//             Expected(db.Get("a"), null);
//             Expected(db.Count("bar"), 0);
//             db.RollbackTransaction();
//             Expected(db.Get("a"), "bar");
//             Expected(db.Count("bar"), 1);
//             db.Commit();
//             Expected(db.Get("a"), "bar");
//             Expected(db.Get("b"), "baz");
//         });
//     }
// }

// public class CustomExample : UnitTest
// {
//     public override bool RunTest()
//     {
//         return Run(db =>
//         {
//             db.Set("a", "xyz");
//             Expected(db.Get("a"), "xyz");

//             db.Set("b!", "");
//         });
//     }
// }


// public class Timings : UnitTest
// {
//     public override bool RunTest()
//     {
//         var stopwatch = new Stopwatch();

//         Run(db =>
//         {
//             db.Set("a", "test");
//             db.Set("b", "test");
//             db.Set("c", "test");

//             TimeMethod(() => { db.Get("a"); }, "single get with 3");
//             TimeMethod(() => { db.Count("test"); }, "count with 3");
//             TimeMethod(() => { db.Delete("a"); }, "single delete with 3");
//             TimeMethod(() => { db.Set("a", "test2"); }, "single set with 3");

//             db.Set("d", "test");
//             db.Set("e", "test");
//             db.Set("f", "test");
//             db.Set("g", "test");
//             db.Set("h", "test");
//             db.Set("i", "test");
//             db.Set("j", "test");
//             db.Set("k", "test");
//             db.Set("l", "test");
//             db.Set("m", "test");
//             db.Set("n", "test");
//             db.Set("o", "test");
//             db.Set("p", "test");
//             db.Set("q", "test");
//             db.Set("r", "test");
//             db.Set("s", "test");
//             db.Set("t", "test");
//             db.Set("u", "test");
//             db.Set("v", "test");
//             db.Set("x", "test");
//             db.Set("y", "test");
//             db.Set("z", "test");

//             TimeMethod(() => { db.Get("z"); }, "single get with 26");
//             TimeMethod(() => { db.Count("test"); }, "count with 26");
//             TimeMethod(() => { db.Delete("a"); }, "single delete with 26");
//             TimeMethod(() => { db.Set("a", "test3"); }, "single set with 26");

//             //add 1000 more records
//             for (var i = 1; i < 1000; i++)
//             {
//                 db.Set(i.ToString(), "test4");
//             }

//             TimeMethod(() => { db.Get("999"); }, "single get with 1026");
//             TimeMethod(() => { db.Count("test"); }, "count with 1026");
//             TimeMethod(() => { db.Delete("888"); }, "single delete with 1026");
//             TimeMethod(() => { db.Set("888", "test3"); }, "single set with 1026");

//             //add 10000 more records
//             for (var i = 1; i < 10000; i++)
//             {
//                 db.Set("x" + i.ToString(), "test4");
//             }

//             TimeMethod(() => { db.Get("9999"); }, "single get with 11026");
//             TimeMethod(() => { db.Count("test4"); }, "count with 11026");
//             TimeMethod(() => { db.Delete("8888"); }, "single delete with 11026");
//             TimeMethod(() => { db.Set("8888", "test3"); }, "single set with 11026");

//             db.CreateTransaction();

//             for (var i = 1; i < 1000; i++)
//             {
//                 db.Set("t" + i.ToString(), "test4");
//             }

//             TimeMethod(() => { db.Get("9999"); }, "transaction get with 12026");
//             TimeMethod(() => { db.Count("test4"); }, "transaction count with 12026");
//             TimeMethod(() => { db.Delete("8888"); }, "transaction delete with 12026");
//             TimeMethod(() => { db.Set("8888", "test3"); }, "transaction set with 12026");

//             TimeMethod(() => db.Commit(), "transaction commiting 1000 records");

//         });
//         return true;
//     }
// }

// public class UnitTests
// {
//     public void Tests()
//     {
//         var tests = new List<UnitTest>
//         {
//             new Example1(),
//             new Example2(),
//             new Example3(),
//             new Example4(),
//             new Timings()
//         };


//         foreach (var test in tests)
//         {
//             var result = test.RunTest();
//             System.Console.WriteLine($"{test.GetType().Name} Was {(result ? "Successful" : "Unsuccessful")}");
//         }
//     }
// }