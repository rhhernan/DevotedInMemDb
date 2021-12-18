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

//for some reason i couldn't add a unit test framework to my project
//so i cobbled together a simple testing to test the 4 examples on the sheet
public class UnitTests
{
    public void Tests()
    {
        var tests = new List<UnitTest>
        {
            new Example1(),
            new Example2(),
            new Example3(),
            new Example4()
        };


        foreach (var test in tests)
        {
            var result = test.RunTest();
            System.Console.WriteLine($"{test.GetType().Name} Was {(result ? "Successful" : "Unsuccessful")}");
        }
    }
}