using System.Data.Common;
using System.Diagnostics;


//for some reason i couldn't add a unit test framework to my project
//so i cobbled together a simple testing to test the 4 examples on the sheet

public abstract class UnitTest
{
    public abstract bool RunTest();
    private InMemDb db = new InMemDb();

    public bool Run(Action<InMemDb> func)
    {
        try
        {
            func.Invoke(db);
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
            throw;
        }
        return true;
    }

    public bool Expected<T>(T got, T expected)
    {
        if (expected == null && got == null)
            return true;

        if (!expected?.Equals(got) ?? true)
            throw new Exception($"Expected was {expected} | Got was {got}");

        return true;
    }

    public void TimeMethod(Action action, string name)
    {
        var timer = new Stopwatch();
        timer.Start();
        action.Invoke();
        timer.Stop();
        System.Console.WriteLine($"Time Taken For {name}: " + timer.Elapsed.TotalMilliseconds.ToString() + "ms");
    }
}

public class Example1 : UnitTest
{
    public override bool RunTest()
    {
        var db = new InMemDb();

        return Run(db =>
        {
            Expected(db.Get("a"), null);
            db.Set("a", "foo");
            db.Set("b", "foo");
            Expected(db.Count("foo"), 2);
            Expected(db.Count("bar"), 0);
            db.Delete("a");
            Expected(db.Count("foo"), 1);
            db.Set("b", "baz");
            Expected(db.Count("foo"), 0);
            Expected(db.Get("b"), "baz");
            Expected(db.Get("B"), null);
        });


    }

}


public class Example2 : UnitTest
{
    public override bool RunTest()
    {
        return Run(db =>
        {
            db.Set("a", "foo");
            db.Set("a", "foo");
            Expected(db.Count("foo"), 1);
            Expected(db.Get("a"), "foo");
            db.Delete("a");
            Expected(db.Get("a"), null);
            Expected(db.Count("a"), 0);
        });
    }
}

public class Example3 : UnitTest
{
    public override bool RunTest()
    {
        return Run(db =>
        {
            db.CreateTransaction();
            db.Set("a", "foo");
            Expected(db.Get("a"), "foo");
            db.CreateTransaction();
            db.Set("a", "bar");
            Expected(db.Get("a"), "bar");
            db.RollbackTransaction();
            Expected(db.Get("a"), "foo");
            db.RollbackTransaction();
            Expected(db.Get("a"), null);
        });
    }
}

public class Example4 : UnitTest
{
    public override bool RunTest()
    {
        return Run(db =>
        {
            db.Set("a", "foo");
            db.Set("b", "baz");
            db.CreateTransaction();
            Expected(db.Get("a"), "foo");
            db.Set("a", "bar");
            Expected(db.Count("bar"), 1);
            db.CreateTransaction();
            Expected(db.Count("bar"), 1);
            db.Delete("a");
            Expected(db.Get("a"), null);
            Expected(db.Count("bar"), 0);
            db.RollbackTransaction();
            Expected(db.Get("a"), "bar");
            Expected(db.Count("bar"), 1);
            db.Commit();
            Expected(db.Get("a"), "bar");
            Expected(db.Get("b"), "baz");
        });
    }
}

public class CustomExample : UnitTest
{
    public override bool RunTest()
    {
        return Run(db =>
        {
            db.Set("a", "xyz");
            Expected(db.Get("a"), "xyz");

            db.Set("b!", "");
        });
    }
}


public class Timings : UnitTest
{
    public override bool RunTest()
    {
        var stopwatch = new Stopwatch();

        Run(db =>
        {
            db.Set("a", "test");
            db.Set("b", "test");
            db.Set("c", "test");

            TimeMethod(() => { db.Get("a"); }, "single get with 3");
            TimeMethod(() => { db.Count("test"); }, "count with 3");
            TimeMethod(() => { db.Delete("a"); }, "single delete with 3");
            TimeMethod(() => { db.Set("a", "test2"); }, "single set with 3");

            db.Set("d", "test");
            db.Set("e", "test");
            db.Set("f", "test");
            db.Set("g", "test");
            db.Set("h", "test");
            db.Set("i", "test");
            db.Set("j", "test");
            db.Set("k", "test");
            db.Set("l", "test");
            db.Set("m", "test");
            db.Set("n", "test");
            db.Set("o", "test");
            db.Set("p", "test");
            db.Set("q", "test");
            db.Set("r", "test");
            db.Set("s", "test");
            db.Set("t", "test");
            db.Set("u", "test");
            db.Set("v", "test");
            db.Set("x", "test");
            db.Set("y", "test");
            db.Set("z", "test");

            TimeMethod(() => { db.Get("z"); }, "single get with 26");
            TimeMethod(() => { db.Count("test"); }, "count with 26");
            TimeMethod(() => { db.Delete("a"); }, "single delete with 26");
            TimeMethod(() => { db.Set("a", "test3"); }, "single set with 26");

            //add 1000 more records
            for (var i = 1; i < 1000; i++)
            {
                db.Set(i.ToString(), "test4");
            }

            TimeMethod(() => { db.Get("999"); }, "single get with 1026");
            TimeMethod(() => { db.Count("test"); }, "count with 1026");
            TimeMethod(() => { db.Delete("888"); }, "single delete with 1026");
            TimeMethod(() => { db.Set("888", "test3"); }, "single set with 1026");

            //add 10000 more records
            for (var i = 1; i < 10000; i++)
            {
                db.Set("x" + i.ToString(), "test4");
            }

            TimeMethod(() => { db.Get("9999"); }, "single get with 11026");
            TimeMethod(() => { db.Count("test4"); }, "count with 11026");
            TimeMethod(() => { db.Delete("8888"); }, "single delete with 11026");
            TimeMethod(() => { db.Set("8888", "test3"); }, "single set with 11026");

            db.CreateTransaction();

            for (var i = 1; i < 1000; i++)
            {
                db.Set("t" + i.ToString(), "test4");
            }

            TimeMethod(() => { db.Get("9999"); }, "transaction get with 12026");
            TimeMethod(() => { db.Count("test4"); }, "transaction count with 12026");
            TimeMethod(() => { db.Delete("8888"); }, "transaction delete with 12026");
            TimeMethod(() => { db.Set("8888", "test3"); }, "transaction set with 12026");

            TimeMethod(() => db.Commit(), "transaction commiting 1000 records");

        });
        return true;
    }
}

public class UnitTests
{
    public void Tests()
    {
        var tests = new List<UnitTest>
        {
            new Example1(),
            new Example2(),
            new Example3(),
            new Example4(),
            new Timings()
        };


        foreach (var test in tests)
        {
            var result = test.RunTest();
            System.Console.WriteLine($"{test.GetType().Name} Was {(result ? "Successful" : "Unsuccessful")}");
        }
    }
}