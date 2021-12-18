class Program
{
    public static void Main()
    {
        Console.WriteLine("Welcome To The In-Memory Database");
        Console.WriteLine("Please Enter Your Command Below");
        Console.WriteLine("Type HELP For A List Of Commands");
        Console.WriteLine();

        InMemDb database = new InMemDb();

        string? input = Console.ReadLine();
        while (input != Commands.END)
        {
            if (input == null || String.IsNullOrWhiteSpace(input))
            {
                System.Console.WriteLine("Please Enter A Command Or Type END To Finish");
                input = Console.ReadLine();
                continue;
            }

            //split the input, and remove any blank array spaces
            var split = input.Split(' ').Where(s => !String.IsNullOrWhiteSpace(s)).ToArray();

            switch (split[0].ToUpper())
            {
                case Commands.SET:
                    if (split.Length != 3)
                    {
                        IncorrectArguments(Commands.SET, split.Length, 3);
                        break;
                    }
                    database.Set(split[1], split[2]);
                    break;

                case Commands.GET:
                    if (split.Length != 2)
                    {
                        IncorrectArguments(Commands.GET, split.Length, 2);
                    }
                    //I set null here to not hardcode it in the Get function
                    Console.WriteLine(database.Get(split[1]) ?? "NULL");
                    break;

                case Commands.DELETE:
                    if (split.Length != 2)
                    {
                        IncorrectArguments(Commands.DELETE, split.Length, 2);
                    }
                    database.Delete(split[1]);
                    break;
                case Commands.COUNT:
                    if (split.Length != 2)
                    {
                        IncorrectArguments(Commands.COUNT, split.Length, 2);
                    }
                    Console.WriteLine(database.Count(split[1]));
                    break;

                case Commands.END:
                    System.Console.WriteLine("Application Ending...");
                    Environment.Exit(0);
                    break;

                case Commands.BEGIN_TRANSACTION:
                    database.CreateTransaction();
                    break;

                case Commands.ROLLBACK_TRANSACTION:
                    database.RollbackTransaction();
                    break;

                case Commands.COMMIT_ALL_TRANSACTIONS:
                    database.Commit();
                    break;

                case Commands.HELP:
                    PrintHelp();
                    break;

                case Commands.RUN_TESTS:
                    var unitTest = new UnitTests();
                    unitTest.Tests();
                    break;

                default:
                    System.Console.WriteLine("Unknown Command. Please Type HELP For A List Of Commands Or END To Finish");
                    break;
            }

            input = Console.ReadLine();
        }
    }


    private static void IncorrectArguments(string command, int parameters, int expectedParameters)
    {
        Console.WriteLine($"Invalid Number Of Arguments For Command {command}. Expected {expectedParameters} | Got {parameters}");
    }

    private static void PrintHelp()
    {
        Console.WriteLine("SET [key] [value]");
        Console.WriteLine("GET [key]");
        Console.WriteLine("DELETE [key]");
        Console.WriteLine("COUNT [value]");
        Console.WriteLine("BEGIN");
        Console.WriteLine("ROLLBACK");
        Console.WriteLine("COMMIT");
        Console.WriteLine("END");
        Console.WriteLine();
    }
}